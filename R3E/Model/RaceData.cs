using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    class RaceData
    {
        /// <summary>
        /// Diff to driver ahead.
        /// </summary>
        public Single DiffAhead { get; set; }

        /// <summary>
        /// Diff to driver behind.
        /// </summary>
        public Single DiffBehind { get; set; }

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
    }
}
