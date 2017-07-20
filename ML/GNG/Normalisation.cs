using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNG
{
    public class Normalisation
    {
        public static List<float[]> NormalizeData(List<float[]> samples)
        {
            float[] mu = Mean(samples);
            float[] std = StandardDeviation(samples);

            int ndim = samples[0].Length;
            List<float[]> meanNormalizedSamples = new List<float[]>();
            // subtract mean from each sample
            foreach (float[] sample in samples)
            {
                for (int i = 0; i < ndim; i++)
                {
                    sample[i] -= mu[i];
                    sample[i] /= std[i];
                }
                meanNormalizedSamples.Add(sample);
            }

            return meanNormalizedSamples;
        }

        public static float[] Mean(List<float[]> samples)
        {
            int ndim = samples[0].Length;
            float[] mu = new float[ndim];

            // calc mean
            foreach (float[] sample in samples)
            {
                for (int i = 0; i < ndim; i++)
                    mu[i] += sample[i];
            }

            for (int i = 0; i < ndim; i++)
                mu[i] /= samples.Count;

            return mu;
        }

        public static double StandardDeviation(List<double> valueList)
        {
            double M = 0.0;
            double S = 0.0;
            int k = 1;
            foreach (double value in valueList)
            {
                double tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return Math.Sqrt(S / (k - 2));
        }

        public static double[] StandardDeviation(List<double[]> valueList)
        {
            int ndim = valueList[0].Length;

            double[] M = new double[ndim];
            double[] S = new double[ndim];
            int k = 1;
            foreach (double[] value in valueList)
            {
                for (int i = 0; i < ndim; i++)
                {
                    double tmpM = M[i];
                    M[i] += (value[i] - tmpM) / k;
                    S[i] += (value[i] - tmpM) * (value[i] - M[i]);
                }
                k++;
            }

            double[] std = new double[ndim];
            for (int i = 0; i < ndim; i++)
            {
                std[i] = Math.Sqrt(S[i] / (k - 2)); // if whole population then k - 1
            }

            return std;
        }


        public static float[] StandardDeviation(List<float[]> valueList)
        {
            int ndim = valueList[0].Length;

            double[] M = new double[ndim];
            double[] S = new double[ndim];
            int k = 1;
            foreach (float[] value in valueList)
            {
                for (int i = 0; i < ndim; i++)
                {
                    double tmpM = M[i];
                    M[i] += (value[i] - tmpM) / k;
                    S[i] += (value[i] - tmpM) * (value[i] - M[i]);
                }
                k++;
            }

            float[] std = new float[ndim];
            for (int i = 0; i < ndim; i++)
            {
                double dstd = Math.Sqrt(S[i] / (k - 2)); // if whole population then k - 1
                std[i] = (float)dstd; // warning reduces precision
            }

            return std;
        }
    }
}
