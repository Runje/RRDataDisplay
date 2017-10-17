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
        /// Boxenstop Suggestion.
        /// </summary>
        public BoxenstopSuggestion BoxenstopSuggestion{ get; set; }

        /// <summary>
        /// Estimated standings after boxenstop.
        /// </summary>
        public List<StandingsDriver> EstimatedStandings { get; set; }

        /// <summary>
        /// Actual Race standings.
        /// </summary>
        public List<StandingsDriverExtended> RaceStandings { get; set; }

        /// <summary>
        /// Last Boxenstop delta in seconds.
        /// </summary>
        public Single LastBoxenstopDelta { get; set; }

        /// <summary>
        /// Standing Time of last boxenstop
        /// </summary>
        public float LastStandingTime { get; set; }

        /// <summary>
        /// Total amount of used fuel.
        /// </summary>
        public float FuelUsed { get; set; }

        public RaceData() : base()
        {
            DiffAhead = DisplayData.INVALID;
            DiffBehind = DisplayData.INVALID;
            DiffSectorsAhead = new Lap();
            DiffSectorsBehind = new Lap();
            FuelToRefill = DisplayData.INVALID_POSITIVE;
            EstimatedRaceLaps = DisplayData.INVALID_INT;
            BoxenstopSuggestion = new BoxenstopSuggestion();
            LastBoxenstopDelta = DisplayData.INVALID_POSITIVE;
            EstimatedStandings = new List<StandingsDriver>();
            RaceStandings = new List<StandingsDriverExtended>();
            LastStandingTime = DisplayData.INVALID_POSITIVE;
            FuelUsed = DisplayData.INVALID_POSITIVE;
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
            BoxenstopSuggestion.Write(writer);
            writer.Write(LastBoxenstopDelta);
            writer.Write(EstimatedStandings.Count);
            // TODO: estimated standings kann sich ändern, LOCK IT
            foreach (var driver in EstimatedStandings)
            {
                driver.Write(writer);
            }

            writer.Write(RaceStandings.Count);
            foreach (var driver in RaceStandings)
            {
                driver.Write(writer);
            }

            writer.Write(LastStandingTime);
            writer.Write(FuelUsed);
        }

        public override int Length()
        {
            int EstimatedStandingsLength = 0;
            foreach (var driver in EstimatedStandings)
            {
                EstimatedStandingsLength += driver.Length;
            }

            return base.Length() + 2 * 8 + 2 * Lap.Length + 8 + 4 + BoxenstopSuggestion.Length + 8 + 4 + EstimatedStandingsLength + 8 + 8;
        }
    }
}
