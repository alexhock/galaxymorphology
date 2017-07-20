using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNG
{
    public class HighestError
    {
        public static int GetHighestErrorNode(double d, int ntlen, List<GraphNode> nodes, int num_threads = 1)
        {
            Result result = null;
            if (num_threads == 1 || nodes.Count < 250)
                result = FindHighestErrorNode(d, ntlen, nodes, 0, nodes.Count);
            else
                result = ThreadedHighestError(d, ntlen, nodes, num_threads);

            return result.idx;
        }

        private static Result FindHighestErrorNode(double d, int ntlen, List<GraphNode> nodes, int start_idx, int end_idx)
        {
            int idx = -1;
            double highest_error = -1.0d;
            for (int i = start_idx; i < end_idx; i++)
            {
                GraphNode node = nodes[i];
                double tcum_error = node.getCumError(ntlen, d);
                if (tcum_error > highest_error)
                {
                    highest_error = tcum_error;
                    idx = i;
                }
            }
            return new Result(idx, highest_error);
        }
        /*
        static int[,] GetIndexes(int num_nodes, int num_indices)
        {
            if (num_nodes < 25)
                num_indices = 1;

            int[,] indexes = new int[num_indices, 2];
            int increment = (num_nodes / num_indices);
            int start_idx = 0;
            int end_idx = increment - 1;
            for (int i = 0; i < num_indices; i++)
            {
                indexes[i, 0] = start_idx;
                indexes[i, 1] = end_idx;
                start_idx = end_idx + 1;
                end_idx = end_idx + increment;
            }

            // will prob miss a couple so add them to the last split
            if (indexes[num_indices - 1, 1] < num_nodes)
                indexes[num_indices - 1, 1] = num_nodes;

            return indexes;
        }
        */
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



        static Result ThreadedHighestError(double d, int ntlen, List<GraphNode> nodes, int num_threads)
        {
            Task<Result>[] taskArray = new Task<Result>[num_threads];
            int[,] indexes = GetIndexes(nodes.Count, num_threads);

            for (int iter = 0; iter < num_threads; iter++)
            {
                int start_idx = indexes[iter, 0];
                int end_idx = indexes[iter, 1];
                //Console.WriteLine("i {0} s {1} e {2}", iter, tstart_idx, tend_idx);
                taskArray[iter] = Task<Result>.Factory.StartNew(() => { return FindHighestErrorNode(d, ntlen, nodes, start_idx, end_idx); });
            }

            int idx = -1;
            double cum_error = 0.0d;
            for (int i = 0; i < taskArray.Length; i++)
            {
                Result r = taskArray[i].Result;
                if (r.cum_error > cum_error)
                {
                    cum_error = r.cum_error;
                    idx = r.idx;
                }
                //Console.Write("{0} {1} {2} {3}", r.cum_error, r.idx, cum_error, idx);
            }

            return new Result(idx, cum_error);
        }
    }

    class Result
    {
        public int idx = -1;
        public double cum_error = 0.0d;
        public Result(int idx, double cum_error)
        {
            this.idx = idx;
            this.cum_error = cum_error;
        }

    }
}
