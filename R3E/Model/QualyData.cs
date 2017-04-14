using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class QualyData : DisplayData
    {
        /// <summary>
        /// Current Position in the sectors and a forecast for final position if lap is not completed.
        /// </summary>
        public int[] SectorPos { get; set; }

        public QualyData() : base()
        {
            SectorPos = new int[] { DisplayData.INVALID_INT, DisplayData.INVALID_INT, DisplayData.INVALID_INT, DisplayData.INVALID_INT };
        }
    }
}
