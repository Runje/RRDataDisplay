using R3E.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E
{
    class R3EMessage
    {
        public Single FrontLeft_Center;

        public Single FrontRight_Center;

        public Single RearLeft_Center;

        public Single RearRight_Center;

        public Single FrontLeftWear;
        public Single FrontRightWear;
        public Single RearLeftWear;
        public Single RearRightWear;

        public byte[] TrackName; // UTF-8
        public byte[] LayoutName; // UTF-8

        int SessionType;

        int RaceLaps;

        public float SessionTimeRemaining { get; private set; }
        public int Position { get; private set; }
        public int CompletedLaps { get; private set; }
        public float LapDistanceFraction { get; private set; }
        public float LapTimeBestLeaderClass { get; private set; }
        public byte[] PlayerName { get; private set; }
        public float CarSpeed { get; private set; }
        public float TimeDeltaAhead { get; private set; }
        public float TimeDeltaBehind { get; private set; }
        public float FuelCapacity { get; private set; }
        public float FuelLeft { get; private set; }
        public float RearRightGrip { get; private set; }
        public float RearLeftGrip { get; private set; }
        public float FrontRightGrip { get; private set; }
        public float FrontLeftGrip { get; private set; }
        public int NumCars { get; private set; }
        public DriverData[] Drivers { get; private set; }
        public Sectors<float> SectorTimesBestSelf { get; private set; }
        public Sectors<float> SectorTimesCurrSelf { get; private set; }
        public float LapDistance { get; private set; }
        public int ControlType { get; private set; }
        public int InMenu { get; private set; }
        public int Paused { get; private set; }
        public float LayoutLength { get; private set; }

        public Sectors<Single> SectorTimesSessionBestLap;

        public int ClassId;
        public int ModelId;

        public int SessionPhase;

        public R3EMessage(Shared shared)
        {
            FrontLeft_Center = shared.TireTemp.FrontLeft_Center;
            FrontRight_Center = shared.TireTemp.FrontRight_Center;
            RearLeft_Center = shared.TireTemp.RearLeft_Center;
            RearRight_Center= shared.TireTemp.RearRight_Center;

            FrontLeftWear = shared.TireWear.FrontLeft;
            FrontRightWear = shared.TireWear.FrontRight;
            RearLeftWear = shared.TireWear.RearLeft;
            RearRightWear = shared.TireWear.RearRight;

            FrontLeftGrip = shared.TireGrip.FrontLeft;
            FrontRightGrip = shared.TireGrip.FrontRight;
            RearLeftGrip = shared.TireGrip.RearLeft;
            RearRightGrip = shared.TireGrip.RearRight;

            TrackName = shared.TrackName;
            LayoutName = shared.LayoutName;

            SessionType = shared.SessionType;
            RaceLaps = shared.NumberOfLaps;
            CompletedLaps = shared.CompletedLaps;
            SessionTimeRemaining = shared.SessionTimeRemaining;
            Position = shared.Position;
            LapDistanceFraction = shared.LapDistanceFraction;
            LapTimeBestLeaderClass = shared.LapTimeBestLeaderClass;
            SectorTimesSessionBestLap = shared.SectorTimesSessionBestLap;
            SectorTimesBestSelf = shared.SectorTimesBestSelf;

            
            
            SectorTimesCurrSelf = shared.SectorTimesCurrentSelf;
            // don't count last sector of formation lap
            if (shared.CompletedLaps == 0)
            {
                var test = new Sectors<float>();
                test.AbsSector1 = SectorTimesCurrSelf.AbsSector1;
                test.AbsSector2 = SectorTimesCurrSelf.AbsSector2;
                test.AbsSector3 = -1;
                SectorTimesCurrSelf = test;
            }

            ClassId = shared.VehicleInfo.ClassId;
            ModelId = shared.VehicleInfo.ModelId;
            SessionPhase = shared.SessionPhase;
            PlayerName = shared.PlayerName;
            CarSpeed = shared.CarSpeed;

            FuelLeft = shared.FuelLeft;
            FuelCapacity = shared.FuelCapacity;

            NumCars = shared.NumCars;
            Drivers = shared.DriverData;
            TimeDeltaAhead = shared.TimeDeltaFront;
            TimeDeltaBehind = shared.TimeDeltaBehind;
            LapDistance = shared.LapDistance;
            InMenu = shared.GameInMenus;
            Paused = shared.GamePaused;
            ControlType = shared.ControlType;
            LayoutLength = shared.LayoutLength;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(FrontLeft_Center);
            writer.Write(FrontRight_Center);
            writer.Write(RearLeft_Center);
            writer.Write(RearRight_Center);

            writer.Write(FrontLeftWear);
            writer.Write(FrontRightWear);
            writer.Write(RearLeftWear);
            writer.Write(RearRightWear);

            writer.Write(FrontLeftGrip);
            writer.Write(FrontRightGrip);
            writer.Write(RearLeftGrip);
            writer.Write(RearRightGrip);

            writer.Write(TrackName);
            writer.Write(LayoutName);
            writer.Write(SessionType);
            writer.Write(RaceLaps);
            writer.Write(CompletedLaps);
            writer.Write(SessionTimeRemaining);
            writer.Write(Position);
            writer.Write(LapDistanceFraction);
            writer.Write(LapDistance);
            writer.Write(LapTimeBestLeaderClass);
            writeRelSectors(SectorTimesSessionBestLap, writer);
            writeRelSectors(SectorTimesBestSelf, writer);

            
            writeRelSectors(SectorTimesCurrSelf, writer);
            
            writer.Write(ClassId);
            writer.Write(ModelId);
            writer.Write(SessionPhase);
            writer.Write(PlayerName);
            writer.Write(CarSpeed);
            writer.Write(FuelLeft);
            writer.Write(FuelCapacity);
            writer.Write(TimeDeltaAhead);
            writer.Write(TimeDeltaBehind);
            writer.Write(InMenu);
            writer.Write(Paused);
            writer.Write(ControlType);
            writer.Write(LayoutLength);

            writer.Write(NumCars);
            for (int i = 0; i < Drivers.Length; i++)
            {
                new R3EDriver(Drivers[i]).Write(writer);
            }
        }

        public static void writeRelSectors(Sectors<float> sectors, BinaryWriter writer)
        {
            writer.Write(sectors.AbsSector1);
            writer.Write(sectors.AbsSector2 == -1 ? -1 : sectors.AbsSector2 - sectors.AbsSector1);
            writer.Write(sectors.AbsSector3 == -1 ? -1 : sectors.AbsSector3 - sectors.AbsSector2);
        }
    }
}
