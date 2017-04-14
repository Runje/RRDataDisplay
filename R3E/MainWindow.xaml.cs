using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using R3E.Data;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;
using R3E.Model;
using System.IO;
using Microsoft.Win32;


namespace R3E
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, R3EView
    {
        private R3EServer r3eServer;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private R3EMemoryReader r3EMemoryReader;
        private DataModel model;
        private Dashboard dashboard;
        private bool playing;

        public MainWindow()
        {
            log.Info("START");
            dashboard = new Dashboard();
            dashboard.Show();
            int port = Properties.Settings.Default.port;
            if (!validatePort(port))
            {
                port = 56678;
            }

            string ip = Properties.Settings.Default.ip;

            if (!validateIP(ip))
            {
                ip = "192.168.0.255";
            }

            int interval = Properties.Settings.Default.intervalInMs;

            if (!validateInterval(interval))
            {
                interval = 100;
            }

            
            r3EMemoryReader = new R3EMemoryReader(interval);
            r3eServer = new R3EServer(port, ip, r3EMemoryReader, this);
            InitializeComponent();
            model = new DataModel();
            model.OnLapCompleted += onLapCompleted;
            r3EMemoryReader.onRead += onRead;
            r3EMemoryReader.onEndTime += onEndTime;
            r3EMemoryReader.onChangeTime += onChangeTime;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            textR3EPort.Text = port.ToString();
            textInterval.Text = interval.ToString();
            textIp.Text = ip;

        }

        private void onRead(object sender, Shared shared)
        {
            model.UpdateFromR3E(shared);
            dashboard.Update(model.ActualData);
        }

        private void onChangeTime(object sender, double e)
        {
            Dispatcher.Invoke(() =>
            {
                textPlaytime.Content = e.ToString();
                sliderPlay.Value = e;
            });
        }

        private void onEndTime(object sender, double e)
        {
            Dispatcher.Invoke(() =>
            {
                textMaxPlaytime.Content = e.ToString();
                sliderPlay.Maximum = e;
            });
        }

        private void onLapCompleted(object sender, LapInfo lap)
        {
            if (!playing)
            {
                byte[] bytes = r3EMemoryReader.LastMs((int)(lap.LapTime* 1000));

                string dir = System.IO.Path.Combine(lap.Track, lap.Start.ToString("yy_MM_dd_HH_mm"));
                Directory.CreateDirectory(dir);

                var filename = System.IO.Path.Combine(dir, lap.Session + lap.LapCount + "_" + lap.LapTime.ToString() + ".lap");
                File.WriteAllBytes(filename, bytes);
                log.Info("Wrote file " + filename);
            }

        }

        private void sharedToGUI(Shared shared)
        {
            // Omitted: car location, cardamage
            StringBuilder sharedText = new StringBuilder();
            sharedText.AppendLine(addTag("Name") + Utilities.byteToString(shared.PlayerName));
            sharedText.AppendLine(addTag("Track") + Utilities.byteToString(shared.TrackName));
            sharedText.AppendLine(addTag("Layout") + Utilities.byteToString(shared.LayoutName));
            sharedText.AppendLine(addTag("SelfBestLap") + Utilities.floatToString(shared.LapTimeBestSelf));
            sharedText.AppendLine(addTag("OverallBestLap") + Utilities.floatToString(shared.LapTimeBestLeaderClass));
            sharedText.AppendLine(addTag("Current Sector1") + Utilities.floatToString(shared.SectorTimesCurrentSelf.AbsSector1));
            sharedText.AppendLine(addTag("Current Sector2") + Utilities.floatToString(shared.SectorTimesCurrentSelf.AbsSector2));
            sharedText.AppendLine(addTag("Current Sector3") + Utilities.floatToString(shared.SectorTimesCurrentSelf.AbsSector3));
            for (int i = 0; i < shared.DriverData.Length; i++)
            {
                var driver = shared.DriverData[i];
                if (driver.DriverInfo.CarNumber != -1)
                {
                    sharedText.AppendLine(addTag(Utilities.byteToString(driver.DriverInfo.Name)) + Utilities.floatToString(driver.SectorTimeBestSelf.AbsSector1));
                }
            }

            sharedText.AppendLine(addTag("LapDistanceFraction") + Utilities.floatToString(shared.LapDistanceFraction));
            sharedText.AppendLine(addTag("LapTimeDeltaLeader") + Utilities.floatToString(shared.LapTimeDeltaLeader));
            sharedText.AppendLine(addTag("TrackSector") + shared.TrackSector);

            textShared.Text = sharedText.ToString();
        }

        private string addTag(string tag)
        {
            string spaces = " ";
            for (int i = 0; i < 10; i++)
            {
                spaces += " ";
            }

            return tag + ":" + spaces;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            r3eServer.Stop();
            dashboard.Close();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            r3eServer.Start();
        }

        public void ShowConnectionError(string message)
        {
            Dispatcher.Invoke(() => textSendError.Content = message);
        }

        public void ShowData(Shared shared)
        {
            Dispatcher.Invoke(() => sharedToGUI(shared));
        }

        public void ShowMappingError(string message)
        {
            Dispatcher.Invoke(() => textMemoryError.Content = message);
        }

        public void ShowRreRunning(bool e)
        {
            Dispatcher.Invoke(() =>
            {
                textR3ERunning.Content = e ? "R3E is running" : "R3E is not running";
                textR3ERunning.Foreground = e ? Brushes.Green : Brushes.Red;
            });
        }

        private void textInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            int interval = 0;
            Int32.TryParse(textInterval.Text, out interval);

            if (validateInterval(interval))
            {
                textInterval.Foreground = Brushes.Black;
                r3eServer.ChangeInterval(interval);
            }
            else
            {
                textInterval.Foreground = Brushes.Red;
            }
        }

        private bool validateInterval(int interval)
        {
            return interval > 10 && interval < 5000;
        }

        private void textIp_TextChanged(object sender, TextChangedEventArgs e)
        {
            string ip = textIp.Text;
            if (validateIP(ip))
            {
                textIp.Foreground = Brushes.Black;
                r3eServer.ipaddress = ip;
            }
            else
            {
                textIp.Foreground = Brushes.Red;
            }
        }

        private bool validateIP(string ip)
        {
            IPAddress i;
            return IPAddress.TryParse(ip, out i);
        }

        private void textR3EPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            int p = 0;
            Int32.TryParse(textR3EPort.Text, out p);

            if (validatePort(p))
            {
                textR3EPort.Foreground = Brushes.Black;
                r3eServer.dstPort = p;
            }
            else
            {
                textR3EPort.Foreground = Brushes.Red;
            }
        }

        private bool validatePort(int p)
        {
            return p > 1024 && p < 65535;
        }

        public void ShowSending(bool v)
        {
            Dispatcher.Invoke(() =>
            {
                textR3eSending.Content = v ? "Sending" : "Not sending";
                textR3eSending.Foreground = v ? Brushes.Green : Brushes.Red;
            });
        }

        private void buttonLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                
                new Thread(() =>
                {
                    var files = openFileDialog.FileNames;
                    foreach (var file in files)
                    {
                        playing = true;
                        model.ResetAll();
                        Dispatcher.Invoke(() =>
                        {
                            textFile.Content = file.ToString();
                        });
                        byte[] bytes = File.ReadAllBytes(file);
                        r3EMemoryReader.Play(bytes);
                    }

                    playing = false;
                    
                }).Start();
            }
            
        }

        private void buttonRewind_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            r3EMemoryReader.Pause();
        }

        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            r3EMemoryReader.Resume();
        }

        private void buttonForward_Click(object sender, RoutedEventArgs e)
        {
            r3EMemoryReader.Forward();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            byte[] bytes = r3EMemoryReader.LastMs(600 * 1000);
            File.WriteAllBytes(DateTime.Now.ToShortDateString(), bytes);
        }
    }
}
