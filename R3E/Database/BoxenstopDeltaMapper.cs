using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Database
{
    public class BoxenstopDeltaMapper : ClassMapping<BoxenstopDelta>
    {
        public BoxenstopDeltaMapper()
        {
            Id(x => x.Id, m => m.Generator(Generators.GuidComb));
            Property(x => x.Delta);
            Property(x => x.StandingTime);
            Property(x => x.FrontTires);
            Property(x => x.RearTires);
            Property(x => x.Layout);
            Property(x => x.Track);
            Property(x => x.Refill);
            Property(x => x.CarClass);
            Property(x => x.CarModel);
        }
    }
}
