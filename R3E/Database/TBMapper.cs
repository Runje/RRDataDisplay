using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Database
{
    public class TBMapper : ClassMapping<TB>
    {
        public TBMapper()
        {
            Id(x => x.Id, m => m.Generator(Generators.GuidComb));
            Property(x => x.CarId);
            Property(x => x.Class);
            //Property(x => x.Lap.Sector1);
            //Property(x => x.Lap.RelSector2);
            //Property(x => x.Lap.RelSector3);
            Property(x => x.Layout);
            Property(x => x.Track);
            //Property(x => x.TireWear);
            Property(x => x.Fuel);

        }
    }
}
