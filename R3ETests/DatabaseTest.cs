using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using R3E.Database;
using NHibernate.Tool.hbm2ddl;
using R3E.Model;

namespace R3ETests
{
    [TestClass]
    public class DatabaseTest
    {
        private R3EDatabase Database;

        [TestInitialize]
        public void init()
        {
            var schemaUpdate = new SchemaUpdate(NHibernateHelper.TestConfiguration);
            schemaUpdate.Execute(false, true);
            Database = new R3EDatabase();
        }

        [TestCleanup]
        public void cleanUp()
        {
            new SchemaExport(NHibernateHelper.TestConfiguration).Drop(false, true);
        }

        [TestMethod]
        public void SaveBoxenstopDelta()
        {
            Database.SaveBoxenstopDelta(27.3f, "Monza", "Grand Prix", 1, true, true, 1);
            float delta = Database.GetBoxenstopDelta("Monza", "Grand Prix", 1);

            Assert.AreEqual(27.3f, delta);

            delta = Database.GetBoxenstopDelta("Suzuka", "Grand Prix", 1);
            Assert.AreEqual(DisplayData.INVALID, delta);
        }
    }
}
