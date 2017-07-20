using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearestNeighbourLib
{
    public abstract class NNAlgorithm
    {
        protected List<double[]> nodePositions = null;
        protected DistanceFunctions distanceFunction = null;

        public NNAlgorithm(List<double[]> nodePositions, DistanceFunctions distanceFunction)
        {
            this.nodePositions = nodePositions;
            this.distanceFunction = distanceFunction;
        }

        public abstract List<int[]> GetNearestNeighbours(List<double[]> samples, int startIdx, int endIdx);

    }
}
