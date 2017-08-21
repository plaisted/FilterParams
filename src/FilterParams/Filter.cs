using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FilterParams
{
    public class Filter<T> : Filter<T, T>
    {

    }

    public class Filter<IT, T>
    {
        private List<IFilterExpression> filters { get; set; } = new List<IFilterExpression>();
        public void AddFilter(PropertyFilter filter)
        {
            filters.Add(filter);
        }
        public void AddFilter(PropertyFilterGroup filterGroup)
        {
            filters.Add(filterGroup);
        }
        public void AddFilter(IFilterExpression filter)
        {
            filters.Add(filter);
        }
        public IQueryable<T> Apply(IQueryable<T> input)
        {
            foreach (var filter in filters)
            {
                input = input.Where(filter.GetLinqExpression<T>(Expression.Parameter(typeof(T))));
            }
            return input;
        }
    }
}
