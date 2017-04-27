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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DataModel model;
        private Dashboard dashboard;
        private P1 p1;

        public MainWindow()
        {
            log.Info("START");
            dashboard = new Dashboard();
            var secondaryScreen = System.Windows.Forms.Screen.AllScreens.Where(s => !s.Primary).FirstOrDefault();
            
            if (secondaryScreen != null)
            {
                var workingArea = secondaryScreen.WorkingArea;
                dashboard.Left = workingArea.Right - dashboard.Width;
                dashboard.Top = workingArea.Top;
            }
            dashboard.Show();
            
            InitializeComponent();
            
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            p1 = new P1(this);
        }

        

        public void onChangeTime(double e)
        {
            Dispatcher.Invoke(() =>
            {
                textPlaytime.Content = e.ToString();
                sliderPlay.Value = e;
            });
        }

        public void onEndTime(double e)
        {
            Dispatcher.Invoke(() =>
            {
                textMaxPlaytime.Content = e.ToString();
                sliderPlay.Maximum = e;
            });
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            p1.Stop();
            dashboard.Close();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            p1.Start();
        }

        public void ShowConnectionError(string message)
        {
            Dispatcher.Invoke(() => textSendError.Content = message);
        }

        public void ShowSharedData(Shared shared)
        {
            //Dispatcher.Invoke(() => sharedToGUI(shared));
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

            if (Config.validateInterval(interval))
            {
                textInterval.Foreground = Brushes.Black;
                p1?.ChangePollInterval(interval);
            }
            else
            {
                textInterval.Foreground = Brushes.Red;
            }
        }

        

        private void textIp_TextChanged(object sender, TextChangedEventArgs e)
        {
            string ip = textIp.Text;

            if (Config.validateIP(ip))
            {
                textIp.Foreground = Brushes.Black;
                p1?.ChangeIp(ip);
            }
            else
            {
                textIp.Foreground = Brushes.Red;
            }
        }

        

        private void textR3EPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            int p = 0;
            Int32.TryParse(textR3EPort.Text, out p);

            if (Config.validatePort(p))
            {
                textR3EPort.Foreground = Brushes.Black;
                p1?.ChangePort(p);
            }
            else
            {
                textR3EPort.Foreground = Brushes.Red;
            }
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
                p1.PlayFiles(openFileDialog.FileNames);
                
            }
            
        }

        private void buttonRewind_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            p1.Pause();
        }

        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            p1.Resume();
        }

        private void buttonForward_Click(object sender, RoutedEventArgs e)
        {
            p1.Forward();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var bytes = p1.Save();
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.InitialDirectory = Environment.CurrentDirectory;
            saveFile.ShowDialog();
            if (saveFile.FileName != "")
            {
                File.WriteAllBytes(saveFile.FileName, bytes);
            }
        }

        public void UpdateConfig(Config config)
        {
            textR3EPort.Text = config.Port.ToString();
            textInterval.Text = config.PollInterval.ToString();
            textIp.Text = config.IP;
        }

        public void ShowDisplayData(DisplayData actualData)
        {
            dashboard.Update(actualData);
        }

        private void ButtonLive_Click(object sender, RoutedEventArgs e)
        {
            p1.GoToLiveMode();
        }
    }
}
