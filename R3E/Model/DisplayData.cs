using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class DisplayData
    {
        /// <summary>
        /// Invalid magic number for positive int values
        /// </summary>
        public const int INVALID_INT = -1;

        /// <summary>
        /// Invald magic number for floats.
        /// </summary>
        public const float INVALID = float.MinValue;

        /// <summary>
        /// Invald magic number for positive floats.
        /// </summary>
        public const float INVALID_POSITIVE = -1;

        /// <summary>
        /// Driver Name.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Position( 1 == first place) of player.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Name of the track.
        /// </summary>
        public String Track { get; set; }

        /// <summary>
        /// Name of the layout.
        /// </summary>
        public String Layout { get; set; }

        /// <summary>
        /// Current running lap time.
        /// </summary>
        public Single CurrentTime { get; set; }

        /// <summary>
        /// Current lap time in seconds of player.
        /// </summary>
        public Lap CurrentLap { get; set; }

        internal byte ToType()
        {
            byte type = 0;
            if (this is QualyData)
            {
                type = 1;
            }
            else if (this is RaceData)
            {
                type = 2;
            }

            return type;
        }

        /// <summary>
        /// Whether this sector time in CurrentLap are completed
        /// </summary>
        public bool[] CurrentLapCompleted { get; set; }

        /// <summary>
        /// Whether this sector time in CurrentLap are valid
        /// </summary>
        public bool[] CurrentLapValid{ get; set; }

        /// <summary>
        /// Previous lap time in seconds of player.
        /// </summary>
        public Lap PreviousLap { get; set; }

        /// <summary>
        /// Personal best lap time in seconds in this session of player.
        /// </summary>
        public Lap PBLap { get; set; }

        /// <summary>
        /// Theoretical best lap time in seconds of player.
        /// </summary>
        public Lap TBLap { get; set; }

        /// <summary>
        /// Fastest lap time in seconds of anyone.
        /// </summary>
        public Lap FastestLap { get; set; }

        /// <summary>
        /// How many complete laps can be driven after completing this lap.
        /// </summary>
        public int FuelRemainingLaps { get; set; }

        /// <summary>
        /// Tires wear. 1.0 == 100%, 0 = 0%.
        /// </summary>
        public Tires TiresWear { get; set; }

        /// <summary>
        /// Fuel used last lap in litre.
        /// </summary>
        public Single FuelLastLap { get; set; }

        /// <summary>
        /// Average fuel used per lap in litre.
        /// </summary>
        public Single FuelAveragePerLap { get; set; }

        /// <summary>
        /// Maximum of fuel used in one lap in litre.
        /// </summary>
        public Single FuelMaxLap { get; set; }

        /// <summary>
        /// Completed Laps in this Session so far.
        /// </summary>
        public List<Lap> CompletedLaps { get; set; }

        /// <summary>
        /// Number of completed laps, can be more than the number of items in CompletedLaps in case of missing data.
        /// </summary>
        public int CompletedLapsCount { get; set; }

        /// <summary>
        /// Tire usage in percent(1 = 100%) per lap.
        /// </summary>
        public Tires TireUsedAveragePerLap { get; set; }

        /// <summary>
        /// Tire usage in percent(1 = 100%) per lap.
        /// </summary>
        public Tires TireUsedLastLap { get; set; }

        /// <summary>
        /// Max Tire usage in percent(1 = 100%) per lap.
        /// </summary>
        public Tires TireUsedMaxLap { get; set; }

        /// <summary>
        /// Current Sector of the player.
        /// </summary>
        public int CurrentSector { get; internal set; }

        /// <summary>
        /// The fraction of the lap.
        /// </summary>
        public float LapDistanceFraction { get; set; }

        public DisplayData()
        {
            Position = INVALID_INT;
            CurrentTime = INVALID;
            CurrentLap = new Lap();
            CurrentLapCompleted = new bool[] { false, false, false, false };
            CurrentLapValid = new bool[] { true, true, true, true };
            PreviousLap = new Lap();
            PBLap = new Lap();
            TBLap = new Lap();
            FastestLap = new Lap();
            FuelRemainingLaps = INVALID_INT;
            TiresWear = new Tires();
            FuelLastLap = INVALID_POSITIVE;
            FuelAveragePerLap = INVALID_POSITIVE;
            FuelMaxLap = INVALID_POSITIVE;
            CompletedLaps = new List<Lap>();
            TireUsedAveragePerLap = new Tires();
            TireUsedLastLap = new Tires();
            TireUsedMaxLap = new Tires();
            CurrentSector = INVALID_INT;
            LapDistanceFraction = INVALID_POSITIVE;
        }

        public virtual void Write(BinaryWriter writer)
        {
            writer.Write(Position);
            writer.Write(CurrentTime);
            CurrentLap.Write(writer);
            for (int i = 0; i < CurrentLapCompleted.Length; i++)
            {
                writer.Write(CurrentLapCompleted[i]);
            }
            for (int i = 0; i < CurrentLapValid.Length; i++)
            {
                writer.Write(CurrentLapValid[i]);
            }
            PreviousLap.Write(writer);
            PBLap.Write(writer);
            TBLap.Write(writer);
            FastestLap.Write(writer);
            writer.Write(FuelRemainingLaps);
            TiresWear.Write(writer);
            writer.Write(FuelLastLap);
            writer.Write(FuelAveragePerLap);
            writer.Write(FuelMaxLap);
            writer.Write(CompletedLaps.Count);
            for (int i = 0; i < CompletedLaps.Count; i++)
            {
                CompletedLaps[i].Write(writer);
            }
            writer.Write(CompletedLapsCount);
            TireUsedAveragePerLap.Write(writer);
            TireUsedLastLap.Write(writer);
            TireUsedMaxLap.Write(writer);
            writer.Write(CurrentSector);
            writer.Write(Utilities.stringToBytes(Track));
            writer.Write(Utilities.stringToBytes(Layout));
            writer.Write(LapDistanceFraction);
            writer.Write(Utilities.stringToBytes(Name));
        }

        public virtual int Length()
        {
            return 4 + 8 + 5 * Lap.Length + CurrentLapCompleted.Length + 4 + 3 * 8 + 4 + CompletedLaps.Count * Lap.Length + 4 + 3 * Tires.Length + 8 + Track.Length + 1 + Layout.Length + 1 + 4 + Name.Length + 1;
        }
    }
}
