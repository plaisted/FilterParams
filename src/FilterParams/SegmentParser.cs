using System;
using System.Collections.Generic;
using System.Text;

namespace FilterParams
{
    public class SegmentParser
    {
        //"(test=gt:5&or:test3=lt:1)&test2=lt:10" // (\\\)|[^)])*\)
        public SegmentParser()
        {
            
        }
        public SegmentParseResult Parse(string query)
        {
            return GetSegment(query);
        }

        private SegmentParseResult GetSegment(string segment)
        {
            var mySegs = new List<Segment>();
            int occurences = 0;
            int start = -1;
            int end = -1;
            int lastEndMatch = -1;
            for (int i=0; i<segment.Length; i++)
            {
                if (segment[i] == '(' && ParserTools.NotEscaped(segment, i))
                {
                    occurences += 1;
                    if (start == -1)
                    {
                        start = i;
                        if (start > 0 && start - lastEndMatch > 1)
                        {
                            var text = segment.Substring(lastEndMatch + 1, start - lastEndMatch - 1);
                            mySegs.AddRange(GetSegmentsFromString(text));
                        }
                    }
                    continue;
                }
                if (segment[i] == ')' && ParserTools.NotEscaped(segment, i))
                {
                    occurences -= 1;
                    if (occurences == 0)
                    {
                        end = i;
                        lastEndMatch = i;
                        var seg = GetSegment(segment.Substring(start + 1, end - start - 1));
                        if (seg.HadError)
                        {
                            return seg;
                        }
                        else
                        {
                            if (segment.Length > i+4 && segment.Substring(i+1, 4) == "&or:")
                            {
                                seg.Segment.WithNext = GroupOperators.OR;
                            } else
                            {
                                seg.Segment.WithNext = GroupOperators.AND;
                            }
                            mySegs.Add(seg.Segment);
                        }
                        start = -1;
                        end = -1;
                    }
                }
            }

            //uneven
            if (occurences > 0)
            {
                return new SegmentParseResult { HadError = true, ErrorReason = ParseErrors.NoEndingParenthesis };
            } else if (occurences < 0)
            {
                return new SegmentParseResult { HadError = true, ErrorReason = ParseErrors.NoStartingParenthesis };
            }

            //none
            if (start == -1 && end == -1 && mySegs.Count == 0)
            {
                return new SegmentParseResult { HadError = false, Segment = new Segment { HasSegments = false, Value = segment } };
            }

            //remainder
            if (start == -1 && end == -1 && mySegs.Count > 0 && lastEndMatch < segment.Length -1)
            {
                mySegs.Add(new Segment { HasSegments = false, Value = segment.Substring(end + 1) });
            }

            return new SegmentParseResult { Segment = new Segment { HasSegments = true, Segments = mySegs }, HadError = false};
        }

        private List<Segment> GetSegmentsFromString(string text)
        {
            var segs = new List<Segment>();
            var textSegments = ParserTools.SplitOnUnescaped(text, new string[] { ParserTools.And, ParserTools.Or });
            for (int s = 0; s < textSegments.Count; s++)
            {
                if (textSegments[s] == ParserTools.And || textSegments[s] == ParserTools.Or)
                {
                    continue;
                }
                var seg = new Segment(textSegments[s]);
                if (textSegments.Count - 1 > s)
                {
                    if (textSegments[s + 1] == ParserTools.Or)
                    {
                        seg.WithNext = GroupOperators.OR;
                    }
                    else
                    {
                        seg.WithNext = GroupOperators.AND;
                    }
                }
                else
                {
                    seg.WithNext = GroupOperators.AND;
                }
                segs.Add(seg);
            }
            return segs;
        }
        
    }
    public class Segment
    {
        public Segment()
        {

        }
        public Segment(string value)
        {
            Value = value;
            HasSegments = false;
        }
        public string Value { get; set; }
        public GroupOperators WithNext { get; set; }
        public bool HasSegments { get; set; }
        public List<Segment> Segments { get; set; }

    }
    public class SegmentParseResult
    {
        public bool HadError { get; set; }
        public string ErrorReason { get; set; }
        public Segment Segment { get; set; }
    }
    public static class ParseErrors
    {
        public const string NoEndingParenthesis = "Starting paranthesis without ending.";
        public const string NoStartingParenthesis = "Ending paranthesis without starting.";
    }

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
