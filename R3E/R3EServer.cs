using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R3E.Data;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

namespace R3E
{
    public class R3EServer
    {
        private R3EMemoryReader memoryReader;
        public int dstPort;
        public string ipaddress;
        private R3EView view;
        private Timer timer;
        private DateTime lastSent;

        public R3EServer(int port, string ip, R3EMemoryReader reader, R3EView view)
        {
            this.ipaddress = ip;
            this.dstPort = port;
            this.view = view;
            memoryReader = reader;
        }

        private void connectionCheck(object state)
        {
            if (lastSent == null)
            {
                view.ShowSending(false);
            }
            else if (DateTime.Now.Subtract(lastSent).TotalSeconds >= 1)
            {
                view.ShowSending(false);
            }
            else
            {
                view.ShowSending(true);
            }
        }

        private void onRreRunning(object sender, bool e)
        {
            view.ShowRreRunning(e);
        }

        private void onError(object sender, Exception e)
        {
            view.ShowMappingError(e.Message);
        }

        private void onRead(object sender, Shared shared)
        {
            try
            {
                sendMessage(shared);
                lastSent = DateTime.Now;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                view.ShowConnectionError(e.Message);
            }

            view.ShowData(shared);
        }

        private void sendMessage(Shared shared)
        {
            R3EMessage msg = new R3EMessage(shared);
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    msg.Write(bw);
                    byte[] bytes = ms.ToArray();
                    using (UdpClient c = new UdpClient())
                    {
                        int sentBytes = c.Send(bytes, bytes.Length, ipaddress, dstPort);
                        if (sentBytes != bytes.Length)
                        {
                            view.ShowConnectionError("Sent less bytes than expected");
                        }
                    }
                }
            }
        }

        public void Start()
        {
            memoryReader.onRead += onRead;
            memoryReader.onError += onError;
            memoryReader.onRreRunning += onRreRunning;
            new Thread(() => memoryReader.Run()).Start();
            timer = new Timer(connectionCheck, null, 0, 1000);
        }

        public void Stop()
        {
            memoryReader.onRead -= onRead;
            memoryReader.onError -= onError;
            memoryReader.onRreRunning -= onRreRunning;
            memoryReader.Dispose();
            timer.Dispose();
            timer = null;
        }

        internal void ChangeInterval(int interval)
        {
            memoryReader.TimeInterval = TimeSpan.FromMilliseconds(interval);
        }
    }
}
