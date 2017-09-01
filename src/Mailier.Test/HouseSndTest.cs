using Mailier.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailier.Test
{
    [TestClass]
    public class HouseSndTest
    {
        [TestMethod]
        public void TestCreateAll() {
            new HouseSndService().CreateAll();
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestQuery() {
            var dictionary = new Dictionary<string, string>
            {
                { "BrokerID", "53455" },
                { "GardenID", "1AE7343322F39630E053660310AC41DE,1AE7343322F69630E053660310AC41DE" }
            };

            var result = new HouseSndService().Query(dictionary, "inthouseno", "brokerId", "gardenId");
            Assert.IsTrue(result.Any());
        }
    }
}
