namespace R3E.Model
{
    public class SessionInfo
    {
        public string Track { get; set; }
        public string Layout { get; set; }
        public int Session { get; set; }
        public int CarId { get; set; }

        public SessionInfo(string track, string layout, int session, int carId)
        {
            this.Track = track;
            this.Layout = layout;
            this.Session = session;
            this.CarId = carId;
        }

        
    }
}