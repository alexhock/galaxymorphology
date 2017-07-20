using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearestNeighbourLib
{
    public class NearestNeighbour
    {

        public static List<int[]> GetNearestNeighbours(List<double[]> samples, NNAlgorithm nnMethod, int numThreads = 1)
        {
            int numDim = samples[0].Length;
            int numSamples = samples.Count;

            if (numThreads == 1 || numSamples < 500)
            {
                return nnMethod.GetNearestNeighbours(samples, 0, numSamples);
            }
            else
            {
                int[,] indexes = GetIndexes(samples.Count, numThreads);
                List<int[]> results = RunThreads(samples, nnMethod, indexes, numThreads);
                results.Sort(new IntComp()); // sort to make sure are in the order of the index and not the order they were threaded
                return results;
            }
        }

        private static int[,] GetIndexes(int num_nodes, int num_threads)
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

        private static List<int[]> RunThreads(List<double[]> samples, NNAlgorithm algorithm, int[,] indexes, int numThreads)
        {
            Task<List<int[]>>[] taskArray = new Task<List<int[]>>[numThreads];

            // start theads
            for (int iter = 0; iter < numThreads; iter++)
            {
                int tstart_idx = indexes[iter, 0]; // must copy local here see:
                int tend_idx = indexes[iter, 1]; // must copy local here see:
                taskArray[iter] = Task<List<int[]>>.Factory.StartNew(() => { return algorithm.GetNearestNeighbours(samples, tstart_idx, tend_idx); });
            }

            // get results            
            List<int[]> results = new List<int[]>();
            for (int iter = 0; iter < numThreads; iter++)
            {
                results.AddRange(taskArray[iter].Result); // .Result will block until thread is finished.                
            }

            return results;
        }


        internal class IntComp : IComparer<int[]>
        {
            public int Compare(int[] x, int[] y)
            {
                return x[0].CompareTo(y[0]);
            }
        }
    }
}
