using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class RaceData : DisplayData
    {
        /// <summary>
        /// Actual diff to driver ahead.
        /// </summary>
        public Single DiffAhead { get; set; }

        /// <summary>
        /// Actual diff to driver behind.
        /// </summary>
        public Single DiffBehind { get; set; }

        /// <summary>
        /// Diffs in the sector times to the driver ahead.
        /// </summary>
        public Lap DiffSectorsAhead { get; set; }

        /// <summary>
        /// Diffs in the sector times to the driver behind.
        /// </summary>
        public Lap DiffSectorsBehind{ get; set; }

        /// <summary>
        /// How much fuel the driver has to refill to finish the race.
        /// </summary>
        public Single FuelToRefill { get; set; }

        /// <summary>
        /// Estimated total laps in this race.
        /// </summary>
        public int EstimatedRaceLaps { get; set; }

        /// <summary>
        /// Estimated boxenstop delta in seconds.
        /// </summary>
        public Single EstimatedBoxenstopDelta { get; set; }

        /// <summary>
        /// Last Boxenstop delta in seconds.
        /// </summary>
        public Single LastBoxenstopDelta { get; set; }

        public RaceData() : base()
        {
            DiffAhead = DisplayData.INVALID;
            DiffBehind = DisplayData.INVALID;
            DiffSectorsAhead = new Lap();
            DiffSectorsBehind = new Lap();
            FuelToRefill = DisplayData.INVALID_POSITIVE;
            EstimatedRaceLaps = DisplayData.INVALID_INT;
            EstimatedBoxenstopDelta = DisplayData.INVALID_POSITIVE;
            LastBoxenstopDelta = DisplayData.INVALID_POSITIVE;
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(DiffAhead);
            writer.Write(DiffBehind);
            DiffSectorsAhead.Write(writer);
            DiffSectorsBehind.Write(writer);
            writer.Write(FuelToRefill);
            writer.Write(EstimatedRaceLaps);
            writer.Write(EstimatedBoxenstopDelta);
            writer.Write(LastBoxenstopDelta);
        }

        public override int Length()
        {
            return base.Length() + 2 * 8 + 2 * Lap.Length + 8 + 4 + 2 * 8;
        }
    }
}
