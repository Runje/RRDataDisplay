using R3E.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Database
{
    public class TB
    {
        public virtual Guid Id { get; set; }
        public virtual String Track { get; set; }
        public virtual String Layout { get; set; }
        public virtual Lap Lap { get; set; }
        public virtual int Class { get; set; }
        public virtual int CarId { get; set; }
        public virtual Tires TireWear { get; set; }
        public virtual float Fuel { get; set; }

    }
}
