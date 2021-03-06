using System;
using System.Collections.Generic;
using System.Linq;
using FilterParams;
using Xunit;

namespace FilterParams.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void TestMethod1()
        {
            RunTest<Int16>(new List<short> { 1, 2, 3, 4 }, Operators.Equal, "1", new List<short> { 1 });
            RunTest<Int16>(new List<short> { 1, 2, 3, 4 }, Operators.Not, "1", new List<short> { 2, 3, 4 });
            RunTest<Int16>(new List<short> { 1, 2, 3, 4 }, Operators.GreaterThan, "2", new List<short> { 3, 4 });
            RunTest<Int16>(new List<short> { 1, 2, 3, 4 }, Operators.GreaterThanEqual, "2", new List<short> { 2, 3, 4 });
            RunTest<Int16>(new List<short> { 1, 2, 3, 4 }, Operators.LessThan, "3", new List<short> { 1, 2 });
            RunTest<Int16>(new List<short> { 1, 2, 3, 4 }, Operators.LessThanEqual, "3", new List<short> { 1, 2, 3 });

            RunGroupTest<Int16>(new List<short> { 1, 2, 3, 4 }, GroupOperators.AND,
                Operators.LessThan, "3",
                Operators.GreaterThan, "1",
                new List<short> { 2 });

            RunGroupTest<Int16>(new List<short> { 1, 2, 3, 4 }, GroupOperators.OR,
                Operators.Equal, "3",
                Operators.Equal, "4",
                new List<short> { 3, 4 });
        }

        [Fact]
        public void TestMethodString()
        {
            RunTest<string>(new List<string> { "a", "b", "c" }, Operators.Equal, "a", new List<string> { "a" });
            RunTest<string>(new List<string> { "a", "b", "c" }, Operators.Not, "a", new List<string> { "b", "c" });
            RunTest<string>(new List<string> { "atesta", "b", "c" }, Operators.Like, "test", new List<string> { "atesta" });
        }

        public void RunTest<T>(List<T> seedValues, Operators op, string value, List<T> expectedResults)
        {
            List<TestContainer<T>> list = new List<TestContainer<T>>();
            foreach (var val in seedValues)
            {
                list.Add(new TestContainer<T> { Value = val });
            }

            var filterProvider = new Filter<ITestContainer<T>>();
            filterProvider.AddFilter(new PropertyFilter { Operator = op, PropertyName = "Value", Value = value });
            var result = filterProvider.Apply(list.AsQueryable());
            var values = result.Select(x => x.Value).ToList();

            Assert.True(Enumerable.SequenceEqual(values.OrderBy(x => x), expectedResults.OrderBy(x => x)));
        }

        public void RunGroupTest<T>(List<T> seedValues, GroupOperators op, Operators opFirst, string valueFirst,
            Operators opSecond, string valueSecond, List<T> expectedResults)
        {
            List<TestContainer<T>> list = new List<TestContainer<T>>();
            foreach (var val in seedValues)
            {
                list.Add(new TestContainer<T> { Value = val });
            }

            var filterProvider = new Filter<TestContainer<T>>();
            var group = new PropertyFilterGroup();
            group.Operator = op;
            group.Children.Add(new PropertyFilter { Operator = opFirst, PropertyName = "Value", Value = valueFirst });
            group.Children.Add(new PropertyFilter { Operator = opSecond, PropertyName = "Value", Value = valueSecond });
            filterProvider.AddFilter(group);
            var result = filterProvider.Apply(list.AsQueryable());
            var values = result.Select(x => x.Value).ToList();

            Assert.True(Enumerable.SequenceEqual(values.OrderBy(x => x), expectedResults.OrderBy(x => x)));
        }
    }
    public class TestContainer<T> : ITestContainer<T>
    {
        public T Value { get; set; }
        public SByte SByte { get; set; }
        public Byte Byte { get; set; }
        public Int16 Int16 { get; set; }
        public Int32 Int32 { get; set; }
        public Int64 Int64 { get; set; }
        public Single Single { get; set; }
        public Double Double { get; set; }
        public Decimal Decimal { get; set; }
        public DateTime DateTime { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public string String { get; set; }
    }

    public interface ITestContainer<T>
    {
        T Value { get; set; }
    }

}
