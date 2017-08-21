using Xunit;

namespace FilterParams.Tests
{
    public class QueryParserTests
    {
        [Fact]
        public void Parser_Gives_Error_For_No_Ending_Parenthesis()
        {
            var parser = new QueryParser();
            var result = parser.Parse("(one");
            Assert.True(result.HadError);
            Assert.Equal(ParseErrors.NoEndingParenthesis, result.ErrorReason);
        }

        [Fact]
        public void Parser_Gives_Error_For_No_Starting_Parenthesis()
        {
            var parser = new QueryParser();
            var result = parser.Parse("one)");
            Assert.True(result.HadError);
            Assert.Equal(ParseErrors.NoStartingParenthesis, result.ErrorReason);
        }

        [Fact]
        public void Parser_Does_Not_Error_For_No_Starting_Parenthesis_If_Escaped()
        {
            var parser = new QueryParser();
            var result = parser.Parse(@"one\)");
            Assert.False(result.HadError);
        }

        [Fact]
        public void Parser_Does_Not_Error_For_No_Ending_Parenthesis_If_Escaped()
        {
            var parser = new QueryParser();
            var result = parser.Parse(@"\(one");
            Assert.False(result.HadError);
        }

        [Fact]
        public void Parser_Grabs_Multiple_TopLevel_Segments()
        {
            var parser = new QueryParser();
            var result = parser.Parse(@"(one)two(three)");
            Assert.False(result.HadError);
            Assert.Equal(3, result.Segment.Segments.Count);
            Assert.Equal("one", result.Segment.Segments[0].Value);
            Assert.Equal(GroupOperators.AND, result.Segment.Segments[0].WithNext);
            Assert.Equal("two", result.Segment.Segments[1].Value);
            Assert.Equal(GroupOperators.AND, result.Segment.Segments[1].WithNext);
            Assert.Equal("three", result.Segment.Segments[2].Value);
        }

        [Fact]
        public void Parser_Properly_Evaluates_Ors()
        {
            var parser = new QueryParser();
            var result = parser.Parse(@"(one)&or:two&(three)");
            Assert.False(result.HadError);
            Assert.Equal(3, result.Segment.Segments.Count);
            Assert.Equal("one", result.Segment.Segments[0].Value);
            Assert.Equal(GroupOperators.OR, result.Segment.Segments[0].WithNext);
            Assert.Equal("two", result.Segment.Segments[1].Value);
            Assert.Equal(GroupOperators.AND, result.Segment.Segments[1].WithNext);
            Assert.Equal("three", result.Segment.Segments[2].Value);
        }
        [Fact]
        public void Parser_Properly_Segments_Multiple_Statments()
        {
            var parser = new QueryParser();
            var result = parser.Parse(@"(one)two&three&or:(four)");
            Assert.False(result.HadError);
            Assert.Equal(4, result.Segment.Segments.Count);
            Assert.Equal("one", result.Segment.Segments[0].Value);
            Assert.Equal(GroupOperators.AND, result.Segment.Segments[0].WithNext);
            Assert.Equal("two", result.Segment.Segments[1].Value);
            Assert.Equal(GroupOperators.AND, result.Segment.Segments[1].WithNext);
            Assert.Equal("three", result.Segment.Segments[2].Value);
            Assert.Equal(GroupOperators.OR, result.Segment.Segments[2].WithNext);
            Assert.Equal("four", result.Segment.Segments[3].Value);
        }
        [Fact]
        public void Parser_Grabs_Nested()
        {
            var parser = new QueryParser();
            var result = parser.Parse(@"(one&or:(two)(three))four(five)");
            Assert.False(result.HadError);
            Assert.Equal(3, result.Segment.Segments.Count);
            Assert.Equal(3, result.Segment.Segments[0].Segments.Count);
            Assert.Equal(GroupOperators.OR, result.Segment.Segments[0].Segments[0].WithNext);
        }

        [Fact]
        public void Parser_Converts_Segment_To_Expressionable()
        {
            var parser = new QueryParser();
            var result = parser.Parse(@"a=a&or:b=b&c=c&d=c&or:f=f&or:g=g");
            Assert.False(result.HadError);
            var queryable = parser.ConvertSegment(result.Segment);
        }
        //ConvertSegment
    }
}
