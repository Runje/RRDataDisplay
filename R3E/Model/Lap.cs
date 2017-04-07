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
        public Single AbsSector2 { get; set; }
        public Single Time { get; set; }

        public Single RelSector2
        {
            get
            {
                if (AbsSector2 == DisplayData.INVALID_POSITIVE)
                {
                    return DisplayData.INVALID_POSITIVE;
                }

                return AbsSector2 - Sector1;
            }
        }

        public Single RelSector3
        {
            get
            {
                if (Time == DisplayData.INVALID_POSITIVE)
                {
                    return DisplayData.INVALID_POSITIVE;
                }

                return Time - AbsSector2;
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
            AbsSector2 = DisplayData.INVALID_POSITIVE;
            Time = DisplayData.INVALID_POSITIVE;
        }


    }
}
