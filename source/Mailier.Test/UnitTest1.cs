using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mailier.Core.Services;

namespace Mailier.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var str = "14号线-阜通-3|14号线-望京-3|14号线-望京南-3|15号线-望京-3";
            var array = str.Split(new[] { '|', '-' });
            Assert.IsTrue(array.Length > 0);
        }
    }
}
