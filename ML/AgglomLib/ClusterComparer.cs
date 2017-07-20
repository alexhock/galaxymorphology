using System.Collections.Generic;

namespace AgglomLib
{
    
    public class ClusterComparer : IComparer<Cluster>
    {
        public int Compare(Cluster x, Cluster y)
        {
            double one = x.Distance;
            double two = y.Distance;

            // X is equal so sort on second key of Y
            return one.CompareTo(two)*-1;
        }

    }
}
