using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FilterParams
{
    public class PropertyFilterGroup : IFilterExpression
    {
        public GroupOperators Operator { get; set; }
        public List<IFilterExpression> Children { get; set; } = new List<IFilterExpression>();

        public Expression GetExpression<T>(ParameterExpression parameter)
        {
            if (Children.Count == 0)
            {
                throw new NotImplementedException();
            } else if (Children.Count == 1)
            {
                return Children[0].GetExpression<T>(parameter);
            } else
            {
                var exp = Children[0].GetExpression<T>(parameter);
                var others = Children.ToList();
                others.RemoveAt(0);
                foreach (var child in others)
                {
                    switch (Operator)
                    {
                        case GroupOperators.AND:
                            exp = Expression.AndAlso(exp, child.GetExpression<T>(parameter));
                            break;
                        case GroupOperators.OR:
                            exp = Expression.OrElse(exp, child.GetExpression<T>(parameter));
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    
                }
                return exp;
            }
        }

        public Expression<Func<T, bool>> GetLinqExpression<T>(ParameterExpression parameter)
        {
            return Expression.Lambda<Func<T, bool>>(GetExpression<T>(parameter), parameter);
            
        }
    }
}
