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
using R3E.Model;

namespace R3E
{
    public class R3EServer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsSending { get; private set; }
        public int dstPort;
        public string ipaddress;
        private Timer timer;
        private DateTime lastSent;

        public R3EServer(int port, string ip)
        {
            this.ipaddress = ip;
            this.dstPort = port;
        }

        private void connectionCheck(object state)
        {
            if (lastSent == null)
            {
                IsSending = false;
            }
            else if (DateTime.Now.Subtract(lastSent).TotalSeconds >= 1)
            {
                IsSending = false;
            }
            else
            {
                IsSending = true;
            }
        }

        public void SendMessage(DisplayData data)
        {
            short version = 0;
            byte type = data.ToType();
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(version);
                    bw.Write(type);
                    data.Write(bw);
                    byte[] bytes = ms.ToArray();
                    using (UdpClient c = new UdpClient())
                    {
                        try
                        {
                            int sentBytes = c.Send(bytes, bytes.Length, ipaddress, dstPort);
                            lastSent = DateTime.Now;
                            if (sentBytes != bytes.Length)
                            {
                                log.Error("Sent less byte than expected");
                            }
                        }
                        catch(Exception e)
                        {
                            log.Error("Error while sending bytes: " + e.Message);
                        }
                    }
                }
            }
        }

        public void Start()
        {
            timer = new Timer(connectionCheck, null, 0, 1000);
        }

        public void Stop()
        {
            timer.Dispose();
            timer = null;
        }

    }
}
