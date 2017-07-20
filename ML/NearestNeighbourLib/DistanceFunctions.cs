using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NearestNeighbourLib
{
    /// <summary>
    /// An interface which enables flexible distance functions.
    /// </summary>
    public interface DistanceFunctions
    {
        /// <summary>
        /// Compute a distance between two n-dimensional points.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <returns>The n-dimensional distance.</returns>
        double Distance(double[] p1, double[] p2);

        /// <summary>
        /// Find the shortest distance from a point to an axis aligned rectangle in n-dimensional space.
        /// </summary>
        /// <param name="point">The point of interest.</param>
        /// <param name="min">The minimum coordinate of the rectangle.</param>
        /// <param name="max">The maximum coorindate of the rectangle.</param>
        /// <returns>The shortest n-dimensional distance between the point and rectangle.</returns>
        double DistanceToRectangle(double[] point, double[] min, double[] max);
    }

    /// <summary>
    /// A distance function for our KD-Tree which returns squared euclidean distances.
    /// </summary>
    public class SquareEuclideanDistanceFunction : DistanceFunctions
    {
        /// <summary>
        /// Find the squared distance between two n-dimensional points.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <returns>The n-dimensional squared distance.</returns>
        public double Distance(double[] p1, double[] p2)
        {
            double fSum = 0;
            for (int i = 0; i < p1.Length; i++)
            {
                double fDifference = (p1[i] - p2[i]);
                fSum += fDifference * fDifference;
            }
            return fSum;
        }

        /// <summary>
        /// Find the shortest distance from a point to an axis aligned rectangle in n-dimensional space.
        /// </summary>
        /// <param name="point">The point of interest.</param>
        /// <param name="min">The minimum coordinate of the rectangle.</param>
        /// <param name="max">The maximum coorindate of the rectangle.</param>
        /// <returns>The shortest squared n-dimensional squared distance between the point and rectangle.</returns>
        public double DistanceToRectangle(double[] point, double[] min, double[] max)
        {
            double fSum = 0;
            double fDifference = 0;
            for (int i = 0; i < point.Length; ++i)
            {
                fDifference = 0;
                if (point[i] > max[i])
                    fDifference = (point[i] - max[i]);
                else if (point[i] < min[i])
                    fDifference = (point[i] - min[i]);
                fSum += fDifference * fDifference;
            }
            return fSum;
        }
    }

    public class Pearson : DistanceFunctions
    {

        public double Distance(double[] p1, double[] p2)
        {
            // https://code.google.com/p/sprwikiwordrelatedness/source/browse/trunk/src/main/java/edu/osu/slate/experiments/Pearson.java?r=62
            double[] xVect = p1;
            double[] yVect = p2;

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

        public double DistanceToRectangle(double[] point, double[] min, double[] max)
        {
            return 0.0d;
        }
    }

    
    public class Cosine : DistanceFunctions
    {
        public double Distance(double[] one, double[] two)
        {
            int N = one.Length;
            double dot = 0.0d;
            double mag1 = 0.0d;
            double mag2 = 0.0d;
            for (int n = 0; n < N; n++)
            {
                dot += one[n] * two[n];
                mag1 += Math.Pow(one[n], 2);
                mag2 += Math.Pow(two[n], 2);
            }
            return 1.0 - (dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2)));
        }

        public double DistanceToRectangle(double[] point, double[] min, double[] max)
        {
            return 0.0d;
        }
    }

    public class FractionalEuclidean : DistanceFunctions
    {
        public double Distance(double[] p1, double[] p2)
        {
            double fSum = 0;
            for (int i = 0; i < p1.Length; i++)
            {
                double numerator = (p1[i] - p2[i]);
                numerator = numerator * numerator;

                double denominator = (0.5 * (p1[i] + p2[i]));
                denominator = denominator * denominator;

                fSum += numerator / denominator;               
            }
            return fSum;

        }

        public double DistanceToRectangle(double[] point, double[] min, double[] max)
        {
            return 0.0d;
        }
    }
}
