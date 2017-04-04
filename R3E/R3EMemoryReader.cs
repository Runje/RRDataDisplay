using R3E.Data;
using System;
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
                _buffer = _stream.ReadBytes(Marshal.SizeOf(typeof(Shared)));
                
                GCHandle _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
                _data = (Shared)Marshal.PtrToStructure(_handle.AddrOfPinnedObject(), typeof(Shared));
                _handle.Free();
                onRead?.Invoke(this, _data);
                return true;
            }
            catch (Exception e)
            {
                onError?.Invoke(this, e);
                return false;
            }
        }

        
    }

    
}