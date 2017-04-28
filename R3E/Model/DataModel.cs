using R3E.Data;
using R3E.Database;
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
        /// EventHandler for a new session.
        /// </summary>
        public EventHandler<SessionInfo> OnNewSession;

        /// <summary>
        /// EventHandler for a boxenstop delta.
        /// </summary>
        public EventHandler<BoxenstopDelta> OnBoxenstopDelta;

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
        /// Fuel at begin of this lap.
        /// </summary>
        public float FuelLeftBegin { get; private set; }

        /// <summary>
        /// Fuel used last laps.
        /// </summary>
        public float[] FuelLastLaps { get; private set; }

        /// <summary>
        /// Fuel usage factor. 0 --> no fuel usage, 1 --> 1x, 2 --> 2x ...
        /// </summary>
        public int FuelMultiplicator { get; private set; }

        /// <summary>
        /// Tires wear at begin of this lap.
        /// </summary>
        public Tires TireLeftBegin { get; private set; }

        /// <summary>
        /// Tire used last laps.
        /// </summary>
        public Tires[] TireLastLaps { get; private set; }

        /// <summary>
        /// Tire usage factor. 0 --> no tire usage, 1 --> 1x, 2 --> 2x ...
        /// </summary>
        public int TireMultiplicator { get; private set; }

        /// <summary>
        /// Number of laps to calculate the average fuel usage.
        /// </summary>
        public const int FUEL_AVERAGE_N = 10;

        /// <summary>
        /// Number of laps to calculate the average tire usage.
        /// </summary>
        public const int TIRE_AVERAGE_N = 5;

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

        /// <summary>
        /// Index of the actual FuelLastLaps.
        /// </summary>
        private int IndexFuelLastLaps;

        /// <summary>
        /// Index of the actual TireLastLaps.
        /// </summary>
        private int IndexTireLastLaps;
        private bool isWarmupLap;
        private double StartPitting;

        /// <summary>
        /// Number of PitStops at begin of lap.
        /// </summary>
        private int PitStopsBeginLap;

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
            LastShared = new Shared();
            IsFirst = true;
            LastShared.TrackSector = DisplayData.INVALID_INT;
            LastShared.SessionType = DisplayData.INVALID_INT;
            FuelLeftBegin = DisplayData.INVALID_POSITIVE;
            TireLeftBegin = new Tires();
            StartPitting = DisplayData.INVALID_INT;
            ResetFuelAndTireAverage();
        }

        internal void SetBoxenstopDelta(float delta)
        {
            lock (dataLock)
            {
                if (IsRace())
                {
                    RaceData.EstimatedBoxenstopDelta = delta;
                }
            }
        }

        public void UpdateFromR3E(Shared shared)
        {
            lock (dataLock)
            {
                try
                {
                    if (shared.Player.GameSimulationTime == LastShared.Player.GameSimulationTime || shared.SessionType == DisplayData.INVALID_INT)
                    {
                        // game is paused or invalid
                        LastShared.SessionType = shared.SessionType;
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
                            FuelMultiplicator = ActualShared.FuelUseActive == 0 ? 0 : 1;
                            TireMultiplicator = ActualShared.TireWearActive == 0 ? 0 : 1;
                            ResetFuelAndTireAverage();
                        }

                        if (IsRace())
                        {
                            ActualData = new RaceData();
                            isWarmupLap = true;
                        }
                        else
                        {
                            ActualData = new QualyData();
                        }

                        ActualData.Track = Utilities.byteToString(ActualShared.TrackName);
                        ActualData.Layout = Utilities.byteToString(ActualShared.LayoutName);
                        FuelLeftBegin = DisplayData.INVALID_POSITIVE;
                        TireLeftBegin = new Tires();
                        // TODO: classId? --> carid
                        OnNewSession?.Invoke(this, new SessionInfo(ActualData.Track, ActualData.Layout, shared.SessionType, shared.VehicleInfo.ClassId));
                        
                    }


                    ActualData.CurrentTime = shared.LapTimeCurrentSelf == -1 ? DisplayData.INVALID : shared.LapTimeCurrentSelf;
                    ActualData.CurrentLap = R3EUtils.SectorsToLap(shared.SectorTimesCurrentSelf);

                    updateCurrentLap();
                    // TODO: Calculate Sector Times with help of ActualShared.Player.GameTime to show time on invalid lap
                    ActualData.PBLap = R3EUtils.SectorsToLap(shared.SectorTimesBestSelf);
                    ActualData.FastestLap = R3EUtils.SectorsToLap(shared.SectorTimesSessionBestLap);

                    ActualData.Position = shared.Position;
                    ActualData.CurrentSector = (shared.TrackSector + 2) % 3;
                    ActualData.CompletedLapsCount = shared.CompletedLaps;
                    
                    ActualData.LapDistanceFraction = shared.LapDistanceFraction;



                    UpdateTires();


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
                        else
                        {
                            // just after sector1 after pitting
                            if (PitStopsBeginLap + 1 == ActualShared.NumPitstopsPerformed && ActualShared.TrackSector == 2 && LastShared.TrackSector == 1)
                            {
                                calcBoxenstopDelta();
                                log.Info("Boxenstop delta: " + RaceData.LastBoxenstopDelta);
                                // TODO: Refill, Tires, ClassId
                                OnBoxenstopDelta?.Invoke(this, new BoxenstopDelta(ActualData.Track, ActualData.Layout, RaceData.LastBoxenstopDelta, 1, true, true, shared.VehicleInfo.ClassId));
                            }
                        }
                    }

                    if (IsRace())
                    {
                        RaceData.DiffAhead = ActualShared.TimeDeltaFront;
                        RaceData.DiffBehind = ActualShared.TimeDeltaBehind;
                        UpdateSectorDiffs();
                    }

                    if (isNewStint() && !IsRace())
                    {
                        log.Debug("Start of new stint");
                        IsFirstSector = true;
                        FuelLeftBegin = DisplayData.INVALID_POSITIVE;
                        TireLeftBegin = new Tires();
                    }

                    if (isInPits())
                    {
                        FuelLeftBegin = DisplayData.INVALID_POSITIVE;
                        TireLeftBegin = new Tires();
                    }

                    if (isStartOfNewLap())
                    {
                        log.Debug("Start of new Lap, previous: " + LastShared.CompletedLaps + ", actual: " + ActualShared.CompletedLaps);
                        isWarmupLap = false;
                        if (ActualShared.LapTimePreviousSelf != DisplayData.INVALID)
                        {
                            string session = GetSessionAbbr();

                            try
                            {
                                OnLapCompleted?.Invoke(this, new LapInfo(ActualShared.LapTimePreviousSelf, ActualData.CompletedLapsCount, session, ActualData.Track, StartOfEvent));
                            }
                            catch(Exception e)
                            {
                                log.Error("Exception on saving lap: " + e.Message);
                            }
                        }

                        if (!IsFirstSector && FuelMultiplicator > 0)
                        {
                            FuelCalculation();
                        }

                        if (!IsFirstSector && TireMultiplicator > 0)
                        {
                            TireCalculation();
                        }

                        PitStopsBeginLap = shared.NumPitstopsPerformed;
                    }

                    if (IsRace() && LeaderStartsNewLap())
                    {
                        log.Info("Leader starts new lap: " + Utilities.byteToString(ActualShared.DriverData[0].DriverInfo.Name));
                        UpdateEstimatedLaps();
                    }


                    LastShared = shared;
                    IsFirst = false;
                    ActualData.PreviousLap = ActualData.CurrentLap;
                    
                }
                catch (Exception e)
                {
                    log.Error("Exception: " + e.Message);
                    LastShared = shared;
                }
            }
        }

        private void calcBoxenstopDelta()
        {
            // sector3 and sector1 diff to pb
            RaceData.LastBoxenstopDelta = ActualData.PreviousLap.RelSector3 - ActualData.PBLap.RelSector3 + ActualData.CurrentLap.Sector1 - ActualData.PBLap.Sector1;
        }

        private bool isInPits()
        {
            if (ActualShared.InPitlane != 0)
            {
                if (StartPitting == DisplayData.INVALID)
                {
                    StartPitting = ActualShared.Player.GameSimulationTime;
                }
                else if (ActualShared.Player.GameSimulationTime - StartPitting > 3)
                {
                    // count as pitting if more than 3 seconds in pitlane, sometimes it happens accidentally for a few tenths.
                    return true;
                }
            }
            else
            {
                StartPitting = DisplayData.INVALID;
            }

            return false;
        }

        private String GetSessionAbbr()
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

            return session;
        }

        private void ResetFuelAndTireAverage()
        {
            FuelLastLaps = new float[FUEL_AVERAGE_N];
            for (int i = 0; i < FUEL_AVERAGE_N; i++)
            {
                FuelLastLaps[i] = DisplayData.INVALID_INT;
            }

            IndexFuelLastLaps = 0;


            TireLastLaps = new Tires[TIRE_AVERAGE_N];
            for (int i = 0; i < TIRE_AVERAGE_N; i++)
            {
                TireLastLaps[i] = new Tires();
            }

            IndexTireLastLaps = 0;
        }

        private void UpdateSectorDiffs()
        {
            if (ActualShared.Position > 1 && ActualShared.NumCars > 0)
            {
                if (ActualData.CurrentSector == 0)
                {
                    // in the first sector we need to compare with previous lap to show diffs from previous lap
                    RaceData.DiffSectorsAhead = ActualData.CurrentLap - R3EUtils.SectorsToLap(ActualShared.DriverData[ActualShared.Position - 2].SectorTimePreviousSelf);
                }
                else
                {
                    RaceData.DiffSectorsAhead = ActualData.CurrentLap - R3EUtils.SectorsToLap(ActualShared.DriverData[ActualShared.Position - 2].SectorTimeCurrentSelf);
                }
            }
            else
            {
                RaceData.DiffSectorsAhead = new Model.Lap();
            }

            
            if (ActualShared.NumCars > ActualShared.Position && ActualShared.Position > 0)
            {
                var driverBehind = ActualShared.DriverData[ActualShared.Position];
                if (ActualData.CurrentSector == 0)
                {
                    // in the first sector we need to compare with previous lap
                    this.RaceData.DiffSectorsBehind = this.ActualData.CurrentLap - R3EUtils.SectorsToLap(driverBehind.SectorTimePreviousSelf);
                }
                else if (IsInSector(driverBehind) != 0)
                {
                    this.RaceData.DiffSectorsBehind = this.ActualData.CurrentLap - R3EUtils.SectorsToLap(driverBehind.SectorTimeCurrentSelf);
                }
                else
                {
                    RaceData.DiffSectorsBehind = new Model.Lap();
                }
            }
            else
            {
                RaceData.DiffSectorsBehind = new Model.Lap();
            }
        }

        private int IsInSector(DriverData driver)
        {
            return (driver.TrackSector + 2) % 3;
        }

        private bool LeaderStartsNewLap()
        {
            if (ActualShared.NumCars > 0 && LastShared.NumCars > 0)
            {
                return ActualShared.DriverData[0].CompletedLaps == LastShared.DriverData[0].CompletedLaps + 1;
            }

            return false;
        }

        private void UpdateTires()
        {
            ActualData.TiresWear = R3EUtils.R3ETiresToTires(ActualShared.TireWear);
        }

        private void FuelCalculation()
        {
            // TODO: Calculate offset to finishline
            if (FuelLeftBegin != DisplayData.INVALID_POSITIVE)
            {
                ActualData.FuelLastLap = FuelLeftBegin - ActualShared.FuelLeft;
                if (ActualData.FuelLastLap > ActualData.FuelMaxLap)
                {
                    ActualData.FuelMaxLap = ActualData.FuelLastLap;
                }

                FuelLastLaps[IndexFuelLastLaps] = ActualData.FuelLastLap;
                IndexFuelLastLaps = (IndexFuelLastLaps + 1) % FUEL_AVERAGE_N;

                UpdateAverageFuel();
                UpdateFuelLaps();
                UpdateFuelToRefill();
                log.Info("Fuel usage last lap: " + ActualData.FuelLastLap);
            }

            FuelLeftBegin = ActualShared.FuelLeft;
        }

        /// <summary>
        /// Calculates the estimated total laps for a race.
        /// </summary>
        private void UpdateEstimatedLaps()
        {
            if (ActualShared.NumCars > 0)
            {
                DriverData driver = ActualShared.DriverData[0];
                float fastestLap = driver.SectorTimeBestSelf.AbsSector3;
                int completedLaps = driver.CompletedLaps;
                if (fastestLap != DisplayData.INVALID)
                {
                    int restlaps = 0;
                    if (ActualShared.SessionLengthFormat == 0)
                    {
                        // time based
                        restlaps = (int)(ActualShared.SessionTimeRemaining / fastestLap) + 1;
                    }
                    else if (ActualShared.SessionLengthFormat == 1)
                    {
                        // lap based
                        restlaps = ActualShared.NumberOfLaps - completedLaps;
                    }
                    else if (ActualShared.SessionLengthFormat == 0)
                    {
                        // time based with extra lap
                        restlaps = (int)(ActualShared.SessionTimeRemaining / fastestLap) + 2;
                    }

                     RaceData.EstimatedRaceLaps = completedLaps + restlaps;
                }
                else
                {
                    RaceData.EstimatedRaceLaps = DisplayData.INVALID_INT;
                }
            }
        }

        private void UpdateFuelToRefill()
        {
            if (IsRace())
            {
                if (ActualData.FuelAveragePerLap <= 0)
                {
                    RaceData.FuelToRefill = DisplayData.INVALID_POSITIVE;
                }
                else
                {
                    float restLap = (1 - ActualShared.LapDistanceFraction) * ActualData.FuelAveragePerLap;
                    RaceData.FuelToRefill = restLap + (RaceData.EstimatedRaceLaps - ActualShared.CompletedLaps - 1) * ActualData.FuelAveragePerLap - ActualShared.FuelLeft;
                }
            }
        }

        private void TireCalculation()
        {
            var tires = R3EUtils.R3ETiresToTires(ActualShared.TireWear);
            // TODO: Calculate offset to finishline
            if (TireLeftBegin.Valid)
            {
                ActualData.TireUsedLastLap = TireLeftBegin - tires;
                ActualData.TireUsedMaxLap.FrontLeft = Math.Max(ActualData.TireUsedMaxLap.FrontLeft, ActualData.TireUsedLastLap.FrontLeft);
                ActualData.TireUsedMaxLap.FrontRight = Math.Max(ActualData.TireUsedMaxLap.FrontRight, ActualData.TireUsedLastLap.FrontRight);
                ActualData.TireUsedMaxLap.RearLeft = Math.Max(ActualData.TireUsedMaxLap.RearLeft, ActualData.TireUsedLastLap.RearLeft);
                ActualData.TireUsedMaxLap.RearRight = Math.Max(ActualData.TireUsedMaxLap.RearRight, ActualData.TireUsedLastLap.RearRight);

                TireLastLaps[IndexTireLastLaps] = ActualData.TireUsedLastLap;
                IndexTireLastLaps = (IndexTireLastLaps + 1) % TIRE_AVERAGE_N;

                UpdateAverageTire();
                log.Info("Tire usage last lap: " + ActualData.TireUsedLastLap);
            }

            this.TireLeftBegin = tires;
        }

        private void UpdateFuelLaps()
        {
            if (ActualData.FuelAveragePerLap > 0)
            {
                ActualData.FuelRemainingLaps = (int)((ActualShared.FuelLeft - 1) / ActualData.FuelAveragePerLap);
            }
            else
            {
                ActualData.FuelRemainingLaps = DisplayData.INVALID_INT;
            }
        }

        private void UpdateAverageFuel()
        {
            int count = 0;
            float sum = 0;
            for (int i = 0; i < FUEL_AVERAGE_N; i++)
            {
                if (FuelLastLaps[i] != DisplayData.INVALID_POSITIVE)
                {
                    count++;
                    sum += FuelLastLaps[i];
                }
            }

            ActualData.FuelAveragePerLap = count != 0 ? sum / count : DisplayData.INVALID_POSITIVE;
        }

        private void UpdateAverageTire()
        {
            int count = 0;
            Tires sum = new Tires(0, 0, 0, 0);
            for (int i = 0; i < TIRE_AVERAGE_N; i++)
            {
                if (TireLastLaps[i].Valid)
                {
                    count++;
                    sum = sum + TireLastLaps[i];
                }
            }

            if (sum.Valid)
            {
                ActualData.TireUsedAveragePerLap = new Tires(sum.FrontLeft / count, sum.FrontRight / count, sum.RearLeft / count, sum.RearRight / count);
            }
            else
            {
                ActualData.TireUsedAveragePerLap = new Tires();
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
                if (current == DisplayData.INVALID || !ActualData.CurrentLapCompleted[i])
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
                        if (driverCur != DisplayData.INVALID && driverCur < current || driverBest != DisplayData.INVALID && driverBest < current)
                        {
                            pos++;
                        }
                    }
                }

                QualyData.SectorPos[i] = pos;
            }

            // show on lap pos estimated position, if 3. sector is not over yet
            if (currentSec[2] == DisplayData.INVALID)
            {
                if (currentSec[1] == DisplayData.INVALID)
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
                        if (driverCur != DisplayData.INVALID && driverCur < current || driverBest != DisplayData.INVALID && driverBest < current)
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
                    ActualData.CurrentLapCompleted[i] = true;
                }
            }
            else if (ActualData.CurrentSector == 1)
            {
                ActualData.CurrentLapCompleted[0] = true;
                for (int i = 1; i < 4; i++)
                {
                    ActualData.CurrentLapCompleted[i] = false;
                }
            }
            else if (ActualData.CurrentSector == 2)
            {
                ActualData.CurrentLapCompleted[0] = true;
                ActualData.CurrentLapCompleted[1] = true;
                ActualData.CurrentLapCompleted[2] = false;
                ActualData.CurrentLapCompleted[3] = false;
            }
        }

        private void UpdateTB()
        {
            // from current lap
            for (int i = 0; i < 3; i++)
            {
                float sec = ActualData.CurrentLap.RelArray[i];
                var tbSec = ActualData.TBLap.RelArray[i];
                
                
                if (this.ActualData.CurrentLapCompleted[i] && sec != DisplayData.INVALID && (sec < tbSec || tbSec == DisplayData.INVALID))
                {
                    log.Info("New TB in Sector: " + (i + 1));
                    ActualData.TBLap.SetRelSector(i, sec);
                }
            }

            // from previous lap / from PB

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
            else if (ActualData.CurrentSector == 1 && ActualShared.SectorTimesCurrentSelf.AbsSector2 == DisplayData.INVALID_POSITIVE && !IsFirstSector && ActualData.CurrentLap.Sector1 != DisplayData.INVALID)
            {
                ActualData.CurrentLap.RelSector2 = ActualShared.LapTimeCurrentSelf - ActualData.CurrentLap.Sector1;
            }
            else if (ActualData.CurrentSector == 2 && ActualShared.SectorTimesCurrentSelf.AbsSector3 == DisplayData.INVALID_POSITIVE && !IsFirstSector && ActualData.CurrentLap.AbsSector2 != DisplayData.INVALID)
            {
                if (ActualShared.LapTimeCurrentSelf < 5)
                {
                    // it is not possible that the driver is in sector 3!
                    return;
                }

                ActualData.CurrentLap.RelSector3 = ActualShared.LapTimeCurrentSelf - ActualData.CurrentLap.AbsSector2;
            }

            // dismiss sector3 time from shared if in -1. lap of the race (start grid to race line)
            if (isWarmupLap)
            {
                ActualData.CurrentLap = new Model.Lap();
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
            // TrackSector 0 --> Sector 3, TrackSector 1 = Sector 1
            return !IsFirst && LastShared.TrackSector == 0 && ActualShared.TrackSector == 1;
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
