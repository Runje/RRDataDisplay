using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class Data
    {
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
        public Tires TireUsePerKM { get; set; }

        /// <summary>
        /// Tire usage in percent(1 = 100%) per 1000 m.
        /// </summary>
        public Tires TireUsedLastLap { get; set; }
    }
}
