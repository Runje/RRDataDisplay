using NHibernate;
using NHibernate.Criterion;
using NHibernate.Tool.hbm2ddl;
using R3E.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Database
{
    public class R3EDatabase
    {
        public R3EDatabase()
        {
            var schemaUpdate = new SchemaUpdate(NHibernateHelper.Configuration);
            schemaUpdate.Execute(false, true);
        }

        public void SaveBoxenstopDelta(float delta, String Track, String Layout, int fuel, bool front, bool rear, int carId)
        {
            var bDelta = new BoxenstopDelta(Track, Layout, delta, fuel, front, rear, carId);
            SaveBoxenstopDelta(bDelta);
        }

        public void SaveBoxenstopDelta(BoxenstopDelta bDelta)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(bDelta);
                transaction.Commit();
            }
        }

        public float GetBoxenstopDelta(string track, string layout, int carId)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var fromDb = session.CreateCriteria(typeof(BoxenstopDelta)).Add(Restrictions.Eq("Track", track) & Restrictions.Eq("Layout", layout) & Restrictions.Eq("CarId", carId))
                    .List<BoxenstopDelta>();
                    
                if (fromDb.Count > 0)
                {
                    return fromDb[0].Delta;
                }
            }

            return DisplayData.INVALID_POSITIVE;
        }
    }
}
