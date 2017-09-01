using Mailier.Core.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mailier.Test
{
    [TestClass]
    public class RedisTest
    {
        [TestMethod]
        public void TestString() {
            var helper = new RedisHelper();
            const string key = "mailier";
            const string value = "test";

            helper.StringSet(key, value);
            var val = helper.StringGet(key);
            
            Assert.IsTrue(val.Equals(value));

            helper.KeyDelete(key);
            Assert.IsFalse(helper.KeyExists(key));
        }
    }
}
