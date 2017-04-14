using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class Lap
    {
        public Single Sector1 { get; set; }
        public Single RelSector2 { get; set; }
        public Single RelSector3 { get; set; }

        public Single AbsSector2
        {
            get
            {
                if (RelSector2 == DisplayData.INVALID_POSITIVE || Sector1 == DisplayData.INVALID_POSITIVE)
                {
                    return DisplayData.INVALID_POSITIVE;
                }

                return Sector1 + RelSector2;
            }
        }

        public Single Time
        {
            get
            {
                if (RelSector2 == DisplayData.INVALID_POSITIVE || Sector1 == DisplayData.INVALID_POSITIVE || RelSector3 == DisplayData.INVALID_POSITIVE)
                {
                    return DisplayData.INVALID_POSITIVE;
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
            Sector1 = DisplayData.INVALID_POSITIVE;
            RelSector2 = DisplayData.INVALID_POSITIVE;
            RelSector3 = DisplayData.INVALID_POSITIVE;
        }

        public Lap(float absSector1, float relSec2, float relSec3)
        {
            this.Sector1 = absSector1;
            this.RelSector2 = relSec2;
            this.RelSector3 = relSec3;
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
    }
}
