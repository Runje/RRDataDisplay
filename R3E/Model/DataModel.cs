using R3E.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static R3E.Constant;

namespace R3E.Model
{
    public class DataModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private object dataLock = new object();

        /// <summary>
        /// EventHandler for completed Laps
        /// </summary>
        public EventHandler<LapInfo> OnLapCompleted;

        /// <summary>
        /// The actual data to display.
        /// </summary>
        public DisplayData ActualData { get; set; }

        public RaceData RaceData { get { return (RaceData)ActualData; } }

        public QualyData QualyData { get { return (QualyData)ActualData; } }

        /// <summary>
        /// The actual shared data from R3E.
        /// </summary>
        public Shared ActualShared { get; private set; }
        
        /// <summary>
        /// The last shared data from R3E.
        /// </summary>
        public Shared LastShared;

        /// <summary>
        /// Whether it is the first update.
        /// </summary>
        private bool IsFirst = true;

        /// <summary>
        /// Whether it is the first sector driven --> invalid current sector time.
        /// </summary>
        private bool IsFirstSector = true;

        /// <summary>
        /// DateTime of start of the event.
        /// </summary>
        private DateTime StartOfEvent;

        public DataModel()
        {
            ResetAll();
        }

        /// <summary>
        /// Resets all values of ActualData to invalid data.
        /// </summary>
        public void ResetAll()
        {
            ActualData = new DisplayData();
            
            IsFirst = true;
            LastShared.TrackSector = DisplayData.INVALID_INT;
            LastShared.SessionType = DisplayData.INVALID_INT;
        }

        public void UpdateFromR3E(Shared shared)
        {
            lock (dataLock)
            {
                if (shared.Player.GameSimulationTime == LastShared.Player.GameSimulationTime)
                {
                    // game is paused
                    return;
                }

                ActualShared = shared;

                if (isStartOfNewSession())
                {
                    log.Debug("Start of new session " + LastShared.SessionType + " --> " + shared.SessionType);
                    if (LastShared.SessionType == DisplayData.INVALID_INT)
                    {
                        log.Debug("Start of new event");
                        // new event
                        StartOfEvent = DateTime.Now;
                    }

                    if (IsRace())
                    {
                        ActualData = new RaceData();
                    }
                    else
                    {
                        ActualData = new QualyData();
                    }

                    // TODO: Load TB, Fuel and Tire usage
                }


                ActualData.CurrentTime = shared.LapTimeCurrentSelf;
                ActualData.CurrentLap = R3EUtils.SectorsToLap(shared.SectorTimesCurrentSelf);

                updateCurrentLap();
                // TODO: Calculate Sector Times with help of ActualShared.Player.GameTime to show time on invalid lap
                ActualData.PBLap = R3EUtils.SectorsToLap(shared.SectorTimesBestSelf);
                ActualData.FastestLap = R3EUtils.SectorsToLap(shared.SectorTimesSessionBestLap);

                ActualData.Position = shared.Position;
                ActualData.CurrentSector = (shared.TrackSector + 2) % 3;
                ActualData.CompletedLapsCount = shared.CompletedLaps;
                ActualData.Track = Utilities.byteToString(ActualShared.TrackName);
                ActualData.Layout = Utilities.byteToString(ActualShared.LayoutName);

                if (isNewSector())
                {
                    log.Debug("Sector change: " + (LastShared.TrackSector + 2) % 3 + " --> " + (ActualShared.TrackSector + 2) % 3);
                    if (LastShared.TrackSector != DisplayData.INVALID_INT)
                    {
                        IsFirstSector = false;
                    }

                    UpdateCompletedAndValidSectors();
                    UpdateTB();
                    if (!IsRace())
                    {
                        UpdateCurrentSectorPositions();
                    }
                }

                

                if (isNewStint() && !IsRace())
                {
                    log.Debug("Start of new stint");
                }

                if (isStartOfNewLap())
                {
                    log.Debug("Start of new Lap, previous: " + LastShared.CompletedLaps + ", actual: " + ActualShared.CompletedLaps);
                    if (ActualShared.LapTimePreviousSelf != DisplayData.INVALID_POSITIVE)
                    {
                        string session = "-";
                        switch (ActualShared.SessionType)
                        {
                            case 0:
                                session = "P";
                                break;
                            case 1:
                                session = "P";
                                break;
                            case 2:
                                session = "R";
                                break;
                        }

                        OnLapCompleted?.Invoke(this, new LapInfo(ActualShared.LapTimePreviousSelf, ActualData.CompletedLapsCount, session, ActualData.Track, StartOfEvent));
                    }
                }



                LastShared = shared;
                IsFirst = false;
            }
        }

