using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class Lap
    {
        public static int Length { get { return 3 * 8; }}
        public Single Sector1 { get; set; }
        public Single RelSector2 { get; set; }
        public Single RelSector3 { get; set; }

        public Single AbsSector2
        {
            get
            {
                if (RelSector2 == DisplayData.INVALID || Sector1 == DisplayData.INVALID)
                {
                    return DisplayData.INVALID;
                }

                return Sector1 + RelSector2;
            }
        }

        public Single Time
        {
            get
            {
                if (RelSector2 == DisplayData.INVALID || Sector1 == DisplayData.INVALID || RelSector3 == DisplayData.INVALID)
                {
                    return DisplayData.INVALID;
                }

                return Sector1 + RelSector2 + RelSector3;
            }
        }

        public Single[] RelArray
        {
            get
            {
                return new Single[] { Sector1, RelSector2, RelSector3, Time };
            }
        }

        public Lap()
        {
            Sector1 = DisplayData.INVALID;
            RelSector2 = DisplayData.INVALID;
            RelSector3 = DisplayData.INVALID;
        }

        public Lap(float absSector1, float relSec2, float relSec3)
        {
            this.Sector1 = absSector1;
            this.RelSector2 = relSec2;
            this.RelSector3 = relSec3;
        }

        public Lap(BinaryReader reader)
        {
            Sector1 = reader.ReadSingle();
            RelSector2 = reader.ReadSingle();
            RelSector3 = reader.ReadSingle();
        }

        internal void SetRelSector(int i, float sec)
        {
            if (i == 0)
            {
                Sector1 = sec;
            }
            else if (i == 1)
            {
                RelSector2 = sec;
            }
            else if (i == 2)
            {
                RelSector3 = sec;
            }
            else
            {
                throw new ArgumentException("Index > 2: " + i);
            }
        }

        public static Lap operator -(Lap a, Lap b)
        {
            Lap result = new Model.Lap();
            if (a.Sector1 != DisplayData.INVALID && b.Sector1 != DisplayData.INVALID)
            {
                result.Sector1 = a.Sector1 - b.Sector1;
            }

            if (a.RelSector2 != DisplayData.INVALID && b.RelSector2 != DisplayData.INVALID)
            {
                result.RelSector2 = a.RelSector2 - b.RelSector2;
            }

            if (a.RelSector3 != DisplayData.INVALID && b.RelSector3 != DisplayData.INVALID)
            {
                result.RelSector3 = a.RelSector3 - b.RelSector3;
            }

            return result;
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(Sector1);
            writer.Write(RelSector2);
            writer.Write(RelSector3);
        }

        

    }
}
