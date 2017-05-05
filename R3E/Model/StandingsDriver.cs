using System;
using System.IO;

namespace R3E.Model
{
    public class StandingsDriver
    {
        public int Pos { get; set; }
        public float Delta { get; set; }
        public string Name { get; set; }
        public int Length { get { return 4 + 8 + Name.Length + 1; } }
        public StandingsDriver(int pos, float delta, string name)
        {
            this.Pos = pos;
            this.Delta = delta;
            this.Name = name;
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(Pos);
            writer.Write(Delta);
            writer.Write(Utilities.stringToBytes(Name));
        }
    }
}