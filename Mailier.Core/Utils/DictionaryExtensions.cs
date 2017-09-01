using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mailier.Core.Utils
{
    public static class DictionaryExtensions
    {
        public static string TryGetString(this Dictionary<string, string> dictionary, string key) {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (!dictionary.ContainsKey(key)) return string.Empty;
            return dictionary[key];
        }
    }
}