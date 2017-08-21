using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FilterParams.Tests
{
    public class FilterParserTests
    {
        [Fact]
        public void It_Parses()
        {
            var parser = new FilterParser();
            var results = parser.Parse("a=b&or:prop1=value&prop2=gte:5");

        }
    }
}
