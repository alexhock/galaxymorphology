using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgglomLib
{
    public class EuclideanDistance : Similarity
    {
        double Similarity.GetDistance(Cluster one, Cluster two)
        {
            double total = 0.0d;
            for (int k = 0; k < one.Vector.Length; k++)
            {
                float val = one.Vector[k] - two.Vector[k];
                total = total + (val * val);
            }
            return Math.Sqrt(total);
        }

        public override String ToString()
        {
            return "EuclideanDistance";
        }
    }
}
