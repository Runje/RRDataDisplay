using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class DisplayData
    {
        /// <summary>
        /// Invalid magic number for positive values
        /// </summary>
        public const int INVALID_INT = -1;

        /// <summary>
        /// Invald magic number for floats.
        /// </summary>
        public const float INVALID = float.MinValue;

        /// <summary>
        /// Invald magic number for positive floats.
        /// </summary>
        public const float INVALID_POSITIVE = float.MinValue;

        /// <summary>
        /// Position( 1 == first place) of player.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Current lap time in seconds of player.
        /// </summary>
        public Lap CurrentLap { get; set; }

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
        public Lap[] CompletedLaps { get; set; }

        /// <summary>
        /// Tire usage in percent(1 = 100%) per 1000 m.
        /// </summary>
        public Tires AverageTireUsePerKM { get; set; }

        /// <summary>
        /// Tire usage in percent(1 = 100%) per 1000 m.
        /// </summary>
        public Tires TireUsedLastLap { get; set; }


        private const int MaxLapsCount = 200;
        public DisplayData()
        {
            Position = INVALID_INT;
            CurrentLap = new Lap();
            PreviousLap = new Lap();
            PBLap = new Lap();
            TBLap = new Lap();
            PreviousLap = new Lap();
            FastestLap = new Lap();
            FuelRemainingLaps = INVALID_INT;
            TiresWear = new Tires();
            FuelLastLap = INVALID_POSITIVE;
            FuelAveragePerLap = INVALID_POSITIVE;
            FuelMaxLap = INVALID_POSITIVE;
            CompletedLaps = new Lap[MaxLapsCount];
            AverageTireUsePerKM = new Tires();
            TireUsedLastLap = new Tires();
        }
    }
}
