using System;

namespace R3E.Model
{
    public class LapInfo
    {
        public float LapTime;
        public float LapCount;
        public string Session;
        public string Track;
        public DateTime Start;

        public LapInfo(float lapTime, float lapCount, string session, string track, DateTime start)
        {
            this.LapTime = lapTime;
            this.LapCount = lapCount;
            this.Session = session;
            this.Track = track;
            this.Start = start;
        }
    }
}