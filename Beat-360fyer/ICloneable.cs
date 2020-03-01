using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    public interface ICloneable<T>
    {
        T Clone();
    }

    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Clone<T>(this IEnumerable<T> items) where T : ICloneable<T>
        {
            foreach (var item in items)
                yield return item.Clone();
        }
    }
}
