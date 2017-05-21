using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Database
{
    public class TrackMapper : ClassMapping<TrackLimits>
    {
        public TrackMapper()
        {
            Id(x => x.Id, m => m.Generator(Generators.GuidComb));
            Property(x => x.Name);
            Property(x => x.Layout);
            Property(x => x.FirstSector);
            Property(x => x.SecondSector);
            Property(x => x.FirstSectorError);
            Property(x => x.SecondSectorError);
        }
    }
}
