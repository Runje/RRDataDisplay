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
        public object DataLock = new object();

        /// <summary>
        /// EventHandler for completed Laps
        /// </summary>
        public EventHandler<LapInfo> OnLapCompleted;

        /// <summary>
        /// EventHandler for updating sector limits.
        /// </summary>
        public EventHandler<TrackLimits> OnTrackUpdate;

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
        /// Sector of last iteration.
        /// </summary>
        private int LastSector;

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

        /// <summary>
        /// Start standing time in the pits.
        /// </summary>
        private double StartStandingTime;
        private float StartPitFuel;
        private Tires StartPitTires;
        private bool FrontTiresChanged;

        /// <summary>
        /// Whether the sector time was calculated on my own.
        /// </summary>
        private bool[] CurrentLapOwnCalculation;
        /// <summary>
        /// Game time when the last sector was started
        /// </summary>
        private double StartLastSectorTime;

        /// <summary>
        /// Current sector times which are already completed
        /// </summary>
        private Lap CurrentCompleteLap
        {
            get
            {
                var curLap = ActualData.CurrentLap;
                return new Lap(ActualData.CurrentLapCompleted[0] ? curLap.Sector1 : DisplayData.INVALID, ActualData.CurrentLapCompleted[1] ? curLap.RelSector2 : DisplayData.INVALID, ActualData.CurrentLapCompleted[2] ? curLap.RelSector3 : DisplayData.INVALID);
            }
        }

        public float Refilled { get; private set; }
        public bool RearTiresChanged { get; private set; }
        public Car DriversCar { get; private set; }
        public bool IsOutlap { get; private set; }

        private TrackLimits Track;
        private bool FirstSectorValid;
        private float LapTimeCurrentSelf;
        private bool LastLap;
        private float AllFuelUsage;
        private bool RestartedLeaderboard;

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
            IsFirstSector = true;
            LastShared.TrackSector = DisplayData.INVALID_INT;
            LastShared.SessionType = DisplayData.INVALID_INT;
            FuelLeftBegin = DisplayData.INVALID_POSITIVE;
            TireLeftBegin = new Tires();
            StartPitting = DisplayData.INVALID_INT;
            ResetFuelAndTireAverage();
            StartStandingTime = DisplayData.INVALID_POSITIVE;
            Track = null;
            for (int i = 0; i < ActualData.CurrentLapValid.Length; i++)
            {
                ActualData.CurrentLapValid[i] = true;
            }
            FirstSectorValid = true;
            StartLastSectorTime = DisplayData.INVALID_POSITIVE;
            LastSector = DisplayData.INVALID_INT;
            IsOutlap = true;
            LapTimeCurrentSelf = DisplayData.INVALID_POSITIVE;
            CurrentLapOwnCalculation = new bool[3];
            ResetCurrentLapOwnCalculation();
            LastLap = false;
            AllFuelUsage = 0;
        }

        private void ResetCurrentLapOwnCalculation()
        {
            for (int i = 0; i < CurrentLapOwnCalculation.Length; i++)
            {
                CurrentLapOwnCalculation[i] = false;
            }
        }

        internal void SetBoxenstopDelta(float delta)
        {
            lock (DataLock)
            {
                if (IsRace())
                {
                    RaceData.EstimatedBoxenstopDelta = delta;
                }
            }
        }

        internal void SetLimits(TrackLimits track)
        {
            lock (DataLock)
            {
                
                if (track != null)
                {
                    Track = track;
                }
                else
                {
                    Track = new Database.TrackLimits(ActualData.Track, ActualData.Layout, DisplayData.INVALID_POSITIVE, DisplayData.INVALID_POSITIVE, DisplayData.INVALID_POSITIVE, DisplayData.INVALID_POSITIVE);
                }
            }
        }

        public bool OwnSectorCalculationNeeded()
        {
            if (IsRace())
            {
                // in the race all sector times are shown
                return false;
            }

            int currentSector = ActualData.CurrentSector;
            return (!FirstSectorValid && currentSector == 1) || !ActualData.CurrentLapValid[3] || IsOutlap;
        }

        public void UpdateFromR3E(Shared shared)
        {
            lock (DataLock)
            {
                try
                {
                    if (shared.Player.GameSimulationTime == LastShared.Player.GameSimulationTime || shared.SessionType == DisplayData.INVALID_INT)
                    {
                        // game is paused or invalid
                        // TODO: This is called too often, also while driving. Is it reading too often?
                        LastShared.SessionType = shared.SessionType;
                        return;
                    }

                    ActualShared = shared;
                    

                    if (isStartOfNewSession())
                    {
                        log.Debug("Start of new session " + LastShared.SessionType + " --> " + shared.SessionType);
                        StartLastSectorTime = DisplayData.INVALID_POSITIVE;
                        LastSector = DisplayData.INVALID_INT;
                        ResetLapValid();
                        if (LastShared.SessionType == DisplayData.INVALID_INT)
                        {
                            log.Debug("Start of new event");
                            ResetAll();
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
                            LastLap = false;
                            log.Info("RACE");
                            AllFuelUsage = 0;
                        }
                        else
                        {
                            ActualData = new QualyData();
                            IsOutlap = true;
                            log.Info("NO RACE");
                        }
                        IsFirstSector = true;
                        ActualData.PBLap = R3EUtils.SectorsToLap(shared.SectorTimesBestSelf);
                        ActualData.PreviousLap = R3EUtils.SectorsToLap(shared.SectorTimesPreviousSelf);
                        ActualData.FastestLap = R3EUtils.SectorsToLap(shared.SectorTimesSessionBestLap);
                        ActualData.CurrentLap = R3EUtils.SectorsToLap(shared.SectorTimesCurrentSelf);
                        

                        if (TrackSectorToCurrentSector() == 0)
                        {
                            // because in the first sector the last lap is shown
                            ActualData.CurrentLap = R3EUtils.SectorsToLap(shared.SectorTimesPreviousSelf);
                        }


                        UpdateTB();
                        UpdateTBFromPrevious();
                        ActualData.Track = Utilities.byteToString(ActualShared.TrackName);
                        ActualData.Layout = Utilities.byteToString(ActualShared.LayoutName);
                        FuelLeftBegin = DisplayData.INVALID_POSITIVE;
                        TireLeftBegin = new Tires();
                        DriversCar = new Car(shared.VehicleInfo.ClassId, shared.VehicleInfo.ModelId);
                        OnNewSession?.Invoke(this, new SessionInfo(ActualData.Track, ActualData.Layout, shared.SessionType, DriversCar));
                    }

                    if (shared.ControlType == 1 && IsLeaderBoard() && TrackSectorToCurrentSector() == 2 && !RestartedLeaderboard)
                    {
                        // probably hit restart in leaderboard mode
                        log.Info("Restart leaderboard stint");
                        IsFirstSector = true;
                        IsOutlap = true;
                        FuelLeftBegin = DisplayData.INVALID_POSITIVE;
                        TireLeftBegin = new Tires();
                        ResetLapValid();
                        StartLastSectorTime = DisplayData.INVALID_POSITIVE;
                        ActualData.CurrentLap = new Lap();
                        RestartedLeaderboard = true;
                    }
                    else if (!(shared.ControlType == 1 && IsLeaderBoard() && TrackSectorToCurrentSector() == 2))
                    {
                        RestartedLeaderboard = false;
                    }

                    if (isNewStint() && !IsRace())
                    {
                        log.Debug("Start of new stint");
                        IsFirstSector = true;
                        IsOutlap = true;
                        FuelLeftBegin = DisplayData.INVALID_POSITIVE;
                        TireLeftBegin = new Tires();
                        ResetLapValid();
                        StartLastSectorTime = DisplayData.INVALID_POSITIVE;
                        ActualData.CurrentLap = new Lap();
                    }

                    if (HasQuitStint())
                    {
                        if (!HasLastQuitStint())
                        {
                            log.Debug("Quit stint");
                        }
                        StartLastSectorTime = DisplayData.INVALID_POSITIVE;
                        FuelLeftBegin = DisplayData.INVALID_POSITIVE;
                        TireLeftBegin = new Tires();
                        ActualData.CurrentTime = DisplayData.INVALID;
                        IsFirstSector = true;
                        ActualData.CurrentSector = DisplayData.INVALID_INT;
                        LapTimeCurrentSelf = DisplayData.INVALID;
                        LastShared = ActualShared;
                        return;
                    }

                    LapTimeCurrentSelf = ActualShared.LapTimeCurrentSelf;
                    ActualData.CurrentTime = shared.LapTimeCurrentSelf == -1 ? DisplayData.INVALID : shared.LapTimeCurrentSelf;
                    UpdateCurrentSector();
                    detectCutting();
                    if (isNewSector())
                    {
                        log.Debug("Sector change: " + (LastShared.TrackSector + 2) % 3 + " --> " + TrackSectorToCurrentSector());


                        UpdateCompletedAndValidSectors();
                        // must be after update sector limits
                        UpdateSectorLimits();
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
                                OnBoxenstopDelta?.Invoke(this, new BoxenstopDelta(ActualData.Track, ActualData.Layout, RaceData.LastBoxenstopDelta, (int)Math.Round(Refilled), FrontTiresChanged, RearTiresChanged, DriversCar, RaceData.LastStandingTime));
                            }
                        }
                    }

                    updateCurrentLap();
                    // TODO: Calculate Sector Times with help of ActualShared.Player.GameTime to show time on invalid lap
                    ActualData.PBLap = R3EUtils.SectorsToLap(shared.SectorTimesBestSelf);
                    ActualData.FastestLap = R3EUtils.SectorsToLap(shared.SectorTimesSessionBestLap);

                    ActualData.Position = shared.Position;
                    
                    ActualData.CompletedLapsCount = shared.CompletedLaps;
                    
                    ActualData.LapDistanceFraction = shared.LapDistanceFraction;
                    ActualData.Name = Utilities.byteToString(shared.PlayerName);



                    UpdateTires();


                    

                    if (IsRace())
                    {
                        RaceData.DiffAhead = ActualShared.TimeDeltaFront;
                        RaceData.DiffBehind = ActualShared.TimeDeltaBehind;
                        UpdateSectorDiffs();
                        if (!isInPits())
                        {
                            UpdateStandingsAfterBox();
                        }
                        else
                        {
                            handlePitstop();
                        }
                    }

                    

                    

                    if (isStartOfNewLap())
                    {
                        log.Debug("Start of new Lap, previous: " + LastShared.CompletedLaps + ", actual: " + ActualShared.CompletedLaps);
                        isWarmupLap = false;
                        IsOutlap = false;
                        if (ActualData.CurrentLap.Time != DisplayData.INVALID)
                        {
                            string session = GetSessionAbbr();

                            try
                            {
                                OnLapCompleted?.Invoke(this, new LapInfo(ActualData.CurrentLap.Time, ActualData.CompletedLapsCount, session, ActualData.Track, StartOfEvent));
                            }
                            catch(Exception e)
                            {
                                log.Error("Exception on saving lap: " + e.Message);
                            }
                        }
                        else
                        {
                            log.Debug("Lap is not saved because Current lap time is invalid");
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
                        if (ActualShared.SessionTimeRemaining == 0 && ActualShared.SessionLengthFormat == 2)
                        {
                            LastLap = true;
                        }

                        UpdateEstimatedLaps();
                    }


                    LastShared = shared;
                    IsFirst = false;
                }
                catch (Exception e)
                {
                    log.Error("Exception: " + e.Message);
                    LastShared = shared;
                    // Force to reinit session
                    LastShared.SessionType = -1;
                }
            }
        }

        private bool IsLeaderBoard()
        {
            return ActualShared.SessionType == 1 && ActualShared.NumCars < 3;
        }

        private bool HasQuitStint()
        {
            return ActualShared.GameInMenus != 0 && !(ActualShared.GamePaused != 0);
        }

        private bool HasLastQuitStint()
        {
            return LastShared.GameInMenus != 0 && !(LastShared.GamePaused != 0);
        }

        private void handlePitstop()
        {
            if (isStanding() && StartStandingTime == DisplayData.INVALID_POSITIVE)
            {
                StartStandingTime = ActualShared.Player.GameSimulationTime;
                StartPitFuel = ActualShared.FuelLeft;
                StartPitTires = ActualData.TiresWear;
            }
            else if (!isStanding() && StartStandingTime != DisplayData.INVALID_POSITIVE)
            {
                float standingTime = (float)(ActualShared.Player.GameSimulationTime - StartStandingTime);
                if (standingTime > 3)
                {
                    // only count as boxenstop if standing more than 3s in case the driver has to drive backwards to the correct position
                    RaceData.LastStandingTime = standingTime;
                    Refilled = ActualShared.FuelLeft - StartPitFuel;
                    RearTiresChanged = StartPitTires.RearLeft < ActualData.TiresWear.RearLeft;
                    FrontTiresChanged = StartPitTires.FrontLeft < ActualData.TiresWear.FrontLeft;
                    log.Debug("Standing Time: " + RaceData.LastStandingTime + ", Refilled: " + Refilled + ", Changed Rear: " + RearTiresChanged + ", Changed Front: " + FrontTiresChanged);
                    StartStandingTime = DisplayData.INVALID_POSITIVE;
                    StartPitFuel = DisplayData.INVALID_POSITIVE;
                    StartPitTires = new Tires();
                    FuelLeftBegin += (float) Math.Ceiling(Refilled);
                    TireLeftBegin = new Tires();
                }
                else
                {
                    log.Debug("Standing time: " + standingTime);
                    StartStandingTime = DisplayData.INVALID_POSITIVE;
                    StartPitFuel = DisplayData.INVALID_POSITIVE;
                    StartPitTires = new Tires();
                }
            }
        }

        private void UpdateCurrentSector()
        {
            if (HasQuitStint())
            {
                return;
            }

            LastSector = ActualData.CurrentSector;

            if (!OwnSectorCalculationNeeded() || (Track == null || Track.FirstSector == DisplayData.INVALID_POSITIVE || Track.SecondSector == DisplayData.INVALID_POSITIVE))
            {
                this.ActualData.CurrentSector = TrackSectorToCurrentSector();
            }
            else
            {
                ActualData.CurrentSector = 0;

                if (ActualShared.LapDistanceFraction > Track.FirstSector)
                {
                    ActualData.CurrentSector = 1;
                }

                if (ActualShared.LapDistanceFraction > Track.SecondSector)
                {
                    ActualData.CurrentSector = 2;
                }

            }

            if (isNewSector())
            {
                if (LastSector!= DisplayData.INVALID_INT)
                {
                    IsFirstSector = false;
                }

                if (!IsFirstSector)
                {
                    double StartSectorTime = ActualShared.Player.GameSimulationTime;
                    float meterDriven = 0;
                    if (ActualData.CurrentSector == 0)
                    {
                        meterDriven = ActualShared.LapDistanceFraction * ActualShared.LayoutLength;
                    }
                    else if (ActualData.CurrentSector == 1)
                    {
                        // reset current lap time at end of first sector, because last lap is show until 2nd sector
                        ActualData.PreviousLap = new Lap(ActualData.CurrentLap);
                        ActualData.CurrentLap = new Lap();
                        ResetCurrentLapOwnCalculation();
                    }
                    else  if (Track != null && Track.FirstSector != DisplayData.INVALID_POSITIVE && Track.SecondSector != DisplayData.INVALID_POSITIVE)
                    {
                        if (ActualData.CurrentSector == 1)
                        {
                            meterDriven = (ActualShared.LapDistanceFraction - Track.FirstSector) * ActualShared.LayoutLength;
                        }
                        else if (ActualData.CurrentSector == 2)
                        {
                            meterDriven = (ActualShared.LapDistanceFraction - Track.SecondSector) * ActualShared.LayoutLength;
                        }
                    }

                    if (StartLastSectorTime != DisplayData.INVALID_POSITIVE)
                    {
                        float time = CalcSecondsPassedLast(meterDriven);
                        StartSectorTime -= time;
                        float sectorTime = (float)(StartSectorTime - StartLastSectorTime);
                        // set sector time of last sector
                        if (OwnSectorCalculationNeeded())
                        {
                            log.Info("Calculating sector time in sector " + (ActualData.CurrentSector + 1) + ", time: " + sectorTime + ", isOutlap: " 
                                + IsOutlap + ", CurrentLapValid: " + ActualData.CurrentLapValid[3] + ", firstSectorValid: " + FirstSectorValid);
                            if (ActualData.CurrentSector == 0)
                            {
                                ActualData.CurrentLap.RelSector3 = sectorTime;
                                CurrentLapOwnCalculation[2] = true;
                            }
                            else if (ActualData.CurrentSector == 1)
                            {
                                ActualData.CurrentLap.Sector1 = sectorTime;
                                CurrentLapOwnCalculation[0] = true;
                            }
                            else if (ActualData.CurrentSector == 2)
                            {
                                ActualData.CurrentLap.RelSector2 = sectorTime;
                                CurrentLapOwnCalculation[1] = true;
                            }
                        }
                    }

                    // overwrite with data from game if valid

                    Lap currentLap = R3EUtils.SectorsToLap(ActualShared.SectorTimesCurrentSelf);
                    if (ActualData.CurrentSector == 0 && currentLap.RelSector3 != DisplayData.INVALID)
                    {
                        ActualData.CurrentLap.RelSector3 = currentLap.RelSector3;
                        CurrentLapOwnCalculation[2] = false;
                    }
                    else if (ActualData.CurrentSector == 1 && currentLap.Sector1 != DisplayData.INVALID)
                    {
                        ActualData.CurrentLap.Sector1 = currentLap.Sector1;
                        CurrentLapOwnCalculation[0] = false;
                    }
                    else if (ActualData.CurrentSector == 2 && currentLap.RelSector2 != DisplayData.INVALID)
                    {
                        ActualData.CurrentLap.RelSector2 = currentLap.RelSector2;
                        CurrentLapOwnCalculation[1] = false;
                    }
                    else if (LastLap && ActualData.CurrentSector == 0 && IsRace())
                    {
                        // After finish of the race the current time is invalid, use time of previous lap
                        Lap prevLap = R3EUtils.SectorsToLap(ActualShared.SectorTimesPreviousSelf);
                        ActualData.CurrentLap.RelSector3 = prevLap.RelSector3;
                        CurrentLapOwnCalculation[2] = false;
                    }

                    StartLastSectorTime = StartSectorTime;
                }
                
            }
        }

        private int TrackSectorToCurrentSector()
        {
            return (ActualShared.TrackSector + 2) % 3;
        }

        private float CalcSecondsPassedLast(float meterDriven)
        {
            float speed = GetAverageSpeedInMeterPerSecond();
            return meterDriven / speed;
        }

        private void detectCutting()
        {
            if (ActualShared.ExtendedFlags.BlackAndWhite == 4 && ActualData.CurrentLapValid[ActualData.CurrentSector])
            {
                // Cutting
                log.Info("Cutting in Sector: " + (ActualData.CurrentSector + 1));
                if (ActualData.CurrentSector == 0)
                {
                    // because in first sector last lap is shown
                    FirstSectorValid = false;
                }
                else
                {

                    ActualData.CurrentLapValid[ActualData.CurrentSector] = false;
                    ActualData.CurrentLapValid[3] = false;
                }
            }
        }

        private void ResetLapValid()
        {
            for (int i = 0; i < ActualData.CurrentLapValid.Length; i++)
            {
                ActualData.CurrentLapValid[i] = true;
            }
        }

        private void UpdateSectorLimits()
        {
            if (Track == null)
            {
                // update from database not arrived yet
                return;
            }

            if (ActualData.CurrentSector == 1)
            {
                // Update first limit

                // estimate limit
                float estimation = ActualShared.LapDistanceFraction;

                bool goodEstimation = false;
                float timeDriven = DisplayData.INVALID_POSITIVE;
                // how far away?
                if (ActualData.CurrentTime != DisplayData.INVALID && CurrentCompleteLap.Sector1 != DisplayData.INVALID && !CurrentLapOwnCalculation[0])
                {
                    timeDriven = ActualData.CurrentTime - CurrentCompleteLap.Sector1;
                    float speed = GetAverageSpeedInMeterPerSecond();
                    float meterDriven = CalcMeterDrivenInLast(timeDriven);
                    estimation -= meterDriven / ActualShared.LayoutLength;
                    goodEstimation = true;

                }

                if (Track.FirstSector == DisplayData.INVALID_POSITIVE || goodEstimation)
                {
                    if (estimation > Track.SecondSector && Track.SecondSector != DisplayData.INVALID_POSITIVE || estimation <= 0)
                    {
                        log.Error("Bad estimation 1: " + estimation + ", error: " + timeDriven);
                    }
                    else
                    {
                        if (timeDriven < Track.FirstSectorError || Track.FirstSectorError == DisplayData.INVALID_POSITIVE)
                        {
                            // better estimation than in database
                            log.Info("New estimation 1: " + estimation + ", error: " + timeDriven);
                            Track.FirstSector = estimation;
                            Track.FirstSectorError = timeDriven;
                            OnTrackUpdate.Invoke(this, Track);
                        }
                    }
                }
            }
            else if (ActualData.CurrentSector == 2)
            {
                // Update second limit
                // estimate limit
                float estimation = ActualShared.LapDistanceFraction;

                // how far away?
                bool goodEstimation = false;
                float timeDriven = DisplayData.INVALID_POSITIVE;
                if (ActualData.CurrentTime != DisplayData.INVALID && CurrentCompleteLap.AbsSector2 != DisplayData.INVALID && !CurrentLapOwnCalculation[0])
                {
                    timeDriven = ActualData.CurrentTime - CurrentCompleteLap.AbsSector2;
                    float speed = GetAverageSpeedInMeterPerSecond();
                    float meterDriven = speed * timeDriven;
                    estimation -= meterDriven / ActualShared.LayoutLength;
                    goodEstimation = true;
                }

                if (Track.SecondSector == DisplayData.INVALID_POSITIVE || goodEstimation)
                {
                    if (estimation < Track.FirstSector && Track.FirstSector != DisplayData.INVALID_POSITIVE || estimation >= 1)
                    {
                        log.Error("Bad estimation 2: " + estimation + ", error: " + timeDriven);
                    }
                    else
                    {
                        if (timeDriven < Track.SecondSectorError || Track.SecondSectorError == DisplayData.INVALID_POSITIVE)
                        {
                            // better estimtation than in database
                            log.Info("New estimation 2: " + estimation + ", error: " + timeDriven);
                            Track.SecondSector = estimation;
                            Track.SecondSectorError = timeDriven;
                            OnTrackUpdate.Invoke(this, Track);
                        }
                    }
                }

            }
        }

        private float CalcMeterDrivenInLast(float timeDriven)
        {
            float speed = GetAverageSpeedInMeterPerSecond();
            return speed * timeDriven;
        }

        private float GetAverageSpeedInMeterPerSecond()
        {
            return ActualShared.CarSpeed / 2 + LastShared.CarSpeed / 2;
        }

        private bool isStanding()
        {
            return Math.Abs(ActualShared.LapDistance - LastShared.LapDistance) < 0.1;
            //return Math.Abs(ActualShared.CarSpeed) < 0.1;
        }

        private void UpdateStandingsAfterBox()
        {
            if (RaceData.EstimatedBoxenstopDelta != DisplayData.INVALID_POSITIVE)
            {
                RaceData.EstimatedStandings = new List<StandingsDriver>();
                float deltaToLeader = 0;
                StandingsDriver me = null;
                for (int i = 0; i < ActualShared.NumCars; i++)
                {
                    DriverData driver = ActualShared.DriverData[i];
                    int currPos = me == null ? i + 1 : i;
                    if (i == 0)
                    {
                        // first position
                        
                        if (i + 1 == ActualData.Position)
                        {
                            // it's me, add estimatedBoxenstopdelta
                            me = new StandingsDriver(currPos, RaceData.EstimatedBoxenstopDelta, Utilities.byteToString(driver.DriverInfo.Name));
                        }
                        else
                        {
                            RaceData.EstimatedStandings.Add(new StandingsDriver(currPos, 0, Utilities.byteToString(driver.DriverInfo.Name)));
                        }
                    }
                    else
                    {
                        
                        // diff to car in front
                        var timeDeltaFront = driver.TimeDeltaFront;
                        deltaToLeader += timeDeltaFront;
                        if (me != null)
                        {
                            // add me, if i am before the next guy
                            if (me.Delta < deltaToLeader)
                            {
                                me.Pos = currPos;
                                RaceData.EstimatedStandings.Add(me);
                                //log.Debug("Calculated pos: " + currPos);
                                me = null;
                            }
                        }

                        if (i + 1 == ActualData.Position)
                        {
                            // it's me, add estimatedBoxenstopdelta
                            me = new StandingsDriver(currPos, deltaToLeader + RaceData.EstimatedBoxenstopDelta, Utilities.byteToString(driver.DriverInfo.Name));
                        }
                        else
                        {
                            this.RaceData.EstimatedStandings.Add(new StandingsDriver(currPos, deltaToLeader, Utilities.byteToString(driver.DriverInfo.Name)));
                        }
                    }
                }

                if (me != null)
                {
                    // last
                    me.Pos = ActualShared.NumCars;
                    RaceData.EstimatedStandings.Add(me);
                    //log.Debug("last");
                }
            }
        }

        private void calcBoxenstopDelta()
        {
            // sector3 and sector1 diff to pb
            if (ActualData.PreviousLap.RelSector3 != DisplayData.INVALID)
            {
                RaceData.LastBoxenstopDelta = ActualData.PreviousLap.RelSector3 - ActualData.PBLap.RelSector3 + ActualData.CurrentLap.Sector1 - ActualData.PBLap.Sector1;
            }
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
                    RaceData.DiffSectorsAhead = CurrentCompleteLap - R3EUtils.SectorsToLap(ActualShared.DriverData[ActualShared.Position - 2].SectorTimePreviousSelf);
                }
                else
                {
                    RaceData.DiffSectorsAhead = CurrentCompleteLap - R3EUtils.SectorsToLap(ActualShared.DriverData[ActualShared.Position - 2].SectorTimeCurrentSelf);
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
                    this.RaceData.DiffSectorsBehind = CurrentCompleteLap - R3EUtils.SectorsToLap(driverBehind.SectorTimePreviousSelf);
                }
                else if (IsInSector(driverBehind) != 0)
                {
                    this.RaceData.DiffSectorsBehind = CurrentCompleteLap - R3EUtils.SectorsToLap(driverBehind.SectorTimeCurrentSelf);
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
                AllFuelUsage += ActualData.FuelLastLap;
                log.Info("Fuel usage last lap: " + ActualData.FuelLastLap + ", Fuel usage all: " + AllFuelUsage + ", Fuel left: " + ActualShared.FuelLeft);
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
                    else if (ActualShared.SessionLengthFormat == 2)
                    {
                        // time based with extra lap
                        restlaps = (int)(ActualShared.SessionTimeRemaining / fastestLap) + 2;
                        if (LastLap)
                        {
                            restlaps = 1;
                        }
                    }

                    if (restlaps == 1)
                    {
                        log.Info("Last Lap");
                        LastLap = true;
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
                // reset lap valid and update first sector valid
                ResetLapValid();
                ActualData.CurrentLapValid[0] = FirstSectorValid;
                ActualData.CurrentLapValid[3] = FirstSectorValid;

                // reset first sector
                FirstSectorValid = true;
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
            // from PB
            for (int i = 0; i < 3; i++)
            {
                float sec = ActualData.PBLap.RelArray[i];
                var tbSec = ActualData.TBLap.RelArray[i];


                if (sec != DisplayData.INVALID && (sec < tbSec || tbSec == DisplayData.INVALID))
                {
                    log.Info("New TB in Sector: " + (i + 1));
                    ActualData.TBLap.SetRelSector(i, sec);
                }
            }

            // from current lap
            for (int i = 0; i < 3; i++)
            {
                float sec = ActualData.CurrentLap.RelArray[i];
                var tbSec = ActualData.TBLap.RelArray[i];
                
                
                if (!CurrentLapOwnCalculation[i] && this.ActualData.CurrentLapCompleted[i] && ActualData.CurrentLapValid[i] && sec != DisplayData.INVALID && (sec < tbSec || tbSec == DisplayData.INVALID))
                {
                    if (i==2 && isWarmupLap)
                    {
                        // last sector in warmup lap is wrong
                        continue;
                    }

                    log.Info("New TB in Sector: " + (i + 1));
                    ActualData.TBLap.SetRelSector(i, sec);
                }
            }

        }

        private void UpdateTBFromPrevious()
        {
            // from Previous
            for (int i = 0; i < 3; i++)
            {
                float sec = ActualData.PreviousLap.RelArray[i];
                var tbSec = ActualData.TBLap.RelArray[i];


                if (sec != DisplayData.INVALID && (sec < tbSec || tbSec == DisplayData.INVALID))
                {
                    log.Info("New TB in Sector: " + (i + 1));
                    ActualData.TBLap.SetRelSector(i, sec);
                }
            }
        }

        /// <summary>
        /// Updates the current lap, so that the time is running in sector 2 and 3. In the first sector all the sector times from the last lap shall be shown.
        /// </summary>
        private void updateCurrentLap()
        {
            UpdateLiveTiming();

            // dismiss sector3 time from shared if in -1. lap of the race (start grid to race line)
            if (isWarmupLap && ActualShared.CompletedLaps == 0)
            {
                ActualData.CurrentLap = new Model.Lap();
            }
        }

        private void UpdateLiveTiming()
        {
            if (IsFirstSector)
            {
                return;
            }

            if (LapTimeCurrentSelf != DisplayData.INVALID_POSITIVE && !OwnSectorCalculationNeeded())
            {
                if (ActualData.CurrentSector == 1 && ActualShared.SectorTimesCurrentSelf.AbsSector2 == DisplayData.INVALID_POSITIVE && CurrentCompleteLap.Sector1 != DisplayData.INVALID)
                {
                    ActualData.CurrentLap.RelSector2 = LapTimeCurrentSelf - CurrentCompleteLap.Sector1;
                }
                else if (ActualData.CurrentSector == 2 && ActualShared.SectorTimesCurrentSelf.AbsSector3 == DisplayData.INVALID_POSITIVE && CurrentCompleteLap.AbsSector2 != DisplayData.INVALID)
                {
                    if (LapTimeCurrentSelf < 5)
                    {
                        // it is not possible that the driver is in sector 3!
                        return;
                    }

                    ActualData.CurrentLap.RelSector3 = LapTimeCurrentSelf - CurrentCompleteLap.AbsSector2;
                }
            }
            else if (StartLastSectorTime != DisplayData.INVALID_POSITIVE)
            {
                // calc on own
                float secTime = (float)(ActualShared.Player.GameSimulationTime - StartLastSectorTime);
                
                if (ActualData.CurrentSector == 0)
                {
                    ActualData.CurrentTime = secTime;
                }
                else if (ActualData.CurrentSector == 1)
                {
                    ActualData.CurrentLap.RelSector2 = secTime;
                    if (ActualData.CurrentLap.Sector1 != DisplayData.INVALID)
                    {
                        ActualData.CurrentTime = secTime + ActualData.CurrentLap.Sector1;
                    }
                }
                else if (ActualData.CurrentSector == 2)
                {
                    ActualData.CurrentLap.RelSector3 = secTime;

                    if (ActualData.CurrentLap.AbsSector2 != DisplayData.INVALID)
                    {
                        ActualData.CurrentTime = secTime + ActualData.CurrentLap.AbsSector2;
                    }
                }

                
            }
            
        }

        private bool isNewSector()
        {
            // Track limits not loadad or last sector invalid
            if (Track == null || LastShared.TrackSector == DisplayData.INVALID_INT)
            {
                return LastShared.TrackSector != ActualShared.TrackSector;
            }
            else
            {
                return LastSector != ActualData.CurrentSector;
            }
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
