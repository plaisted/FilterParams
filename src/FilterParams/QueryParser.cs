using System;
using System.Collections.Generic;
using System.Text;

namespace FilterParams
{
    public class QueryParser
    {
        public QueryParser(string query)
        {
            while (true)
            {
                var result = GrabOuterMost(query);
                if (result.Item2 == "")
                {
                    HandleSegment(result.Item1);
                    break;
                }
            }
            
            
        }
        private void HandleSegment(string segment)
        {

        }

        private Tuple<string, string> GrabOuterMost(string segment)
        {
            var first = segment.IndexOf("?(");
            if (first == -1)
            {
                first = segment.IndexOf("&(");
            }
            if (first == -1)
            {
                return new Tuple<string, string>(segment, "");
            }
            else
            {
                var last = segment.LastIndexOf(")&");
                if (last == -1)
                {
                    if (segment[segment.Length] == ')')
                    {
                        last = segment.Length;
                    } else
                    {
                        throw new Exception("Starting parenthesis but no ending.");
                    }
                }
                return new Tuple<string, string>(segment.Substring(first, last - first),
                    segment.Substring(0, first) + segment.Substring(last, segment.Length));

            }
        }
    }
}
