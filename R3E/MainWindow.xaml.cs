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

namespace R3E
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, R3EView
    {
        private R3EServer r3eServer;

        public MainWindow()
        {
            
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

            
            InitializeComponent();
            R3EMemoryReader r3EMemoryReader = new R3EMemoryReader(interval);
            r3eServer = new R3EServer(port, ip, r3EMemoryReader, this);
            var model = new DataModel();
            r3EMemoryReader.onRead += (t, s) => model.UpdateFromR3E(s);
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            textR3EPort.Text = port.ToString();
            textInterval.Text = interval.ToString();
            textIp.Text = ip;

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
    }
}
