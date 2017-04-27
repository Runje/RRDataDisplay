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
        public String Track { get; set; }
        public String Layout { get; set; }
        public Lap Lap { get; set; }
        public int Class { get; set; }
        public int CarId { get; set; }
    }
}
