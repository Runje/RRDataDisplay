using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class Tires
    {
        public static int Length { get { return 4 * 8; }}
        public bool Valid { get { return FrontLeft != DisplayData.INVALID_POSITIVE && FrontRight != DisplayData.INVALID_POSITIVE && RearLeft != DisplayData.INVALID_POSITIVE && RearRight != DisplayData.INVALID_POSITIVE; } }
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

        public Tires(float frontLeft, float frontRight, float rearLeft, float rearRight)
        {
            this.FrontLeft = frontLeft;
            this.FrontRight = frontRight;
            this.RearLeft = rearLeft;
            this.RearRight = rearRight;
        }

        public Tires(BinaryReader reader)
        {
            this.FrontLeft = reader.ReadSingle();
            this.FrontRight = reader.ReadSingle();
            this.RearLeft = reader.ReadSingle();
            this.RearRight = reader.ReadSingle();
        }

        public static Tires operator +(Tires a, Tires b)
        {
            if (!a.Valid || !b.Valid)
            {
                throw new ArgumentException("Invalid tires argument");
            }

            return new Tires(a.FrontLeft + b.FrontLeft, a.FrontRight + b.FrontRight, a.RearLeft + b.RearLeft, a.RearRight + b.RearRight);
        }

        public static Tires operator -(Tires a, Tires b)
        {
            if (!a.Valid || !b.Valid)
            {
                throw new ArgumentException("Invalid tires argument");
            }

            return new Tires(a.FrontLeft - b.FrontLeft, a.FrontRight - b.FrontRight, a.RearLeft - b.RearLeft, a.RearRight - b.RearRight);
        }

        public override string ToString()
        {
            return string.Format("FL: {0} FR: {1} RL: {2} RR: {3}", Utilities.floatToString(FrontLeft), Utilities.floatToString(FrontRight), Utilities.floatToString(RearLeft), Utilities.floatToString(RearRight));
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(FrontLeft);
            writer.Write(FrontRight);
            writer.Write(RearLeft);
            writer.Write(RearRight);
        }
    }
}
