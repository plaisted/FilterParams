using System;
using System.Collections.Generic;
using System.Text;

namespace FilterParams
{
    public static class ParserTools
    {
        public static string StripSurroundingOps(string value)
        {
            if (value.StartsWith(Or))
            {
                value = value.Substring(Or.Length);
            }
            if (value.StartsWith(And))
            {
                value = value.Substring(And.Length);
            }
            if (value.EndsWith(Or))
            {
                value = value.Substring(0, value.Length - Or.Length);
            }
            if (value.EndsWith(And))
            {
                value = value.Substring(0, value.Length - And.Length);
            }
            return value;
        }
        public static bool NotEscaped(string segment, int character)
        {
            if (character == 0)
            {
                return true;
            }
            int count = 0;
            for (int i = character - 1; i > -1; i--)
            {
                if (segment[i] == '\\')
                {
                    count += 1;
                }
                else
                {
                    break;
                }
            }
            return count % 2 == 0;
        }
        public static bool ContainsUnescaped(string segment, string value)
        {
            var loc = segment.IndexOf(value);
            return loc != -1 && NotEscaped(segment, loc);
        }
        public static List<string> SplitOnUnescaped(string data, string[] values)
        {
            List<string> vals = new List<string>(values);
            vals.Sort((a, b) => -1 * a.CompareTo(b));
            List<string> splits = new List<string>();
            while (!string.IsNullOrEmpty(data))
            {
                int first = -1;
                int foundLength = -1;
                foreach (var val in vals)
                {
                    var loc = data.IndexOf(val);
                    if (loc == -1)
                    {
                        continue;
                    }
                    if (ParserTools.NotEscaped(data, loc) && (loc < first || first == -1))
                    {
                        first = loc;
                        foundLength = val.Length;
                    }
                }
                if (first == -1)
                {
                    splits.Add(data);
                    break;
                }
                if (first != 0)
                {
                    splits.Add(data.Substring(0, first));
                    splits.Add(data.Substring(first, foundLength));
                }
                data = data.Substring(first + foundLength);

            }
            return splits;
        }
        public const string Or = "&or:";
        public const string And = "&";
    }
}
