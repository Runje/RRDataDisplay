using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Database
{
    public class Track
    {
        public virtual Guid Id { get; set; }
        public virtual String Name { get; set; }
        public virtual String Layout { get; set; }
        public virtual float FirstSector { get; set; }
        public virtual float SecondSector { get; set; }

        public Track()
        {

        }
        public Track(string name, string layout, float firstSector, float secondSector)
        {
            this.Name = name;
            this.Layout = layout;
            this.FirstSector = firstSector;
            this.SecondSector = secondSector;
        }

        
    }
}
