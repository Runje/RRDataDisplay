using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class BoxenstopSuggestion
    {
        public const int Length = 1 + 8 + 1 + 1 + 8;

        public bool Yes { get; set; }
        public float EstimatedDelta { get; set; }
        public bool FrontTires { get; set; }
        public bool RearTires { get; set; }
        public float Refill { get; set; }

        public BoxenstopSuggestion(bool yes, float estimatedDelta, bool frontTires, bool rearTires, float refill)
        {
            this.Yes = yes;
            this.EstimatedDelta = estimatedDelta;
            this.FrontTires = frontTires;
            this.RearTires = rearTires;
            this.Refill = refill;
        }

        public BoxenstopSuggestion()
        {
            Yes = false;
            EstimatedDelta = DisplayData.INVALID_POSITIVE;
            FrontTires = false;
            RearTires = false;
            Refill = DisplayData.INVALID_POSITIVE;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Yes);
            writer.Write(EstimatedDelta);
            writer.Write(FrontTires);
            writer.Write(RearTires);
            writer.Write(Refill);
        }
    }
}
