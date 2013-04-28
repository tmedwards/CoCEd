using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoCEd.Common
{
    public static class LayoutHelper
    {
        public static double MaxOrZero<T>(this IEnumerable<T> items, Func<T, double> selector)
        {
            double value = 0.0;
            foreach (var item in items) value = Math.Max(value, selector(item));
            return value;
        }
    }
}
