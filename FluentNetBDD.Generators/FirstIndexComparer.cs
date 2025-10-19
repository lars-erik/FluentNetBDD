using System.Collections.Generic;

namespace FluentNetBDD.Generators
{
    internal class FirstIndexComparer<T> : IEqualityComparer<T[]>
    {
        public bool Equals(T[] x, T[] y)
        {
            if (x == null || y == null || x.Length < 1 || y.Length < 1)
            {
                return false;
            }
            return x[0].Equals(y[0]);
        }

        public int GetHashCode(T[] obj)
        {
            return obj[0].GetHashCode();
        }
    }
}