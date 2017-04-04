using R3E.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E
{
    class R3EDriver
    {
        public int place;
        public int CompletedLaps { get; private set; }
        public byte[] Name { get; private set; }
        public int NumPitstops { get; private set; }
        public Sectors<float> SectorTimeBestSelf { get; private set; }
        public Sectors<float> SectorTimeCurrentSelf { get; private set; }
        public Sectors<float> SectorTimePreviousSelf{ get; private set; }
        public float TimeDeltaBehind { get; private set; }
        public float TimeDeltaAhead { get; private set; }
        public float LapDistance { get; private set; }

        public R3EDriver(DriverData data)
        {
            Name = data.DriverInfo.Name;
            CompletedLaps = data.CompletedLaps;
            NumPitstops = data.NumPitstops;
            place = data.Place;
            SectorTimeBestSelf = data.SectorTimeBestSelf;
            SectorTimeCurrentSelf = data.SectorTimeCurrentSelf;
            SectorTimePreviousSelf = data.SectorTimePreviousSelf;
            TimeDeltaBehind = data.TimeDeltaBehind;
            TimeDeltaAhead = data.TimeDeltaFront;
            LapDistance = data.LapDistance;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(place);
            writer.Write(CompletedLaps);
            writer.Write(Name);
            writer.Write(NumPitstops);
            R3EMessage.writeRelSectors(SectorTimeBestSelf, writer);
            UpdateCurrentSectors();
            R3EMessage.writeRelSectors(SectorTimeCurrentSelf, writer);
            writer.Write(TimeDeltaBehind);
            writer.Write(TimeDeltaAhead);
            writer.Write(LapDistance);
        }

        private void UpdateCurrentSectors()
        {
            if (SectorTimeCurrentSelf.AbsSector1 == SectorTimePreviousSelf.AbsSector1 && SectorTimeCurrentSelf.AbsSector2 == SectorTimePreviousSelf.AbsSector2)
            {
                SectorTimeCurrentSelf = SectorTimePreviousSelf;
            }
        }
    }
}
