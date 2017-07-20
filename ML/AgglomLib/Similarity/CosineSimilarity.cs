using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgglomLib
{
    public class CosineSimilarity : Similarity
    {
        double Similarity.GetDistance(Cluster one, Cluster two)
        {
            int N = one.Vector.Length;
            double dot = 0.0d;
            double mag1 = 0.0d;
            double mag2 = 0.0d;
            for (int n = 0; n < N; n++)
            {
                dot += one.Vector[n] * two.Vector[n];
                mag1 += Math.Pow(one.Vector[n], 2);
                mag2 += Math.Pow(two.Vector[n], 2);
            }
            return 1.0 - (dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2)));
        }

        public override String ToString()
        {
            return "Cosine";
        }

    }
}
