using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomNumber;

namespace GNG
{
    public class GrowingNeuralGas
    {
        public Graph graph = null;

        float eps_b = 0.2f;
        float eps_n = 0.006f;
        int max_age = 50;
        int lambda = 100;
        float alpha = 0.5f;
        double d = 0.995d;
        int max_nodes = 2147483647;
        int metric = 1; // euclidean

        public int tlen = 0;

        int input_dim = -1;

        public GrowingNeuralGas(Graph graph, int metric, int input_dim, int max_nodes = 2147483647, float eps_b = 0.2f, float eps_n = 0.006f,
            int max_age = 50, int lambda = 100, float alpha = 0.5f, double d = 0.995d, List<float[]> startPos = null)
        {
            this.graph = graph;
            this.input_dim = input_dim;

            this.max_nodes = max_nodes;
            this.eps_b = eps_b;
            this.eps_n = eps_n;
            this.max_age = max_age;
            this.lambda = lambda;
            this.alpha = alpha;
            this.d = d;
            this.metric = metric;

            if (startPos != null && startPos.Count == 2)
            {
                GraphNode g1 = graph.AddNode(startPos[0], tlen);
                GraphNode g2 = graph.AddNode(startPos[1], tlen);
                graph.addEdge(g1, g2);
            }
        }

        public void train(List<float[]> input)
        {
            checkInput(input);

            if (this.graph.GetNodeCount() == 0)
            {
                // if missing, generate two initial nodes at random
                // assuming that the input data has zero mean and unit variance,
                // choose the random position according to a gaussian distribution
                // with zero mean and unit variance
                //InitialiseGraph();
                GraphNode g1 = graph.AddNode(input[0], tlen);
                GraphNode g2 = graph.AddNode(input[1], tlen);
                //graph.addEdge(g1, g2);
            }

            foreach (float[] sample in input)
            {
                this.tlen++;

                // step 2 - find the nearest nodes
                //  dists are the squared distances of sample from the 2 nn
                NNResult nnresult = this.graph.GetNearestNodes(sample);
                GraphNode n0 = nnresult.nearestNeighbour0;
                GraphNode n1 = nnresult.nearestNeightbour1;

                // step 3 increase age of the emanating edges
                foreach (GraphEdge e in n0.getEdges())
                {
                    e.incAge();
                }

                double n0CumError = getCumError(n0);
                if (this.metric == 1)
                    n0CumError += Math.Sqrt(nnresult.distance0); // do the sqrt here instead of the nn call
                    //n0CumError += nnresult.distance0; // Fritzke says add the SQUARED distance ......do the sqrt here instead of the nn call
                else
                    n0CumError += Math.Abs(nnresult.distance0); // cosine                
                setCumError(n0, n0CumError);

                // step 5 - move nearest node and neighbours
                this.moveNode(n0, sample, this.eps_b);
                List<GraphNode> neighbours = n0.getNeighbours();
                foreach (GraphNode n in neighbours)
                    moveNode(n, sample, this.eps_n);

                // step 6 update n0<->n1 edge
                if (neighbours.Contains(n1))
                {
                    // should only be one edge
                    List<GraphEdge> edges = n0.getEdges(n1);
                    edges[0].Age = 0;
                }
                else
                {
                    "Adding edge {0} id0: {1} id1: {2} ".Cout(this.tlen, nnresult.index0, nnresult.index1);
                    this.graph.addEdge(n0, n1);
                }

                // step 7
                this.removeOldEdges(n0.getEdges());

                // step 8 - add a new node each lambda steps
                if (this.tlen % this.lambda == 0 && graph.GetNodeCount() < this.max_nodes)
                {
                    foreach (GraphNode zn in this.graph.getNodes())
                    {
                        float[] pos = this.graph.GetNodePosition(zn);

                        double cumError = zn.getCumError(tlen, this.d);

                        //Console.WriteLine("{4} [{0},{1},{2}]  {3}", pos[0], pos[1], pos[2], cumError, tlen);
                    }

                    //if (tlen > 2000)
                    //    Console.WriteLine("stop");

                    this.insertNewNode();
                }

                if (this.tlen % 10000 == 1)
                {
                    Console.WriteLine("Number of Nodes: {0}, Number of edges: {1}", this.graph.GetNodeCount(), this.graph.GetEdgeCount());

                }

                // step 9 - no step 9
            }

            "tlen:{0} insert_node_count: {1} removed_node_count: {2}".Cout(this.tlen, this.graph.InsertNodeCount, this.graph.RemovedNodeCount);
            Console.WriteLine("Number of Nodes: {0}, Number of edges: {1}", this.graph.GetNodeCount(), this.graph.GetEdgeCount());
        }

