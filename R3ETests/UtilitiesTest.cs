using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using R3E;

namespace R3ETests
{
    [TestClass]
    public class UtilitiesTest
    {
        [TestMethod]
        public void StringBytes()
        {
            const string text = "Monza Circuit";
            Assert.AreEqual(text, R3E.Utilities.byteToString(R3E.Utilities.stringToBytes(text)));
        }
    }
}
