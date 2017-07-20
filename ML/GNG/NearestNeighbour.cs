using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNG
{
    public class NearestNeighbour
    {
        static int ACTIVE = 1;
        public enum Metric { Euclidean, Cosine };

        public static double[,] GetTwoNearestNeighbours(int metric, int num_threads, int num_nodes, int num_dim, float[] sample, float[][] data)
        {
            if (num_threads == 1 || num_nodes < 10)
            {
                switch (metric)
                {
                    case 1: // euclidean
                        return FindTwoNNEuclidean(num_dim, sample, data, 0, num_nodes);
                    case 2: // cosine
                        return FindTwoNNCosine(num_dim, sample, data, 0, num_nodes);
                    case 3: // pearson
                        return FindTwoNNPearson(num_dim, sample, data, 0, num_nodes);
                    default:
                        return FindTwoNNEuclidean(num_dim, sample, data, 0, num_nodes);
                }                
            }
            else
            {
                int[,] indexes = GetIndexes(num_nodes, num_threads);
                List<double[,]> threadResults = RunThreads(metric, num_threads, num_dim, indexes, sample, data);
                return GetResult(threadResults);
            }
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

        public static double[,] GetResult(List<double[,]> threadResults)
        {
            double[,] results = new double[3, 2];
            double first_dist = Double.MaxValue;
            int first_index = -1, second_index = -1;
            double second_dist = Double.MaxValue;            

            for (int i = 0; i < threadResults.Count; i++)
            {
                double[,] nn = threadResults[i];
                if (nn[0, 0] < first_dist)
                {
                    second_dist = first_dist;
                    second_index = first_index;
                    first_dist = nn[0, 0];
                    first_index = (int)nn[0, 1];
                }
                else
                {
                    if (nn[0, 0] < second_dist)
                    {
                        second_dist = nn[0, 0];
                        second_index = (int)nn[0, 1];
                    }
                }
                if (nn[1, 0] < second_dist)
                {
                    second_dist = nn[1, 0];
                    second_index = (int)nn[1, 1];
                }

                results[2, 0] += nn[2, 0]; // count
                //Console.Write("{0} {1} {2} {3}", nearest_neighbours[0,0], nearest_neighbours[0,1], nearest_neighbours[1,0], nearest_neighbours[1,1]);
            }

            results[0, 0] = first_dist;
            results[0, 1] = first_index;
            results[1, 0] = second_dist;
            results[1, 1] = second_index;
            return results;
        }

        public static List<double[,]> RunThreads(int metric, int num_threads, int num_dim, int[,] indexes, float[] sample, float[][] data)
        {
            Task<double[,]>[] taskArray = new Task<double[,]>[num_threads];

            // start theads
            for (int iter = 0; iter < num_threads; iter++)
            {
                int tstart_idx = indexes[iter, 0]; // must copy local here see:
                int tend_idx = indexes[iter, 1]; // must copy local here see:
                switch(metric)
                {
                    case 1:
                        taskArray[iter] = Task<double[,]>.Factory.StartNew(() => { return FindTwoNNEuclidean(num_dim, sample, data, tstart_idx, tend_idx); });
                        break;
                    case 2:
                        taskArray[iter] = Task<double[,]>.Factory.StartNew(() => { return FindTwoNNCosine(num_dim, sample, data, tstart_idx, tend_idx); });
                        break;
                    case 3:
                        taskArray[iter] = Task<double[,]>.Factory.StartNew(() => { return FindTwoNNPearson(num_dim, sample, data, tstart_idx, tend_idx); });
                        break;
                }
            }

            // get results
            List<double[,]> results = new List<double[,]>();
            for (int iter = 0; iter < num_threads; iter++)
            {
                results.Add(taskArray[iter].Result); // .Result will block until thread is finished.                
            }

            return results;
        }


        public static double[,] FindTwoNNCosine(int ndim, float[] a, float[][] data, int start_idx, int end_idx)
        {
            double[,] results = new double[3, 2];

            double first_dist = Double.MaxValue;
            int first_index = -1;
            double second_dist = Double.MaxValue;
            int second_index = -1;

            double dist = 0.0d;

            int status_dim = ndim;
            int job = -1;
            for (job = start_idx; job < end_idx; job++)
            {
                if (data[job][status_dim] != ACTIVE)
                    continue;

                /////////////// start similarity
                /*
                double total = 0;
                for (int k = 0; k < ndim; k++)
                {
                    double val = a[k] - data[job][k];
                    total = total + (val * val);
                }
                dist = total;
                */
                //
                double dot = 0.0d;
                double mag1 = 0.0d;
                double mag2 = 0.0d;
                for (int n = 0; n < ndim; n++)
                {
                    dot += a[n] * data[job][n];
                    mag1 += Math.Pow(a[n], 2);
                    mag2 += Math.Pow(data[job][n], 2);
                }

                double sim = dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));

                dist = 1.0 - sim; // equal to 1 means the same. less than one means different


                ////////////// end similarity


                // check if new nearest or second nearest neighbour
                if (dist < first_dist)
                {
                    second_dist = first_dist;
                    second_index = first_index;
                    first_dist = dist;
                    first_index = job;
                }
                else
                {
                    if (dist < second_dist)
                    {
                        second_dist = dist;
                        second_index = job;
                    }
                }
            }

            results[0, 0] = first_dist; //Math.Sqrt(first_dist);
            results[0, 1] = first_index;

            results[1, 0] = second_dist;// Math.Sqrt(second_dist);
            results[1, 1] = second_index;
            //Console.WriteLine("dist0: {0} index0: {1} dist1:{2} index1:{3}", results[0, 0], first_index, results[1, 0], second_index);

            results[2, 0] = job - start_idx;

            return results;        
        }

        public static double[,] FindTwoNNPearson(int ndim, float[] a, float[][] data, int start_idx, int end_idx)
        {
            double[,] results = new double[3, 2];

            double first_dist = Double.MaxValue;
            int first_index = -1;
            double second_dist = Double.MaxValue;
            int second_index = -1;

            double dist = 0.0d;

            int status_dim = ndim;
            int job = -1;
            for (job = start_idx; job < end_idx; job++)
            {
                if (data[job][status_dim] != ACTIVE)
                    continue;

                /////////////// start similarity

                double dot = 0.0d;
                double mag1 = 0.0d;
                double mag2 = 0.0d;
                for (int n = 0; n < ndim; n++)
                {
                    dot += a[n] * data[job][n];
                    mag1 += Math.Pow(a[n], 2);
                    mag2 += Math.Pow(data[job][n], 2);
                }

                double num = dot - (mag1 * mag2 / ndim);
                double temp = (mag1 - Math.Pow(a.Sum(), 2) / ndim) * (mag2 - Math.Pow(data[job].Sum(), 2) / ndim);
                double den = Math.Sqrt(Math.Abs(temp)); //?????

                if (den == 0)
                    dist = 0;
                else
                    dist = 1.0 - num / den;

                ////////////// end similarity


                // check if new nearest or second nearest neighbour
                if (dist < first_dist)
                {
                    second_dist = first_dist;
                    second_index = first_index;
                    first_dist = dist;
                    first_index = job;
                }
                else
                {
                    if (dist < second_dist)
                    {
                        second_dist = dist;
                        second_index = job;
                    }
                }
            }

            results[0, 0] = first_dist; //Math.Sqrt(first_dist);
            results[0, 1] = first_index;

            results[1, 0] = second_dist;// Math.Sqrt(second_dist);
            results[1, 1] = second_index;
            //Console.WriteLine("dist0: {0} index0: {1} dist1:{2} index1:{3}", results[0, 0], first_index, results[1, 0], second_index);

            results[2, 0] = job - start_idx;

            return results;
        }

        public static double GetCosineSimilarity(List<double> V1, List<double> V2)
        {
            int N = 0;
            N = ((V2.Count < V1.Count) ? V2.Count : V1.Count);
            double dot = 0.0d;
            double mag1 = 0.0d;
            double mag2 = 0.0d;
            for (int n = 0; n < N; n++)
            {
                dot += V1[n] * V2[n];
                mag1 += Math.Pow(V1[n], 2);
                mag2 += Math.Pow(V2[n], 2);
            }

            return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
        }

        public static double[,] FindTwoNNEuclidean(int ndim, float[] a, float[][] data, int start_idx, int end_idx)
        {
            double[,] results = new double[3, 2];

            double first_dist = Double.MaxValue;
            int first_index = -1;
            double second_dist = Double.MaxValue;
            int second_index = -1;

            double dist = 0.0d;

            int status_dim = ndim;
            int job = -1;
            for (job = start_idx; job < end_idx; job++)
            {
                if (data[job][status_dim] != ACTIVE)
                    continue;

                double total = 0;
                for (int k = 0; k < ndim; k++)
                {
                    double val = a[k] - data[job][k];
                    total = total + (val * val);
                }
                dist = total;

                // check if new nearest or second nearest neighbour
                if (dist < first_dist)
                {
                    second_dist = first_dist;
                    second_index = first_index;
                    first_dist = dist;
                    first_index = job;
                }
                else
                {
                    if (dist < second_dist)
                    {
                        second_dist = dist;
                        second_index = job;
                    }
                }
            }

            results[0, 0] = first_dist; //Math.Sqrt(first_dist);
            results[0, 1] = first_index;

            results[1, 0] = second_dist;// Math.Sqrt(second_dist);
            results[1, 1] = second_index;
            //Console.WriteLine("dist0: {0} index0: {1} dist1:{2} index1:{3}", results[0, 0], first_index, results[1, 0], second_index);

            results[2, 0] = job - start_idx;

            return results;
        }
    }
    
    public class NNResult
    {
        public GraphNode nearestNeighbour0 = null;
        public double distance0 = 0.0d;
        public int index0 = -1;
        public GraphNode nearestNeightbour1 = null;
        public double distance1 = 0.0d;
        public int index1 = -1;
    }

}
