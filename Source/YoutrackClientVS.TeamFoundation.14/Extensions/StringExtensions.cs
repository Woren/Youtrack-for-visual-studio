﻿using System;
using System.Security;

namespace YouTrackClientVS.TeamFoundation.Extensions
{
    public static class StringExtensions
    {
        public static SecureString ToSecureString(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;
            else
            {
                SecureString result = new SecureString();
                foreach (char c in source)
                    result.AppendChar(c);
                return result;
            }
        }
        public static string TrimEnd(this string s, string suffix)
        {
            if (s == null) return null;
            if (!s.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return s;

            return s.Substring(0, s.Length - suffix.Length);
        }

        public static bool Contains(this string str,string subStr, StringComparison comparisonType)
        {
            return str.IndexOf(subStr, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}