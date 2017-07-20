using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgglomLib
{
    class Pairwise
    {
        private static double[] CalcPairwise(List<Cluster> clusters, Similarity similarity, int startIdx, int endIdx, float[][] cache, int offset)
        {
            double[] lowestPair = new double[] { 0.0d, 1.0d, Double.MaxValue, 0.0d };
            double closest = similarity.GetDistance(clusters[0], clusters[1]);
            Int64 cacheMisses = 0;

            // loop through every pair looking for the smallest distance
            for (int i = startIdx; i < endIdx; i++)
            {
                for (int j = (i + 1); j < clusters.Count; j++)
                {
                    int row = clusters[i].Id + offset;
                    int col = clusters[j].Id + offset;

                    //if (cache[row][col] == -10.0d)
                    //{
                        cache[row][col] = (float)similarity.GetDistance(clusters[i], clusters[j]);
                        cacheMisses++;
                    //}

                    double d = cache[row][col];

                    if (d < closest)
                    {
                        closest = d;
                        lowestPair = new double[] { i, j, closest, 0.0d };
                    }
                }
            }

            lowestPair[3] = cacheMisses;

            return lowestPair;
        }

        public static double[] FindNearestPair(int numThreads, List<Cluster> clusters, Similarity similarity, float[][] cache, int offset, long duration)
        {

            double[] lowestPair = new double[3];
            if (clusters.Count > 100) // if over 300 ms then multithread
            {
                int[,] indices = GetIndexes(clusters.Count, numThreads);
                List<double[]> threadResults = RunThreads(numThreads, indices, clusters, similarity, cache, offset);

                lowestPair = GetBestResult(threadResults);
            }
            else
            {
                lowestPair = CalcPairwise(clusters, similarity, 0, clusters.Count, cache, offset);
            }

            return lowestPair;
        }


        private static List<double[]> RunThreads(int numThreads, int[,] indexes, List<Cluster> clusters, Similarity similarity, float[][] cache, int offset)
        {
            var taskArray = new Task<double[]>[numThreads];

            // start theads
            for (int iter = 0; iter < numThreads; iter++)
            {
                int tstart_idx = indexes[iter, 0]; // must copy local here see:
                int tend_idx = indexes[iter, 1]; // must copy local here see:
                taskArray[iter] = Task<double[]>.Factory.StartNew(() => { return CalcPairwise(clusters, similarity, tstart_idx, tend_idx, cache, offset); });
            }

            // get results
            var results = new List<double[]>();
            for (int iter = 0; iter < numThreads; iter++)
            {
                results.Add(taskArray[iter].Result); // .Result will block until thread is finished.                
            }

            return results;
        }

        public static int[,] GetIndexes(int num_nodes, int num_threads)
        {
            int num_indices = num_threads;
            int[,] indexes = new int[num_indices, 2];
            int increment = (num_nodes / num_indices);
            int start_idx = 0;
            int end_idx = increment;
            for (int i = 0; i < num_indices; i++)
            {
                indexes[i, 0] = start_idx;
                indexes[i, 1] = end_idx;
                start_idx = end_idx;
                end_idx = end_idx + increment;
            }

            // could miss a couple so add them due to rounding errors so add to the last split
            if (indexes[num_indices - 1, 1] < num_nodes)
                indexes[num_indices - 1, 1] = num_nodes;

            return indexes;
        }

        public static double[] GetBestResult(List<double[]> threadResults)
        {
            double[] result = new double[] { -1, -1, Double.MaxValue };

            double cacheMisses = 0;
            for (int i = 0; i < threadResults.Count; i++)
            {
                double[] nn = threadResults[i];
                cacheMisses += nn[3];
                if (nn[2] < result[2])
                    result = nn;
            }

            return new double[] { result[0], result[1], result[2], cacheMisses };
        }


    }
}
