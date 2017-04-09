using R3E.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace R3E
{
    public class R3EMemoryReader : IDisposable
    {
        public EventHandler<Shared> onRead;
        private object sharedLock = new object();
        private bool Mapped
        {
            get { return (_file != null); }
        }

        private Shared _data;
        private MemoryMappedFile _file;
        private byte[] _buffer;

        public TimeSpan TimeInterval = TimeSpan.FromMilliseconds(100);

        private bool running;
        public EventHandler<Exception> onError;
        public EventHandler<bool> onRreRunning;

        private Queue<byte[]> lastShared = new Queue<byte[]>();
        private int MaxLastShared = 100;

        public R3EMemoryReader(int interval)
        {
            TimeInterval = TimeSpan.FromMilliseconds(interval);
        }

        public void Dispose()
        {
            running = false;
            _file?.Dispose();
        }

        public void Run()
        {
            running = true;
            var timeReset = DateTime.UtcNow;
            var timeLast = timeReset;

            Console.WriteLine("Looking for RRRE.exe...");

            while (running)
            {
                var timeNow = DateTime.UtcNow;

                if (timeNow.Subtract(timeLast) < TimeInterval)
                {
                    Thread.Sleep(1);
                    continue;
                }

                timeLast = timeNow;
                bool rreRunning = Utilities.IsRrreRunning();
                onRreRunning?.Invoke(this, rreRunning);
                if (rreRunning && !Mapped)
                {
                    Console.WriteLine("Found RRRE.exe, mapping shared memory...");

                    if (Map())
                    {
                        Console.WriteLine("Memory mapped successfully");
                        timeReset = DateTime.UtcNow;

                        _buffer = new Byte[Marshal.SizeOf(typeof(Shared))];
                    }
                }

                if (Mapped)
                {
                    Read();
                    onError?.Invoke(this, new Exception(""));
                }
                else
                {
                    onError?.Invoke(this, new Exception("Mapping not successfully"));
                }
            }

            Console.WriteLine("All done!");
        }

        private bool Map()
        {
            try
            {
                _file = MemoryMappedFile.OpenExisting(Constant.SharedMemoryName);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        private bool Read()
        {
            try
            {
                var _view = _file.CreateViewStream();
                BinaryReader _stream = new BinaryReader(_view);
                _data = bytesToShared(_stream.ReadBytes(Marshal.SizeOf(typeof(Shared))));
                onRead?.Invoke(this, _data);
                lock (sharedLock)
                {
                    lastShared.Enqueue(_buffer);
                    if (lastShared.Count > MaxLastShared)
                    {
                        lastShared.Dequeue();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                onError?.Invoke(this, e);
                return false;
            }
        }

        private Shared bytesToShared(byte[] bytes)
        {
            GCHandle _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            Shared shared = (Shared)Marshal.PtrToStructure(_handle.AddrOfPinnedObject(), typeof(Shared));
            _handle.Free();
            return shared;
        }

        public byte[] LastMs(int ms)
        {
            short number = (short) Math.Min(ms / TimeInterval.TotalMilliseconds + 1, lastShared.Count);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memoryStream))
                {
                    // header information for lap
                    // version
                    writer.Write((byte)1);

                    // date
                    writer.Write(DateTime.Now.ToBinary());

                    // poll time
                    writer.Write((int) TimeInterval.TotalMilliseconds);

                    // number of shared objects
                    writer.Write(number);

                    lock (sharedLock)
                    {
                        for (int i = 0; i < number; i++)
                        {
                            writer.Write(lastShared.Dequeue());
                        }
                    }

                }

                return memoryStream.ToArray();
            }
        }

        public void Play(byte[] bytes)
        {
            // stop reading memory
            running = false;
            using (var ms = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(ms))
                {
                    byte version = reader.ReadByte();
                    DateTime date = new DateTime(reader.ReadInt64());
                    int pollTime = reader.ReadInt32();
                    int count = reader.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        byte[] sBytes = reader.ReadBytes(Marshal.SizeOf(typeof(Shared)));
                        Shared shared = bytesToShared(sBytes);
                        onRead?.Invoke(this, shared);
                    }
                }
            }
        }

        
    }

    
}