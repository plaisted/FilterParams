using System;
using System.Linq.Expressions;

namespace FilterParams
{
    public interface IExpressionable
    {
        Expression<Func<T, bool>> GetLinqExpression<T>(ParameterExpression parameter);
        Expression GetExpression<T>(ParameterExpression parameter);
    }
}
