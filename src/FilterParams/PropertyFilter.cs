using System;
using System.Collections.Generic;
using System.Text;

namespace FilterParams
{
    public class PropertyFilter
    {
        public Operators Operator { get; set; }
        public string PropertyName { get; set; }
        public string Value { get; set; }
    }
}
