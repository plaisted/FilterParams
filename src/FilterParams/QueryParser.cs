using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterParams
{
    public class QueryParser
    {
        public QueryParser()
        {
            
        }
        public SegmentParseResult Parse(string query)
        {
            return GetSegment(query);
        }

        public IFilterExpression ConvertSegment(Segment segment)
        {
            if (!segment.HasSegments)
            {
                return new PropertyFilter(segment.Value);
            }
            if (segment.Segments.Any(x=>x.WithNext == GroupOperators.OR))
            {
                var group = new PropertyFilterGroup();
                group.Operator = GroupOperators.OR;

                var andGroup = new PropertyFilterGroup();
                GroupOperators lastOp = GroupOperators.None;
                var segments = segment.Segments.ToList();
                for (int i = segments.Count-1; i > -1; i--)
                {
                    if (i > 0 && segments[i - 1].WithNext == GroupOperators.AND)
                    {
                        andGroup.Children.Insert(0,ConvertSegment(segments[i]));
                        segments.RemoveAt(i);
                        lastOp = GroupOperators.AND;
                    }
                    else if (i > 0 && segments[i - 1].WithNext == GroupOperators.OR)
                    {
                        if (lastOp == GroupOperators.AND)
                        {
                            andGroup.Children.Insert(0,ConvertSegment(segments[i]));
                            group.Children.Insert(0, andGroup);
                            andGroup = new PropertyFilterGroup();
                        }
                        else
                        {
                            group.Children.Insert(0, ConvertSegment(segments[i]));
                        }
                        segments.RemoveAt(i);
                        lastOp = GroupOperators.OR;
                    }
                    else
                    {
                        if (lastOp == GroupOperators.OR)
                        {
                            group.Children.Insert(0, ConvertSegment(segments[i]));
                        } else
                        {
                            andGroup.Children.Insert(0, ConvertSegment(segments[i]));
                            group.Children.Insert(0, andGroup);
                        }
                    }
                }
                return group;
            } else
            {
                var group = new PropertyFilterGroup();
                group.Operator = GroupOperators.AND;
                group.Children.AddRange(segment.Segments.Select(x => ConvertSegment(x)));
                return group;
            }
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
                            var newSegs = GetSegmentsFromString(text);
                            var last = newSegs.Last();
                            if (last.WithNext == GroupOperators.None)
                            {
                                last.WithNext = GetNextOperator(segment, i);
                            }
                            mySegs.AddRange(newSegs);
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
                            seg.Segment.WithNext = GetNextOperator(segment, i);
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
                var results = GetSegmentsFromString(segment);
                var result = new Segment();
                if (results.Count > 1)
                {
                    result.HasSegments = true;
                    result.Segments = results;
                } else
                {
                    result = results.First();
                }
                return new SegmentParseResult { HadError = false, Segment = result };
            }

            //remainder
            if (start == -1 && end == -1 && mySegs.Count > 0 && lastEndMatch < segment.Length -1)
            {
                mySegs.Add(new Segment { HasSegments = false, Value = segment.Substring(end + 1) });
            }

            return new SegmentParseResult { Segment = new Segment { HasSegments = true, Segments = mySegs }, HadError = false};
        }
        private GroupOperators GetNextOperator(string text, int location)
        {
            if (text.Length > location + 4 && text.Substring(location + 1, 4) == ParserTools.Or)
            {
                return GroupOperators.OR;
            }
            else if (text.Length > location + 1)
            {
                return GroupOperators.AND;
            }
            else
            {
                return GroupOperators.None;
            }
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
                    seg.WithNext = GroupOperators.None;
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
}
