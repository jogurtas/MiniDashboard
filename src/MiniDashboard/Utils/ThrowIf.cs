using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniDashboard.Utils
{
    internal static class ThrowIf
    {
        public static void IsNull(object value, string valueName)
        {
            _ = value ?? throw new ArgumentNullException(valueName);
        }

        public static void IsNullOrEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new Exception("String can not be null or empty");
        }
        
        public static void IsNullOrEmpty<T>(IEnumerable<T> collection)
        {
            if (collection?.Count() == 0)
                throw new Exception("Collection can not be null or empty");
        }

        public static void IsNullOrWhiteSpace(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception("String can not be null or whitespace");
        }

        public static void IsTrue(bool value, string message)
        {
            if (value) throw new Exception(message);
        }

        public static void IsFalse(bool value, string message)
        {
            if (!value) throw new Exception(message);
        }
    }
}