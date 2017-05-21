using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Database
{
    public class TrackLimits
    {
        public virtual Guid Id { get; set; }
        public virtual String Name { get; set; }
        public virtual String Layout { get; set; }
        public virtual float FirstSector { get; set; }
        public virtual float SecondSector { get; set; }

        public virtual float FirstSectorError { get; set; }
        public virtual float SecondSectorError { get; set; }


        public TrackLimits()
        {

        }
        public TrackLimits(string name, string layout, float firstSector, float secondSector, float firstError, float secondError)
        {
            this.Name = name;
            this.Layout = layout;
            this.FirstSector = firstSector;
            this.SecondSector = secondSector;
            this.FirstSectorError = firstError;
            this.SecondSectorError = secondError;
        }

        
    }
}
