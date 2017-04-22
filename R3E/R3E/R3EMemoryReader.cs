using R3E.Data;
using R3E.Model;
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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public int MaxStoreInSec { get; set; }
        public EventHandler<Shared> onRead;
        public EventHandler<double> onEndTime;
        public EventHandler<double> onChangeTime;
        private object sharedLock = new object();
        private bool Mapped
        {
            get { return (_file != null); }
        }

        /// <summary>
        /// Save so many last shared that the MaxStoreInSec can be saved.
        /// </summary>
        public int MaxLastShared { get { return (int) (MaxStoreInSec / TimeInterval.TotalSeconds + 1); } }

        private Shared _data;
        private MemoryMappedFile _file;
        private byte[] _buffer;

        public TimeSpan TimeInterval = TimeSpan.FromMilliseconds(100);

        private bool running;
        public EventHandler<Exception> onError;
        public EventHandler<bool> onRreRunning;

        private Queue<byte[]> lastShared = new Queue<byte[]>();
        private bool fastForward;
        private bool playing;
        private Shared _lastData;

        public R3EMemoryReader(int interval)
        {
            TimeInterval = TimeSpan.FromMilliseconds(interval);
            MaxStoreInSec = 600;
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
                        _lastData = new Shared();
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
                _buffer = _stream.ReadBytes(Marshal.SizeOf(typeof(Shared)));
                _data = bytesToShared(_buffer);
                
                lock (sharedLock)
                {
                    if (!(_data.Player.GameSimulationTime == _lastData.Player.GameSimulationTime || _data.SessionType == DisplayData.INVALID_INT))
                    {
                        lastShared.Enqueue(_buffer);
                        if (lastShared.Count > MaxLastShared)
                        {
                            lastShared.Dequeue();
                        }
                    }
                }
                onRead?.Invoke(this, _data);
                _lastData = _data;
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
            GCHandle _handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            Shared shared = (Shared)Marshal.PtrToStructure(_handle.AddrOfPinnedObject(), typeof(Shared));
            _handle.Free();
            return shared;
        }

        public byte[] LastMs(int ms)
        {
            lock (sharedLock)
            {
                short number = (short)Math.Min(ms / TimeInterval.TotalMilliseconds + 1, lastShared.Count);

                // dismiss all saved shares before:
                while(lastShared.Count > number)
                {
                    lastShared.Dequeue();
                }

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
                        writer.Write((int)TimeInterval.TotalMilliseconds);

                        // number of shared objects
                        writer.Write(number);


                        for (int i = 0; i < number; i++)
                        {
                            writer.Write(lastShared.Dequeue());
                        }

                    }

                    return memoryStream.ToArray();
                }
            }
        }

        public void Play(byte[] bytes)
        {
            // TODO: make own class R3EMemoryPlayer
            // stop reading memory
            running = false;
            fastForward = false;
            log.Info("Playing bytes: " + bytes.Length);
            using (var ms = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(ms))
                {
                    byte version = reader.ReadByte();
                    DateTime date = DateTime.FromBinary(reader.ReadInt64());
                    int pollTime = reader.ReadInt32();
                    short count = reader.ReadInt16();
                    double endTime = pollTime * count / 1000.0;
                    onEndTime?.Invoke(this, endTime);
                    for (int i = 0; i < count; i++)
                    {
                        while(!playing)
                        {
                            Thread.Sleep(1);
                        }
                        byte[] sBytes = reader.ReadBytes(Marshal.SizeOf(typeof(Shared)));
                        Shared shared = bytesToShared(sBytes);
                        onRead?.Invoke(this, shared);
                        onChangeTime?.Invoke(this, i * pollTime / 1000.0);
                        if (!fastForward)
                        {
                            Thread.Sleep(pollTime);
                        }
                    }
                }
            }

            log.Info("Played bytes: " + bytes.Length);
        }

        internal void Resume()
        {
            fastForward = false;
            playing = true;
        }

        internal void Forward()
        {
            fastForward = true;
        }

        internal void Pause()
        {
            playing = false;
        }
    }

    
}