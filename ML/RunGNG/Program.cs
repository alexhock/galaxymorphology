using System;
using System.Collections.Generic;
using GNG;
using Utils;

namespace RunGNG
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("Usage: GNG.exe <datafilepath> <outputfolder> <numepochs> <maxnodes> <numthreads> <metric>");
                Console.WriteLine(@"e.g. GNG.exe c:\data\data.csv c:\data 10 5000 4 1");
                return;
            }

            DateTime dtStart = DateTime.Now;
            Console.WriteLine("Starting: {0}", dtStart);

            String dataFilePath = args[0];
            String outputFolder = args[1];
            int numEpochs = Int32.Parse(args[2]);
            int maxNodes = Int32.Parse(args[3]);
            int numThreads = Int32.Parse(args[4]);            

            // 1 is squared euclidean, 2 is cosine
            int metric = Int32.Parse(args[5]);  

            // load the data text file
            List<float[]> samples = CSVIO.Load<float>(dataFilePath);

            int numDimensions = samples[0].Length;
            
            //
            GNGRunner r = new GNGRunner(numDimensions, maxNodes, numThreads, metric);

            Console.WriteLine("Training: {0}", DateTime.Now);

            // build the graph 
            Graph graph = r.Fit(samples, numEpochs);

            Console.WriteLine("Predicting: {0}", DateTime.Now);

            // identify the cluster each sample is nearest to.
            List<int[]> predictions = r.Predict(samples);


            Console.WriteLine("Saving: {0}", DateTime.Now);

            CSVIO.Save<int>(outputFolder + "/predictions.csv", predictions);

            // save the learnt graph
            SaveGraphData(outputFolder, graph, numDimensions);

            DateTime dtFinished = DateTime.Now;
            Console.WriteLine("Finished: {0} Duration: {1}", dtFinished, dtFinished - dtStart);
        }


        public static void SaveGraphData(String outputFolder, Graph graph, int numDimensions)
        {
            // extract the interesting information
            GraphHelper gh = new GraphHelper(graph, numDimensions);


            // get the graph of the input data

            // get the graph nodes/vertices
            List<float[]> graphVertices = gh.GetNodePositions();
            // get the from to edges
            List<float[]> edges = gh.GetEdges(graphVertices);

            CSVIO.Save<float>(outputFolder + "/graph_nodes.csv", graphVertices);
            CSVIO.Save<float>(outputFolder + "/edge.csv", edges);


            // Get any separate objects

            // get the graph nodes that belong to a connected component.
            List<List<GraphNode>> connComps = gh.GetConnectedComponents();
            // get the mapping
            List<int[]> nodeClusterIndex = gh.GetNodeClusterIndex(connComps);

            // get the average position of the nodes in a connected component
            List<float[]> clusterCenters = gh.GetClusterCenters(connComps, nodeClusterIndex);
            CSVIO.Save<float>(outputFolder + "/cluster_centres.csv", clusterCenters);
            CSVIO.Save<int>(outputFolder + "/node_to_cluster_index.csv", nodeClusterIndex);

        }
    }
}
