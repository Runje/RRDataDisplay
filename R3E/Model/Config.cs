using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class Config
    {
        const int Default_Port = 56678;
        const int Default_SendInterval = 100;
        const int Default_PollInterval = 100;
        const string Default_IP = "192.168.2.255";

        private int port = Default_Port;
        public int Port
        {
            get
            {
                return port;
            }

            set
            {
                if (validatePort(value))
                {
                    port = value;
                }
            }
        }

        private string ip = Default_IP;

        public string IP
        {
            get { return ip; }
            set
            {
                if (validateIP(value))
                {
                    ip = value;
                }
            }
        }

        private int sendInterval  = Default_SendInterval;

        public int SendInterval
        {
            get { return sendInterval; }
            set
            {
                if (validateInterval(value))
                {
                    sendInterval = value;
                }
            }
        }

        private int pollInterval = Default_PollInterval;

        public int PollInterval
        {
            get { return pollInterval; }
            set
            {
                if (validateInterval(value))
                {
                    pollInterval = value;
                }
            }
        }


        public void Read()
        {
            Port = Properties.Settings.Default.port;
            IP = Properties.Settings.Default.ip;
            SendInterval = Properties.Settings.Default.sendIntervalInMs;
            PollInterval = Properties.Settings.Default.pollIntervalInMs;
        }

        public static bool validateIP(string ip)
        {
            IPAddress i;
            return IPAddress.TryParse(ip, out i);
        }

        public static bool validateInterval(int interval)
        {
            return interval >= 10 && interval < 5000;
        }

        public static bool validatePort(int p)
        {
            return p > 1024 && p < 65535;
        }
    }
}
