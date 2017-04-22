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
using System.Diagnostics;
using System.Drawing;

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
            sharedText.AppendLine(addTag("Lap") + Utilities.floatLapToString(actualData.CurrentLap.Time) + " " + Utilities.floatLapToString(actualData.TBLap.Time));
            sharedText.AppendLine(addTag("Position") + actualData.Position);
            sharedText.AppendLine(addTag("Diff to leader") + Utilities.floatLapToString(actualData.PBLap.Time - actualData.FastestLap.Time));
            sharedText.AppendLine(addTag("TrackSector") + actualData.CurrentSector);
            sharedText.AppendLine(addTag("Completed Laps") + actualData.CompletedLapsCount);
            sharedText.AppendLine(addTag("Fuel last lap") + actualData.FuelLastLap);
            sharedText.AppendLine(addTag("Fuel max lap") + actualData.FuelMaxLap);
            sharedText.AppendLine(addTag("Fuel average per lap") + actualData.FuelAveragePerLap);
            sharedText.AppendLine(addTag("Fuel laps") + actualData.FuelRemainingLaps);
            sharedText.AppendLine(addTag("Tire") + actualData.TiresWear);
            sharedText.AppendLine(addTag("Tire last lap") + actualData.TireUsedLastLap);
            sharedText.AppendLine(addTag("Tire average per lap") + actualData.TireUsedAveragePerLap);
            sharedText.AppendLine(addTag("Tire max per lap") + actualData.TireUsedMaxLap);

            //Qualy data
            if (actualData is QualyData)
            {
                QualyData qualyData = actualData as QualyData;
                sharedText.AppendLine(addTag("Sector1 Position") + qualyData.SectorPos[0]);
                sharedText.AppendLine(addTag("Sector2 Position") + qualyData.SectorPos[1]);
                sharedText.AppendLine(addTag("Sector3 Position") + qualyData.SectorPos[2]);
                sharedText.AppendLine(addTag("Lap Position") + qualyData.SectorPos[3]);
            }
            else if (actualData is RaceData)
            {
                RaceData raceData = actualData as RaceData;
                sharedText.AppendLine(addTag("Laps") + raceData.CompletedLapsCount + "/" + raceData.EstimatedRaceLaps);
                sharedText.AppendLine(addTag("Fuel to refill") + raceData.FuelToRefill);
                sharedText.AppendLine(addTag("Sector1 Diff") + sectorToString(raceData.DiffSectorsAhead.Sector1) + " " + sectorToString(raceData.DiffSectorsBehind.Sector1));
                sharedText.AppendLine(addTag("Sector2 Diff") + sectorToString(raceData.DiffSectorsAhead.RelSector2) + " " + sectorToString(raceData.DiffSectorsBehind.RelSector2));
                sharedText.AppendLine(addTag("Sector3 Diff") + sectorToString(raceData.DiffSectorsAhead.RelSector3) + " " + sectorToString(raceData.DiffSectorsBehind.RelSector3));
                sharedText.AppendLine(addTag("Lap Diff") + sectorToString(raceData.DiffSectorsAhead.Time) + " " + sectorToString(raceData.DiffSectorsBehind.Time));

            }
            textDisplay.Text = sharedText.ToString();
        }

        private string sectorToString(float sec)
        {
            if (sec == DisplayData.INVALID)
            {
                return "-";
            }

            return sec.ToString("0.000");
        }

        private string addTag(string tag)
        {
            string spaces = " ";
            for (int i = tag.Length + 1; i < 20; i++)
            {
                spaces += " ";
            }

            var result = tag + ":" + spaces;
            return result;
        }
    }
}
