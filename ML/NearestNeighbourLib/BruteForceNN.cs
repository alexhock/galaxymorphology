using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearestNeighbourLib
{
    public class BruteForceNN : NNAlgorithm
    {
        public BruteForceNN(List<double[]> nodePositions, DistanceFunctions distanceFunction) : base(nodePositions, distanceFunction)        {        }

        public override List<int[]> GetNearestNeighbours(List<double[]> samples, int startIdx, int endIdx)
        {
            Console.WriteLine("Starting Thread: {0} startIdx {1} endIdx {2}", System.Threading.Thread.CurrentThread.ManagedThreadId, startIdx, endIdx);
            List<int[]> nodeIndex = new List<int[]>();

            for (int i = startIdx; i < endIdx; i++)
            {
                double finalDist = Double.MaxValue;
                int[] finalPair = new int[] { -1, -1 };

                for (int j = 0; j < this.nodePositions.Count; j++)
                {
                    double dist = distanceFunction.Distance(samples[i], this.nodePositions[j]);
                    if (dist < finalDist)
                    {
                        finalPair[0] = i;
                        finalPair[1] = j;
                        finalDist = dist;
                    }
                }

                if (i % 2000 == 0)
                    Console.WriteLine("startidx {0} endidx {1} i {2}", startIdx, endIdx, i);

                nodeIndex.Add(finalPair);
            }

            Console.WriteLine("End Thread: {0} startIdx {1} endIdx {2}", System.Threading.Thread.CurrentThread.ManagedThreadId, startIdx, endIdx);
            return nodeIndex;
        }
    }
}
