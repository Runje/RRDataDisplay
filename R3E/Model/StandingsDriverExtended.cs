using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class StandingsDriverExtended : StandingsDriver
    {
        public int Laps { get; set; }
        public int Pitstops { get; set; }
        public float PB { get; set; }
        public float TB { get; set; }
        public StandingsDriverExtended(int pos, float delta, string name, int laps, int pitstops, float pb, float tb) : base(pos, delta, name)
        {
            this.Laps = laps;
            this.Pitstops = pitstops;
            this.PB = pb;
            this.TB = tb;
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Laps);
            writer.Write(Pitstops);
            writer.Write(PB);
            writer.Write(TB);
        }
    }
}
