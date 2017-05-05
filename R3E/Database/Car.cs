using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Database
{
    public class Car
    {
        public Car(int @class, int model)
        {
            this.Class = @class;
            this.Model = model;
        }

        public int Class { get; set; }
        public int Model { get; set; }
    }
}
