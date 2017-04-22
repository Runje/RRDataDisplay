using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R3E.Data;
using R3E.Model;

namespace R3E
{
    public class R3EUtils
    {
        public static Lap SectorsToLap(Sectors<float> sectors)
        {
            float sec1 = sectors.AbsSector1 == DisplayData.INVALID_POSITIVE ? DisplayData.INVALID : sectors.AbsSector1;
            float relSec2 = sectors.AbsSector2 - sectors.AbsSector1;
            if (sectors.AbsSector2 == DisplayData.INVALID_POSITIVE)
            {
                relSec2 = DisplayData.INVALID;
            }

            float relSec3 = sectors.AbsSector3 - sectors.AbsSector2;
            if (sectors.AbsSector3 == DisplayData.INVALID_POSITIVE)
            {
                relSec3 = DisplayData.INVALID;
            }
            return new Lap(sec1, relSec2, relSec3);
        }

        internal static Tires R3ETiresToTires(TireData tireWear)
        {
            var tires = new Tires();
            tires.FrontLeft = tireWear.FrontLeft;
            tires.FrontRight = tireWear.FrontRight;
            tires.RearLeft = tireWear.RearLeft;
            tires.RearRight = tireWear.RearRight;
            return tires;
        }
    }
}
