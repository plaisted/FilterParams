using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FilterParams
{
    public class PropertyFilter : IExpressionable
    {
        public PropertyFilter()
        {

        }
        public PropertyFilter(string contents)
        {

        }
        public Operators Operator { get; set; }
        public string PropertyName { get; set; }
        private PropertyInfo PropertyInfo { get; set; }
        public string Value { get; set; }
        public Expression<Func<T, bool>> GetLinqExpression<T>(ParameterExpression parameter)
        {
            var prop = typeof(T).GetProperty(PropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var property = Expression.Property(parameter, prop);

            var convertor = TypeDescriptor.GetConverter(prop.PropertyType);
            var value = convertor.ConvertFromInvariantString(Value);
            var expVal = Expression.Constant(value);

            return Expression.Lambda<Func<T, bool>>(GetExpression<T>(property, expVal, prop), parameter);
        }
        public Expression GetExpression<T>(ParameterExpression parameter)
        {
            var prop = typeof(T).GetProperty(PropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var property = Expression.Property(parameter, prop);

            var convertor = TypeDescriptor.GetConverter(prop.PropertyType);
            var value = convertor.ConvertFromInvariantString(Value);
            var expVal = Expression.Constant(value);

            return GetExpression<T>(property, expVal, prop);
        }
        private Expression GetExpression<T>(MemberExpression property, ConstantExpression expVal,
            PropertyInfo prop)
        {
            switch (Operator)
            {
                case Operators.Equal:
                    return Expression.Equal(property, expVal);
                case Operators.Not:
                    return Expression.NotEqual(property, expVal);
                case Operators.Like:
                    if (prop.PropertyType != typeof(string))
                    {
                        throw new Exception("move this error to modelstate handler");
                    }
                    MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    return Expression.Call(property, method, expVal);
                case Operators.GreaterThan:
                    if (!prop.PropertyType.GetInterfaces().Contains(typeof(IComparable)))
                    {
                        throw new Exception("move this error to modelstate handler");
                    }
                    return Expression.GreaterThan(property, expVal);
                case Operators.GreaterThanEqual:
                    if (!prop.PropertyType.GetInterfaces().Contains(typeof(IComparable)))
                    {
                        throw new Exception("move this error to modelstate handler");
                    }
                    return Expression.GreaterThanOrEqual(property, expVal);
                case Operators.LessThan:
                    if (!prop.PropertyType.GetInterfaces().Contains(typeof(IComparable)))
                    {
                        throw new Exception("move this error to modelstate handler");
                    }
                    return Expression.LessThan(property, expVal);
                case Operators.LessThanEqual:
                    if (!prop.PropertyType.GetInterfaces().Contains(typeof(IComparable)))
                    {
                        throw new Exception("move this error to modelstate handler");
                    }
                    return Expression.LessThanOrEqual(property, expVal);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
