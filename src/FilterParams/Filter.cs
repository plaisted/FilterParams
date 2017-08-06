using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace FilterParams
{
    public class Filter<T>
    {
        private List<PropertyFilter> filters { get; set; } = new List<PropertyFilter>();
        public void AddFilter(PropertyFilter filter)
        {
            filters.Add(filter);
        }
        public IQueryable<T> Apply(IQueryable<T> input)
        {
            var output = input;
            foreach (var filter in filters)
            {
                var prop = typeof(T).GetProperty(filter.PropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                var convertor = TypeDescriptor.GetConverter(prop.PropertyType);
                var value = convertor.ConvertFromInvariantString(filter.Value);
                switch (filter.Operator)
                {
                    case Operators.Equal:
                        input = input.Where(x => prop.GetValue(x).Equals(value));
                        break;
                    case Operators.Not:
                        input = input.Where(x => !prop.GetValue(x).Equals(value));
                        break;
                    case Operators.Like:
                        if (prop.PropertyType != typeof(string))
                        {
                            throw new Exception("move this error to modelstate handler");
                        }
                        input = input.Where(x => ((string)prop.GetValue(x)).Contains((string)value));
                        break;
                    case Operators.GreaterThan:
                        if (!prop.PropertyType.GetInterfaces().Contains(typeof(IComparable)))
                        {
                            throw new Exception("move this error to modelstate handler");
                        }
                        input = input.Where(x => ((IComparable)prop.GetValue(x)).CompareTo(value) > 0);
                        break;
                    case Operators.GreaterThanEqual:
                        if (!prop.PropertyType.GetInterfaces().Contains(typeof(IComparable)))
                        {
                            throw new Exception("move this error to modelstate handler");
                        }
                        input = input.Where(x => ((IComparable)prop.GetValue(x)).CompareTo(value) >= 0);
                        break;
                    case Operators.LessThan:
                        if (!prop.PropertyType.GetInterfaces().Contains(typeof(IComparable)))
                        {
                            throw new Exception("move this error to modelstate handler");
                        }
                        input = input.Where(x => ((IComparable)prop.GetValue(x)).CompareTo(value) < 0);
                        break;
                    case Operators.LessThanEqual:
                        if (!prop.PropertyType.GetInterfaces().Contains(typeof(IComparable)))
                        {
                            throw new Exception("move this error to modelstate handler");
                        }
                        input = input.Where(x => ((IComparable)prop.GetValue(x)).CompareTo(value) <= 0);
                        break;
                }
            }
            return input;
        }
    }
}
