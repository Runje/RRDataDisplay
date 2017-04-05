using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class QualyData : Data
    {
        /// <summary>
        /// Position in sector 1.
        /// </summary>
        public int Sector1Pos { get; set; }

        /// <summary>
        /// Position in sector 2.
        /// </summary>
        public int Sector2Pos { get; set; }

        /// <summary>
        /// Position in sector 3.
        /// </summary>
        public int Sector3Pos { get; set; }

        /// <summary>
        /// Position forecast of current lap.
        /// </summary>
        public int ForecastPos { get; set; }
    }
}
