using System;
using System.Linq.Expressions;

namespace FilterParams
{
    public interface IFilterExpression
    {
        Expression<Func<T, bool>> GetLinqExpression<T>(ParameterExpression parameter);
        Expression GetExpression<T>(ParameterExpression parameter);
    }
}
