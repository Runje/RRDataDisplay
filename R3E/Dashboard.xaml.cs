using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using R3E.Model;

namespace R3E
{
    /// <summary>
    /// Interaktionslogik für Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        internal void Update(DisplayData actualData)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateGUI(actualData);
            });
        }

        private void UpdateGUI(DisplayData actualData)
        {
            StringBuilder sharedText = new StringBuilder();
            sharedText.AppendLine(addTag("Current Lap Time") + Utilities.floatLapToString(actualData.CurrentTime));
            sharedText.AppendLine(addTag("Sector1") + Utilities.floatLapToString(actualData.CurrentLap.Sector1) + " " + Utilities.floatLapToString(actualData.TBLap.Sector1));
            sharedText.AppendLine(addTag("Sector2") + Utilities.floatLapToString(actualData.CurrentLap.RelSector2) + " " + Utilities.floatLapToString(actualData.TBLap.RelSector2));
            sharedText.AppendLine(addTag("Sector3") + Utilities.floatLapToString(actualData.CurrentLap.RelSector3) + " " + Utilities.floatLapToString(actualData.TBLap.RelSector3));
            sharedText.AppendLine(addTag("Position") + actualData.Position);
            sharedText.AppendLine(addTag("Diff to leader") + Utilities.floatLapToString(actualData.FastestLap.Time - actualData.PBLap.Time));
            sharedText.AppendLine(addTag("TrackSector") + actualData.CurrentSector);
            sharedText.AppendLine(addTag("Completed Laps") + actualData.CompletedLapsCount);

            //Qualy data
            if (actualData is QualyData)
            {
                QualyData qualyData = actualData as QualyData;
                sharedText.AppendLine(addTag("Sector1 Position") + qualyData.SectorPos[0]);
                sharedText.AppendLine(addTag("Sector2 Position") + qualyData.SectorPos[1]);
                sharedText.AppendLine(addTag("Sector3 Position") + qualyData.SectorPos[2]);
                sharedText.AppendLine(addTag("Lap Position") + qualyData.SectorPos[3]);
            }

            textDisplay.Text = sharedText.ToString();
        }

        private string addTag(string tag)
        {
            string spaces = " ";
            for (int i = 0; i < 15; i++)
            {
                spaces += " ";
            }

            return tag + ":" + spaces;
        }
    }
}
