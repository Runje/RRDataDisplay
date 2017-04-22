using R3E.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class P1
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Config config = new Config();
        private R3EMemoryReader memoryReader;
        private R3EServer r3eServer;
        private R3EView view;
        private Timer timer;
        private DataModel model;
        private bool playing;

        public P1(R3EView view)
        {
            this.view = view;
        }

        public void Start()
        {
            config.Read();
            model = new DataModel();
            model.OnLapCompleted += onLapCompleted;
            memoryReader = new R3EMemoryReader(config.PollInterval);
            r3eServer = new R3EServer(config.Port, config.IP);
            memoryReader.onRead += onRead;
            memoryReader.onError += onError;
            memoryReader.onRreRunning += onRreRunning;
            memoryReader.onRead += onRead;
            memoryReader.onEndTime += onEndTime;
            memoryReader.onChangeTime += onChangeTime;
            new Thread(() => memoryReader.Run()).Start();
            timer = new Timer(sendMessage, null, 0, config.SendInterval);
            view.UpdateConfig(config);
        }

        private void onChangeTime(object sender, double e)
        {
            view.onChangeTime(e);
        }

        private void onEndTime(object sender, double e)
        {
            view.onEndTime(e);
        }


        private void onLapCompleted(object sender, LapInfo lap)
        {
            if (!playing)
            {
                byte[] bytes = memoryReader.LastMs((int)(lap.LapTime * 1000 + 1000));

                string dir = System.IO.Path.Combine(lap.Track, lap.Start.ToString("yy_MM_dd_HH_mm"));
                Directory.CreateDirectory(dir);

                var filename = System.IO.Path.Combine(dir, lap.Session + lap.LapCount + "_" + lap.LapTime.ToString() + ".lap");
                File.WriteAllBytes(filename, bytes);
                log.Info("Wrote file " + filename);
            }

        }

        private void sendMessage(object state)
        {
            r3eServer.SendMessage(model.ActualData);
        }

        public void Stop()
        {
            memoryReader.onRead -= onRead;
            memoryReader.onError -= onError;
            memoryReader.onRreRunning -= onRreRunning;
            memoryReader.Dispose();
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
                model.UpdateFromR3E(shared);
                r3eServer.SendMessage(model.ActualData);
                view.ShowSharedData(shared);
                view.ShowDisplayData(model.ActualData);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                view.ShowConnectionError(e.Message);
            }

            
        }

        internal void ChangePollInterval(int interval)
        {
            config.PollInterval = interval;
            memoryReader.TimeInterval = TimeSpan.FromMilliseconds(config.PollInterval);
        }

        internal void ChangeIp(string ip)
        {
            config.IP = ip;
            r3eServer.ipaddress = config.IP;
        }

        internal void ChangePort(int p)
        {
            config.Port = p;
            r3eServer.dstPort = config.Port;
        }

        internal void PlayFiles(string[] fileNames)
        {
            new Thread(() =>
            {
                foreach (var file in fileNames)
                {
                    playing = true;
                    model.ResetAll();
                    
                    byte[] bytes = File.ReadAllBytes(file);
                    memoryReader.Play(bytes);
                }

                playing = false;

            }).Start();
        }

        internal void Pause()
        {
            memoryReader.Pause();
        }

        internal void Forward()
        {
            memoryReader.Forward();
        }

        internal void Resume()
        {
            memoryReader.Resume();
        }

        internal byte[] Save()
        {
            return memoryReader.LastMs(600 * 1000);
            
        }
    }

    
}
