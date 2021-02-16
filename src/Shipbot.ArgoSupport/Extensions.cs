using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoDeploy.ArgoSupport
{
    public static class Extensions
    {
        public static int GetCollectionHashCode<T>(this ICollection<T> items)
        {
            if (items == null) return 0;

            var total= items.Select(x => x.GetHashCode() * 397)
                .Aggregate((hashCode, next) => hashCode ^ next);

            return total;
        }

        public static bool IsCollectionEqualTo<T>(this ICollection<T> items, ICollection<T> other)
        {
            // some basic comparisons
            if (items == null && other == null)
                return true;

            if (items == null || other == null)
                return false;
            
            if (ReferenceEquals(items, other)) return true;
            
            if (items.Count != other.Count)
                return false;

            return items.GetCollectionHashCode() == other.GetCollectionHashCode();
        }
    }
}