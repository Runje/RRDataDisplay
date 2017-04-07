using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class Tires
    {
        public Single FrontLeft;
        public Single FrontRight;
        public Single RearLeft;
        public Single RearRight;

        public Single[] Array
        {
            get
            {
                return new Single[] { FrontLeft, FrontRight, RearLeft, RearRight };
            }
        }

        public Tires()
        {
            FrontLeft = -1;
            FrontRight= -1;
            RearLeft = -1;
            RearRight = -1;
        }
    }
}