        /// <summary>
        /// Calculate positions of current sectors
        /// </summary>
        private void UpdateCurrentSectorPositions()
        {
            float[] currentSec = ActualData.CurrentLap.RelArray;
            for (int i = 0; i < 4; i++)
            {

                float current = currentSec[i];
                int pos = 1;
                if (current == DisplayData.INVALID_POSITIVE || !ActualData.CurrentLapCompletedAndValid[i])
                {
                    // no valid time
                    pos = -1;
                }
                else
                {
                    // count faster cars (current lap or best lap)
                    for (int j = 0; j < ActualShared.NumCars; j++)
                    {
                        DriverData driver = ActualShared.DriverData[j];
                        float driverCur = R3EUtils.SectorsToLap(driver.SectorTimeCurrentSelf).RelArray[i];
                        float driverBest = R3EUtils.SectorsToLap(driver.SectorTimeBestSelf).RelArray[i];
                        if (driverCur != DisplayData.INVALID_POSITIVE && driverCur < current || driverBest != DisplayData.INVALID_POSITIVE && driverBest < current)
                        {
                            pos++;
                        }
                    }
                }

                QualyData.SectorPos[i] = pos;
            }

            // show on lap pos estimated position, if 3. sector is not over yet
            if (currentSec[2] == DisplayData.INVALID_POSITIVE)
            {
                if (currentSec[1] == DisplayData.INVALID_POSITIVE)
                {
                    // use pos from first time
                    QualyData.SectorPos[3] = QualyData.SectorPos[0];
                }
                else
                {
                    int pos = 1;
                    float current = ActualData.CurrentLap.AbsSector2;
                    for (int j = 0; j < ActualShared.NumCars; j++)
                    {
                        DriverData driver = ActualShared.DriverData[j];
                        float driverCur = R3EUtils.SectorsToLap(driver.SectorTimeCurrentSelf).AbsSector2;
                        float driverBest = R3EUtils.SectorsToLap(driver.SectorTimeBestSelf).AbsSector2;
                        if (driverCur != DisplayData.INVALID_POSITIVE && driverCur < current || driverBest != DisplayData.INVALID_POSITIVE && driverBest < current)
                        {
                            pos++;
                        }
                    }

                    QualyData.SectorPos[3] = pos;
                }
            }
        }

        private void UpdateCompletedAndValidSectors()
        {
            if (ActualData.CurrentSector == 0 || ActualData.CurrentSector == DisplayData.INVALID_INT)
            {
                // set last lap as completed, because running time for first sector is not shown.
                for (int i = 0; i < 4; i++)
                {
                    ActualData.CurrentLapCompletedAndValid[i] = true;
                }
            }
            else if (ActualData.CurrentSector == 1)
            {
                ActualData.CurrentLapCompletedAndValid[0] = true;
                for (int i = 1; i < 4; i++)
                {
                    ActualData.CurrentLapCompletedAndValid[i] = false;
                }
            }
            else if (ActualData.CurrentSector == 2)
            {
                ActualData.CurrentLapCompletedAndValid[0] = true;
                ActualData.CurrentLapCompletedAndValid[1] = true;
                ActualData.CurrentLapCompletedAndValid[2] = false;
                ActualData.CurrentLapCompletedAndValid[3] = false;
            }
        }

        private void UpdateTB()
        {
            // from current lap
            for (int i = 0; i < 3; i++)
            {
                float sec = ActualData.CurrentLap.RelArray[i];
                var tbSec = ActualData.TBLap.RelArray[i];
                
                
                if (this.ActualData.CurrentLapCompletedAndValid[i] && sec != DisplayData.INVALID_POSITIVE && (sec < tbSec || tbSec == DisplayData.INVALID_POSITIVE))
                {
                    log.Info("New TB in Sector: " + (i + 1));
                    ActualData.TBLap.SetRelSector(i, sec);
                }
            }

            // from previous lap

        }

        /// <summary>
        /// Updates the current lap, so that the time is running in sector 2 and 3. In the first sector all the sector times from the last lap shall be shown.
        /// </summary>
        private void updateCurrentLap()
        {
            // TODO: TrackSector is unreliable, use LapDistance or SectorTimesCurrentSelf!
            if (ActualData.CurrentSector == 0 && ActualShared.SectorTimesCurrentSelf.AbsSector1 == DisplayData.INVALID_POSITIVE)
            {
                //ActualData.CurrentLap.Sector1 = ActualShared.LapTimeCurrentSelf;
            }
            else if (ActualData.CurrentSector == 1 && ActualShared.SectorTimesCurrentSelf.AbsSector2 == DisplayData.INVALID_POSITIVE && !IsFirstSector)
            {
                ActualData.CurrentLap.RelSector2 = ActualShared.LapTimeCurrentSelf - ActualData.CurrentLap.Sector1;
            }
            else if (ActualData.CurrentSector == 2 && ActualShared.SectorTimesCurrentSelf.AbsSector3 == DisplayData.INVALID_POSITIVE && !IsFirstSector)
            {
                ActualData.CurrentLap.RelSector3 = ActualShared.LapTimeCurrentSelf - ActualData.CurrentLap.AbsSector2;
            }
        }

        private bool isNewSector()
        {
            return LastShared.TrackSector != ActualShared.TrackSector;
        }



        /// <summary>
        /// Whether a new lap is starting.
        /// </summary>
        /// <returns></returns>
        private bool isStartOfNewLap()
        {
            //return LastShared.TrackSector != ActualShared.TrackSector;
            return !IsFirst && LastShared.CompletedLaps + 1 == ActualShared.CompletedLaps;
        }

        private bool IsRace()
        {
            return (Session)ActualShared.SessionType == Session.Race;
        }

        private bool isNewStint()
        {
            return LastShared.GameInMenus != 0 && LastShared.GamePaused == 0 && (ActualShared.GameInMenus == 0 || LastShared.GamePaused != 0);
        }

        /// <summary>
        /// Determines if a new session(Training, Qualy, Race) has started.
        /// </summary>
        /// <returns></returns>
        private bool isStartOfNewSession()
        {
            // first session begins first if real new session begins, so that the data are still visible in menu
            return (Session) ActualShared.SessionType != Session.Unavailable  && LastShared.SessionType != ActualShared.SessionType;
        }
    }
}
