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
        private object databaseLock = new object();
        public R3EDatabase()
        {
            lock (databaseLock)
            {
                var schemaUpdate = new SchemaUpdate(NHibernateHelper.Configuration);
                schemaUpdate.Execute(false, true);
            }
        }

        public void SaveBoxenstopDelta(float delta, String Track, String Layout, int fuel, bool front, bool rear, Car car, float standingTime)
        {
            var bDelta = new BoxenstopDelta(Track, Layout, delta, fuel, front, rear, car, standingTime);
            SaveBoxenstopDelta(bDelta);
        }

        public void SaveBoxenstopDelta(BoxenstopDelta bDelta)
        {
            lock (databaseLock)
            {
                using (ISession session = NHibernateHelper.OpenSession())
                {
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        BoxenstopDelta databaseDelta = GetBDelta(session, bDelta);
                        if (databaseDelta == null)
                        {
                            session.Save(bDelta);
                        }
                        else
                        {
                            // overwrite old delta
                            databaseDelta.Delta = bDelta.Delta;
                            databaseDelta.FrontTires = bDelta.FrontTires;
                            databaseDelta.RearTires = bDelta.RearTires;
                            databaseDelta.Refill = bDelta.Refill;
                            databaseDelta.StandingTime = bDelta.StandingTime;
                            session.Update(databaseDelta);
                        }

                        transaction.Commit();
                    }
                }
            }
        }

        private ICriteria BoxenstopCriteria(ISession session, string track, string layout, Car car)
        {
            return session.CreateCriteria(typeof(BoxenstopDelta)).Add(Restrictions.Eq("Track", track) & Restrictions.Eq("Layout", layout) & Restrictions.Eq("CarClass", car.Class) & Restrictions.Eq("CarModel", car.Model));
        }

        private BoxenstopDelta GetBDelta(ISession session, BoxenstopDelta bDelta)
        {
            var fromDb = BoxenstopCriteria(session, bDelta.Track, bDelta.Layout, bDelta.Car).List<BoxenstopDelta>();

            if (fromDb.Count > 0)
            {
                // take the newest entry
                return fromDb[fromDb.Count - 1];
            }

            return null;
        }

        internal void UpdateTrack(TrackLimits track)
        {
            lock (databaseLock)
            {
                using (ISession session = NHibernateHelper.OpenSession())
                {
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        TrackLimits trackFromDB = getTrack(session, track.Name, track.Layout);
                        if (trackFromDB == null)
                        {
                            session.Save(track);
                        }
                        else
                        {
                            // overwrite old delta
                            trackFromDB.FirstSector = track.FirstSector;
                            trackFromDB.SecondSector = track.SecondSector;
                            trackFromDB.FirstSectorError = track.FirstSectorError;
                            trackFromDB.SecondSectorError = track.SecondSectorError;
                            session.Update(trackFromDB);
                        }

                        transaction.Commit();
                    }
                }
            }
        }

        public float GetBoxenstopDelta(string track, string layout, Car car)
        {
            lock (databaseLock)
            {
                using (ISession session = NHibernateHelper.OpenSession())
                {
                    var fromDb = BoxenstopCriteria(session, track, layout, car).List<BoxenstopDelta>();

                    if (fromDb.Count > 0)
                    {
                        // take the newest entry
                        return fromDb[fromDb.Count - 1].Delta;
                    }
                }

                return DisplayData.INVALID_POSITIVE;
            }
        }

        internal List<TrackLimits> GetAllTracks()
        {
            lock (databaseLock)
            {
                using (ISession session = NHibernateHelper.OpenSession())
                {
                    return session.QueryOver<TrackLimits>().List<TrackLimits>().ToList<TrackLimits>();
                }
            }
        }

        public TrackLimits GetTrack(string track, string layout)
        {
            lock (databaseLock)
            {
                using (ISession session = NHibernateHelper.OpenSession())
                {
                    return getTrack(session, track, layout);
                }
            }
        }

        private TrackLimits getTrack(ISession session, string track, string layout)
        {
            return session.CreateCriteria(typeof(TrackLimits)).Add(Restrictions.Eq("Name", track) & Restrictions.Eq("Layout", layout)).UniqueResult<TrackLimits>();
        }
    }
}
