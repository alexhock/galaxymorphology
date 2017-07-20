using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNG
{
    public class GraphHelper
    {
        Graph g = null;
        int input_dim = -1;

        public GraphHelper() { }

        public GraphHelper(Graph graph, int input_dim)
        {
            this.g = graph;
            this.input_dim = input_dim;
        }

        public List<List<GraphNode>> GetConnectedComponents()
        {
            return this.g.getConnectedComponents();
        }

        public List<int[]> GetNodeClusterIndex(List<List<GraphNode>> clusters)
        {

            //int[,] nodeClusterIndex = new int[this.g.getNodeCount(), 2];
            var nodeClusterIndex = new List<int[]>();

            for (int cluster_index = 0; cluster_index < clusters.Count; cluster_index++)
            {
                var cluster = clusters[cluster_index];
                foreach (var node in cluster)
                {
                    List<GraphNode> nodes = this.g.getNodes();
                    int tnode_index = -1;
                    for (int node_index = 0; node_index < nodes.Count; node_index++)
                    {
                        if (nodes[node_index] == node)
                        {
                            tnode_index = node_index;
                            break;
                        }
                    }
                    int[] index = new int[] {tnode_index, cluster_index };
                    nodeClusterIndex.Add(index);
                }
            }

            //nodeClusterIndex.Sort();

            return nodeClusterIndex;
        }


        public List<float[]> GetClusterCenters(List<List<GraphNode>> clusters, List<int[]> nodeClusterIndex)
        {
            int numClusters = clusters.Count;
            int numNodes = this.g.GetNodeCount();

            List<float[]> clusterCenters = new List<float[]>();
            List<int> clusterNodeCounts = new List<int>();
            for (int clusterIndex = 0; clusterIndex < numClusters; clusterIndex++)
            {
                float[] position = new float[this.input_dim];
                clusterCenters.Add(position);
                clusterNodeCounts.Add(0);
            }

            // add up the positions
            for (int nodeIndex = 0; nodeIndex < numNodes; nodeIndex++)
            {
                int clusterIndex = nodeClusterIndex[nodeIndex][1];
                GraphNode node = this.g.getNode(nodeIndex);
                float[] nodePosition = this.g.GetNodePosition(node);
                float[] clusterCenter = clusterCenters[clusterIndex];
                for (int i = 0; i < this.input_dim; i++)
                {
                    clusterCenter[i] += nodePosition[i];
                }
                clusterCenters[clusterIndex] = clusterCenter;
                clusterNodeCounts[clusterIndex] += 1;
            }

            // divide by the count.
            for (int clusterIndex = 0; clusterIndex < numClusters; clusterIndex++)
            {
                float[] clusterCenter = clusterCenters[clusterIndex];
                int clusterNodeCount = clusterNodeCounts[clusterIndex];
                for (int i = 0; i < this.input_dim; i++)
                {
                    clusterCenter[i] /= clusterNodeCount;
                }
                clusterCenters[clusterIndex] = clusterCenter;
            }

            return clusterCenters;
        }

        /*
        public int[,] GetNodeClusterIndex(List<List<GraphNode>> clusters)
        {
            int[,] nodeClusterIndex = new int[this.g.getNodeCount(), 2];
            for (int i = 0; i < this.g.getNodeCount(); i++)
                nodeClusterIndex[i, 1] = -1; // initialise  cluster ids to -1 to spot errors

            for (int cluster_index = 0; cluster_index < clusters.Count; cluster_index++)
            {
                var cluster = clusters[cluster_index];
                foreach (var node in cluster)
                {
                    List<GraphNode> nodes = this.g.getNodes();
                    int tnode_index = -1;
                    for (int node_index = 0; node_index < nodes.Count; node_index++)
                    {
                        if (nodes[node_index] == node)
                        {
                            tnode_index = node_index;
                            break;
                        }
                    }
                    if (tnode_index == -1)
                        throw new Exception("node in cluster does not exist");
                    if (nodeClusterIndex[tnode_index, 0] != 0)
                        Console.WriteLine("oooops");
                    nodeClusterIndex[tnode_index, 0] = tnode_index;
                    nodeClusterIndex[tnode_index, 1] = cluster_index;
                }
            }

            for (int i = 0; i < this.g.getNodeCount(); i++)
            {
                if (nodeClusterIndex[i, 1] == -1) // initialise  cluster ids to -1 to spot errors
                    Console.WriteLine("index is -1 hasn't been set {0}", i);
            }
            return nodeClusterIndex;
        }
        */
        public List<float[]> GetClusterCenters()
        {
            List<List<GraphNode>> clusters = this.g.getConnectedComponents();
            List<int[]> nodeClusterIndex = GetNodeClusterIndex(clusters);
            List<float[]> clusterCenters = GetClusterCenters(clusters, nodeClusterIndex);
            return clusterCenters;
        }

        public List<float[]> GetClusterCenters(List<List<GraphNode>> clusters, int[,] nodeClusterIndex)
        {
            int numClusters = clusters.Count;
            int numNodes = this.g.GetNodeCount();

            List<float[]> clusterCenters = new List<float[]>();
            List<int> clusterNodeCounts = new List<int>();
            for (int clusterIndex = 0; clusterIndex < numClusters; clusterIndex++)
            {
                float[] position = new float[this.input_dim];
                clusterCenters.Add(position);
                clusterNodeCounts.Add(0);
            }

            // add up the positions
            for (int nodeIndex = 0; nodeIndex < numNodes; nodeIndex++)
            {
                int clusterIndex = nodeClusterIndex[nodeIndex, 1];
                GraphNode node = this.g.getNode(nodeIndex);
                float[] nodePosition = this.g.GetNodePosition(node);
                float[] clusterCenter = clusterCenters[clusterIndex];
                for (int i = 0; i < this.input_dim; i++)
                {
                    clusterCenter[i] += nodePosition[i];
                }
                clusterCenters[clusterIndex] = clusterCenter;
                clusterNodeCounts[clusterIndex] += 1;
            }

            // divide by the count.
            for (int clusterIndex = 0; clusterIndex < numClusters; clusterIndex++)
            {
                float[] clusterCenter = clusterCenters[clusterIndex];
                int clusterNodeCount = clusterNodeCounts[clusterIndex];
                for (int i = 0; i < this.input_dim; i++)
                {
                    clusterCenter[i] /= clusterNodeCount;
                }
                clusterCenters[clusterIndex] = clusterCenter;
            }

            return clusterCenters;
        }

        public List<float[]> GetNodePositions()
        {
            List<float[]> codeVectors = new List<float[]>();

            int numNodes = this.g.GetNodeCount();

            // add up the positions
            for (int nodeIndex = 0; nodeIndex < numNodes; nodeIndex++)
            {
                GraphNode node = this.g.getNode(nodeIndex);
                float[] codeVector = this.g.GetNodePosition(node);
                codeVectors.Add(codeVector);
            }

            return codeVectors;
        }


        private int GetNodeIndex(GraphNode node)
        {
            List<GraphNode> nodes = this.g.getNodes();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == node)
                {
                    return i;
                }
            }
            return -1;
        }

        public static List<int[]> PredictSamples(List<float[]> samples, List<int[]> nodeClusterIndex, List<float[]> positionsList, int maxThreads = 2, int metric = 1)
        {
            var datapointClusterIndex = new List<int[]>(samples.Count);
            
            int numDims = positionsList[0].Length;

            DataTable dt = new DataTable(numDims, positionsList);
            float[][] positionsData = dt.GetData();

            for (int sampleIndex = 0; sampleIndex < samples.Count; sampleIndex++)
            {
                float[] sample = samples[sampleIndex];
                double[,] nnResults = NearestNeighbour.GetTwoNearestNeighbours(metric, maxThreads, positionsList.Count, numDims, sample, positionsData);
                int firstNearestNeighbourNodeIndex = (int) nnResults[0, 1];
                int clusterIndex = nodeClusterIndex[firstNearestNeighbourNodeIndex][1];
                //datapointClusterIndex[sampleIndex] = new int[] { sampleIndex, clusterIndex };
                int[] row = new int[] { sampleIndex, clusterIndex };
                datapointClusterIndex.Add(row);
            }
            
            return datapointClusterIndex;
        }

        public int[,] GetDatapointNodeIndex(List<float[]> samples, List<GraphNode> nearestNodes)
        {
            int[,] datapointNodeIndex = new int[samples.Count, 2];
            for (int datapointIndex = 0; datapointIndex < samples.Count; datapointIndex++)
            {
                GraphNode nearestNode = nearestNodes[datapointIndex];
                int nodeIndex = GetNodeIndex(nearestNode);
                datapointNodeIndex[datapointIndex, 0] = datapointIndex;
                datapointNodeIndex[datapointIndex, 1] = nodeIndex;
            }
            return datapointNodeIndex;
        }

        public int[,] GetDatapointClusterIndex(List<float[]> samples, int[,] nodeClusterIndex, List<GraphNode> nearestNodes)
        {
            //List<GraphNode> nearestNodes = GetNearestNeighbours(samples);
            int[,] datapointClusterIndex = new int[samples.Count, 2];
            for (int datapointIndex = 0; datapointIndex < samples.Count; datapointIndex++)
            {
                GraphNode nearestNode = nearestNodes[datapointIndex];
                int nodeIndex = GetNodeIndex(nearestNode);
                int clusterIndex = nodeClusterIndex[nodeIndex, 1];

                datapointClusterIndex[datapointIndex, 0] = datapointIndex;
                datapointClusterIndex[datapointIndex, 1] = clusterIndex;
            }
            return datapointClusterIndex;
        }

        public int[,] GetEdgeIndex()
        {
            List<GraphEdge> edges = this.g.getEdges();
            int[,] edgeIndex = new int[edges.Count, 2];
            for (int i = 0; i < edges.Count; i++)
            {
                GraphEdge e = edges[i];
                int headNodeIndex = GetNodeIndex(e.getHead());
                int tailNodeIndex = GetNodeIndex(e.getTail());
                edgeIndex[i, 0] = headNodeIndex;
                edgeIndex[i, 1] = tailNodeIndex;
            }

            return edgeIndex;
        }

        public List<float[]> GetEdges(List<float[]> nodePositions)
        {
            List<float[]> lines = new List<float[]>();
            int numDims = nodePositions[0].Length * 2;

            List<GraphEdge> edges = this.g.getEdges();
            int[,] edgeIndex = new int[edges.Count, 2];
            for (int i = 0; i < edges.Count; i++)
            {
                GraphEdge e = edges[i];

                float[] headPos = this.g.GetNodePosition(e.getHead());
                float[] tailPos = this.g.GetNodePosition(e.getTail());

                float[] newLine = new float[numDims];
                headPos.CopyTo(newLine, 0);
                tailPos.CopyTo(newLine, tailPos.Length);
                
                lines.Add(newLine);
            }
            return lines;
        }

        public List<GraphNode> GetNearestNeighbours(List<float[]> input)
        {
            // Assign each point in the input data to the nearest node in
            // the graph. Return the list of the nearest node instances, and
            // the list of distances.

            List<GraphNode> nodes = new List<GraphNode>();
            List<double> distances = new List<double>();
            foreach (float[] pos in input)
            {
                NNResult nnresult = this.g.GetNearestNodes(pos);
                nodes.Add(nnresult.nearestNeighbour0);
                distances.Add(nnresult.distance0);
            }
            return nodes;
        }
    }
}
