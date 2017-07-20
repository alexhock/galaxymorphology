using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearestNeighbourLib
{
    public class KDTreeNN : NNAlgorithm
    {
        private KDTree<int> tree = null;

        public KDTreeNN(List<double[]> nodePositions, DistanceFunctions distanceFunction) : base(nodePositions, distanceFunction)
        {
            this.tree = CreateKDTree(nodePositions);
        }

        public override List<int[]> GetNearestNeighbours(List<double[]> samples, int startIdx, int endIdx)
        {
            List<int[]> nodeIndex = new List<int[]>();

            for (int i = startIdx; i < endIdx; i++)
            {                
                KDTreeNearestNeighbour<int> nn = tree.NearestNeighbors(samples[i], distanceFunction, 1, -1);
                // get the first item (there should be one)
                nn.MoveNext();
                int nodeIdx = nn.Current;
                nodeIndex.Add(new int[] { i, nodeIdx });

                if (i % 5000 == 0 && i > 0)
                    Console.WriteLine(i);
            }

            return nodeIndex;
        }

        public static KDTree<int> CreateKDTree(List<double[]> nodePositions)
        {
            int numDims = nodePositions[0].Length;
            var pTree = new KDTree<int>(numDims);

            for (int i = 0; i < nodePositions.Count; i++)
            {
                pTree.AddPoint(nodePositions[i], i);
            }

            return pTree;
        }

        public static KDTree<int> CreateKDTree(List<float[]> nodePositions)
        {
            int numDims = nodePositions[0].Length;
            var pTree = new KDTree<int>(numDims);

            for (int i = 0; i < nodePositions.Count; i++)
            {
                double[] td = new double[numDims];
                for (int j = 0; j < numDims; j++)
                    td[j] = nodePositions[i][j];
                //pTree.AddPoint(nodePositions[i], i);
                pTree.AddPoint(td, i);
            }

            return pTree;
        }
    }
}
