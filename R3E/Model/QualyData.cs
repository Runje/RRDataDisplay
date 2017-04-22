using System;
using System.Collections.Generic;
using System.IO;
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

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            for (int i = 0; i < SectorPos.Length; i++)
            {
                writer.Write(SectorPos[i]);
            }
        }

        public override int Length()
        {
            return base.Length() + SectorPos.Length * 8;
        }
    }
}
