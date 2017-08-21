using System;
using System.Collections.Generic;
using System.Text;

namespace FilterParams
{
    public enum Operators
    {
        Equal,
        Not,
        Like,
        GreaterThan,
        GreaterThanEqual,
        LessThan,
        LessThanEqual
    }
    public static class ParseOperators
    {
        public static Operators FromString(string op)
        {
            switch (op.ToLowerInvariant())
            {
                case "eq":
                    return Operators.Equal;
                case "neq":
                    return Operators.Not;
                case "lk":
                case "like":
                    return Operators.Like;
                case "gt":
                    return Operators.GreaterThan;
                case "gte":
                    return Operators.GreaterThanEqual;
                case "lt":
                    return Operators.LessThan;
                case "lte":
                    return Operators.LessThanEqual;
                default:
                    throw new Exception("Unknown operator " + op);
            }
        }
    }
}
