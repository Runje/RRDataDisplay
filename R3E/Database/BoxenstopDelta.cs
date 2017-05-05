using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Database
{
    public class BoxenstopDelta
    {
        public virtual Guid Id { get; set; }
        public virtual String Track { get; set; }
        public virtual String Layout { get; set; }
        public virtual float Delta { get; set; }
        public virtual int Refill { get; set; }
        public virtual bool FrontTires { get; set; }
        public virtual bool RearTires { get; set; }
        public virtual Car Car { get; set; }
        public virtual int CarClass { get { return Car.Class; } set { Car.Class = value; } }
        public virtual int CarModel { get { return Car.Model; } set { Car.Model = value; } }
        

        public BoxenstopDelta()
        {
            Car = new Database.Car(-1, -1);
        }

        public BoxenstopDelta(string track, string layout, float delta, int refill, bool frontTires, bool rearTires, Car car)
        {
            this.Track = track;
            this.Layout = layout;
            this.Delta = delta;
            this.Refill = refill;
            this.FrontTires = frontTires;
            this.RearTires = rearTires;
            this.Car = car;
        }

    }
}