        private void moveNode(GraphNode node, float[] x, float eps)
        {
            float[] position = this.graph.GetNodePosition(node);

            for (int i = 0; i < position.Length; i++)
            {
                position[i] += (eps * (x[i] - position[i]));
            }

            this.graph.SetNodePosition(node, position);
        }

        private void removeOldEdges(List<GraphEdge> edges)
        {
            foreach (GraphEdge edge in edges)
            {
                if (edge.Age > this.max_age)
                {
                    this.graph.removeEdge(edge);
                    "remove edge {0}".Cout(this.tlen);
                    if (edge.getHead().getDegree() == 0)
                    {
                        "remove head node {0}".Cout(this.tlen);
                        this.graph.RemoveNode(edge.getHead());
                    }
                    if (edge.getTail().getDegree() == 0)
                    {
                        "remove tail node {0}".Cout(this.tlen);
                        this.graph.RemoveNode(edge.getTail());
                    }
                }
            }
        }


        private void insertNewNode()
        {

            // get the node with the highest error
            int index = HighestError.GetHighestErrorNode(this.d, this.tlen, this.graph.getNodes(), 1);
            GraphNode highestErrorNode = this.graph.getNode(index);

            // get the neighbour with the highest error
            GraphNode neighbourWithHighestError = GetNeighbourWithHighestError(highestErrorNode);

            // create new node at the midpoint position between the worst node and its worst neighbour
            float[] midpointPosition = GetMidpointPosition(highestErrorNode, neighbourWithHighestError);
            GraphNode newNode = this.graph.AddNode(midpointPosition, tlen);

            // remove edge and add edges to new midpoint node
            List<GraphEdge> edges = highestErrorNode.getEdges(neighbour: neighbourWithHighestError);
            graph.removeEdge(edges[0]);
            graph.addEdge(highestErrorNode, newNode);
            graph.addEdge(neighbourWithHighestError, newNode);

            // update errors
            double heCumError = getCumError(highestErrorNode);
            heCumError *= this.alpha;
            setCumError(highestErrorNode, heCumError);

            double nheCumError = getCumError(neighbourWithHighestError);
            nheCumError *= this.alpha;
            setCumError(neighbourWithHighestError, nheCumError);

            setCumError(newNode, 0.5 * (heCumError + nheCumError));

            "new node pos: {0} cum error: {1} tlen: {2}".Cout(midpointPosition[0], getCumError(newNode), this.tlen);
        }

        private void checkInput(List<float[]> x)
        {
            if (x[0].Length != this.input_dim)
                throw new Exception("The dimensionality of the sample dimension is different to the input dimension");
        }

        private float[] GetMidpointPosition(GraphNode node, GraphNode node2)
        {
            float[] newPos = new float[this.input_dim];
            float[] pos = this.graph.GetNodePosition(node);
            float[] pos1 = this.graph.GetNodePosition(node2);
            for (int i = 0; i < newPos.Length; i++)
            {
                newPos[i] = (0.5f * (pos[i] + pos1[i]));
            }
            return newPos;
        }

        private GraphNode GetNeighbourWithHighestError(GraphNode inputNode)
        {
            // determine the neighbour with the highest error
            List<GraphNode> neighbours = inputNode.getNeighbours();
            GraphNode neighbourWithHighestError = neighbours[0];
            double nheCumError = getCumError(neighbourWithHighestError);
            foreach (GraphNode node in neighbours)
            {
                double cum_error = getCumError(node);

                if (cum_error > nheCumError)
                {
                    nheCumError = cum_error;
                    neighbourWithHighestError = node;
                }
            }
            return neighbourWithHighestError;
        }

        private void InitialiseGraph()
        {
            float[] position = new float[this.input_dim];
            float[] position2 = new float[this.input_dim];

            SimpleRNG r = new SimpleRNG();
            double r1 = SimpleRNG.GetNormal();
            double r2 = SimpleRNG.GetNormal();

            double r3 = SimpleRNG.GetNormal();
            double r4 = SimpleRNG.GetNormal();

            position[0] = (float)r1;
            position[1] = (float)r2;

            position2[0] = (float)r3;
            position2[1] = (float)r4;

            this.graph.AddNode(position, tlen);
            this.graph.AddNode(position2, tlen);
        }

        private double getCumError(GraphNode node)
        {
            return node.getCumError(this.tlen, this.d);
        }

        private void setCumError(GraphNode node, double cum_error)
        {
            node.setCumError(this.tlen - 1, cum_error);
            //node.lastUpdate = this.tlen - 1;
            //node.CumError = cum_error;
        }
    }
}
