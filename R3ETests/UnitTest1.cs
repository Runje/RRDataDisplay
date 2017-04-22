using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using R3E;

namespace R3ETests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void StringBytes()
        {
            const string text = "Monza Circuit";
            Assert.AreEqual(text, Utilities.byteToString(Utilities.stringToBytes(text)));
        }
    }
}
