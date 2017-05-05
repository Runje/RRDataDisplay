using R3E.Database;

namespace R3E.Model
{
    public class SessionInfo
    {
        public string Track { get; set; }
        public string Layout { get; set; }
        public int Session { get; set; }
        public Car Car { get; set; }

        public SessionInfo(string track, string layout, int session, Car car)
        {
            this.Track = track;
            this.Layout = layout;
            this.Session = session;
            this.Car = car;
        }

        
    }
}