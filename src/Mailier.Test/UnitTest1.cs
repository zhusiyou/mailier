using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mailier.Core.Services;
using System.Text.RegularExpressions;
using StackExchange.Redis;

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

        [TestMethod]
        public void TestReplace() {
            var url = "http://app.xx.com:8081/member/123";
            var str = Regex.Replace(url, ":\\d+", "");
            //var str = url.Replace("(\\:\\d+)", "");
            Assert.IsTrue(str.Equals("http://app.xx.com/member/123"));
        }

        [TestMethod]
        public void TestRedisKey() {
            RedisKey key = "1";
            RedisKey anotherKey = key.Append("2");
            var t = anotherKey.Prepend("3");
            Assert.IsTrue(anotherKey == "12");
            Assert.IsTrue(t == "312");
        }
    }
}
