using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FilterParams
{
    public class Filter<T>
    {
        private List<IExpressionable> filters { get; set; } = new List<IExpressionable>();
        public void AddFilter(PropertyFilter filter)
        {
            filters.Add(filter);
        }
        public void AddFilter(PropertyFilterGroup filterGroup)
        {
            filters.Add(filterGroup);
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
