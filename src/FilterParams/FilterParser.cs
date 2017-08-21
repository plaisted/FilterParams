using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FilterParams
{
    public class FilterParser
    {
        public FilterParser()
        {

        }

        public IExpressionable Parse(string input)
        {
            var segments = SplitOnUnescaped(input, new string[] { "&", "&or:", "&OR:" });

            var group = new PropertyFilterGroup();
            if (segments.Any(x=>x == "&or:" || x == "&OR:"))
            {
                while (segments.Any())
                {
                    if (segments.First()[0] == '&')
                    {
                        segments.RemoveAt(0);
                        continue;
                    }
                    if (segments.Count == 1 || segments.Count > 1 && (segments[1] == "&OR:" || segments[1] == "&or:"))
                    {
                        group.Children.Add(ParseStatement(segments[0]));
                        segments.RemoveAt(0);
                        continue;
                    }
                    //next is not OR, find next OR
                    var lower = segments.IndexOf("&or:");
                    var upper = segments.IndexOf("&OR:");
                    if (lower == -1 && upper == -1)
                    {
                        //all ands
                        group.Children.Add(CreateAndGroup(segments));
                        segments.Clear();
                        break;
                    }
                    var next = Math.Min(lower, upper);
                    group.Children.Add(CreateAndGroup(segments.Take(next)));
                    segments.RemoveRange(0, next);
                }
 
                
            }
           
            return group;
        }

        private IExpressionable CreateAndGroup(IEnumerable<string> list)
        {
            var group = new PropertyFilterGroup();
            group.Operator = GroupOperators.AND;
            foreach (var item in list)
            {
                if (item[0] == '&')
                {
                    continue;
                } else
                {
                    group.Children.Add(ParseStatement(item));
                }
            }
            return group;
        }

        private PropertyFilter ParseStatement(string statement)
        {
            var equal = statement.IndexOf('=');
            if (equal == -1)
            {
                throw new Exception("Statement did not contain an equals.");
            }
            var filter = new PropertyFilter();
            filter.PropertyName = statement.Substring(0, equal);
            statement = statement.Substring(equal+1);
            var operatorSep = statement.IndexOf(':');
            if (operatorSep > -1)
            {
                filter.Operator = ParseOperators.FromString(statement.Substring(0, operatorSep));
                statement = statement.Substring(operatorSep+1);
            } else
            {
                filter.Operator = Operators.Equal;
            }
            filter.Value = statement;
            return filter;
            
        }

        private List<string> SplitOnUnescaped(string data, string[] values)
        {
            List<string> vals = new List<string>(values);
            vals.Sort((a, b) => -1 * a.CompareTo(b));
            List<string> splits = new List<string>();
            while (true)
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
                    if (NotEscaped(data, loc) && (loc < first || first == -1))
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

        private bool NotEscaped(string segment, int charLocation)
        {
            if (charLocation == 0)
            {
                return true;
            }
            int count = 0;
            for (int i = charLocation - 1; i > -1; i--)
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
    }
}
