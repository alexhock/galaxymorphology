using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgglomLib
{
    public class PearsonSimilarity : Similarity
    {
        double Similarity.GetDistance(Cluster one, Cluster two)
        {
            float[] xVect = one.Vector;
            float[] yVect = two.Vector;

            double meanX = 0.0, meanY = 0.0;
            for (int i = 0; i < xVect.Length; i++)
            {
                meanX += xVect[i];
                meanY += yVect[i];
            }

            meanX /= xVect.Length;
            meanY /= yVect.Length;

            double sumXY = 0.0, sumX2 = 0.0, sumY2 = 0.0;
            for (int i = 0; i < xVect.Length; i++)
            {
                sumXY += ((xVect[i] - meanX) * (yVect[i] - meanY));
                sumX2 += Math.Pow(xVect[i] - meanX, 2.0);
                sumY2 += Math.Pow(yVect[i] - meanY, 2.0);
            }

            return 1 - (sumXY / (Math.Sqrt(sumX2) * Math.Sqrt(sumY2)));

        }

        public override String ToString()
        {
            return "Pearson";
        }
    }
}

