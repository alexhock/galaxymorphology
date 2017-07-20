using System;
using System.Collections.Generic;
using System.Linq;
using GNG;

namespace RunGNG
{
    class GNGRunner
    {

        public int MaxNodes;
        public int NumThreads;
        public int Metric;
        public int NumDimensions;

        public Graph Graph;
        public DataTable GraphNodeData;
        private GrowingNeuralGas GNG;


        public GNGRunner(int numDimensions, int maxNodes, int numThreads = 1, int metric = 1) 
        {
            NumDimensions = numDimensions;
            MaxNodes = maxNodes;
            NumThreads = numThreads;
            Metric = metric;


            // Node Storage. Each node of the graph stores data
            GraphNodeData = new DataTable(numDimensions, estimatedSize: maxNodes);

            // The graph of the input data created and evolved by the GNG algorithm
            Graph = new Graph(GraphNodeData, numThreads: numThreads, metric: metric);

            // The growing neural gas algorithm processes samples and evolves the Graph
            GNG = new GrowingNeuralGas(Graph, metric, numDimensions, max_nodes: maxNodes);
        }

        public Graph Fit(List<float[]> samples, int numEpochs = 1)
        {
            // one epoch, process all samples
            for (int i = 0; i < numEpochs; i++)
            {
                //List<float[]> randomSamples = GetRandomSamples(0, samples.Count, samples.Count, samples);

                GNG.train(samples);
            }

            return Graph;
        }


        public List<int[]> Predict(List<float[]> samples, bool useCentroids = true)
        {
            GraphHelper gh = new GraphHelper(this.Graph, this.NumDimensions);
            
            List<int[]> predictions = null;

            // use the nodes of the graph
            var nodePositions = gh.GetNodePositions();
            var clusters = gh.GetConnectedComponents();
            var nodeClusterIndex = gh.GetNodeClusterIndex(clusters);

            predictions = GraphHelper.PredictSamples(samples, nodeClusterIndex, nodePositions, NumThreads, Metric);

            return predictions;
        }

        public static List<float[]> GetRandomSamples(int min, int max, int numOfRandomSamples, List<float[]> samples)
        {
            var randomSamples = new List<float[]>();

            var orderedList = Enumerable.Range(0, max);
            var rng = new Random();
            int[] randomIndexes = orderedList.OrderBy(c => rng.Next()).ToArray();
            
            // take
            foreach (int rand in randomIndexes)
                randomSamples.Add(samples[rand]);

            return randomSamples;
        }
    }
}
