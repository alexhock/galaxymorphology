using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNG
{
    public class Graph
    {
        DataTable data = null;
        List<GraphNode> nodes = new List<GraphNode>();
        List<GraphEdge> edges = new List<GraphEdge>();
        private int insertNodeCount = 0;
        private int removedNodeCount = 0;
        private int numThreads = 1;
        private int metric = 1; // Euclidean  2 for cosine 3 for pearson

        public Graph(DataTable data, int numThreads = 1, int metric = 1) 
        {
            this.data = data;
            this.numThreads = numThreads;
            this.metric = metric;
        }

        public int InsertNodeCount
        {
            get { return insertNodeCount; }
        }

        public int RemovedNodeCount
        {
            get { return removedNodeCount; }
        }

        public int GetNodeCount()
        {
            return nodes.Count;
        }

        public int GetEdgeCount()
        {
            return edges.Count;
        }

        public List<GraphEdge> getEdges()
        {
            return edges;
        }

        public List<GraphNode> getNodes()
        {
            return this.nodes;
        }

        public GraphNode getNode(int nodeIndex)
        {
            return this.nodes[nodeIndex];
        }

        public float[] GetNodePosition(GraphNode node)
        {
            return this.data.GetData(node.Id);
        }

        public void SetNodePosition(GraphNode node, float[] position)
        {
            this.data.UpdateData(node.Id, position); 
        }

        public DataTable GetDataTable()
        {
            return this.data;
        }

        public GraphNode AddNode(float[] position, int iteration)
        {
            int lastUpdate = iteration - 1;

            GraphNode newNode = new GraphNode(id: insertNodeCount, lastUpdate: lastUpdate);
            this.nodes.Add(newNode);

            // copy data to backup
            this.data.AddData(insertNodeCount, position, newNode);

            insertNodeCount++;

            return newNode;
        }

        private void removeNode(GraphNode node)
        {
            foreach (var edge in node.getEdges())
                removeEdge(edge);

            this.nodes.Remove(node);
        }
        public void RemoveNode(GraphNode node)
        {
            this.data.RemoveData(node.Id);
            removeNode(node);
            removedNodeCount++;
        }

        public GraphEdge addEdge(GraphNode head, GraphNode tail, object data = null)
        {
            var edge = new GraphEdge(head, tail, data);

            head.addEdgeOut(edge);
            tail.addEdgeIn(edge);

            this.edges.Add(edge);
            return edge;
        }

        public void removeEdge(GraphEdge edge)
        {
            var head = edge.getHead();
            head.removeEdgeOut(edge);

            var tail = edge.getTail();
            tail.removeEdgeIn(edge);

            this.edges.Remove(edge);
        }

        public void visit_fct(GraphNode node, Dictionary<GraphNode, GraphNode> visited)
        {
            visited[node] = null;
        }

        public List<List<GraphNode>> getConnectedComponents()
        {
            var visited = new Dictionary<GraphNode, GraphNode>();
            Action<GraphNode, Dictionary<GraphNode, GraphNode>> vf = visit_fct;

            var components = new List<List<GraphNode>>();
            foreach (GraphNode node in this.nodes)
            {
                if (visited.ContainsKey(node))
                    continue;
                components.Add(this.undirected_dfs(node, visited));
            }
            return components;
        }

        public List<GraphNode> undirected_dfs(GraphNode root, Dictionary<GraphNode, GraphNode> visited)
        {
            Func<GraphNode, List<GraphNode>> neighbours_fct = (node => node.getNeighbours());
            return dfs(neighbours_fct, root, visited);
        }


        /// <summary>
        /// core depth-first sort function
        /// changing the neighbors function to return the sons of a node,
        /// its parents, or both one gets normal dfs, reverse dfs, or
        /// dfs on the equivalent undirected graph, respectively
        /// </summary>
        /// <param name="neighbours_fct"></param>
        /// <param name="root"></param>
        /// <param name="visit_fct"></param>
        /// <returns></returns>
        public List<GraphNode> dfs(Func<GraphNode, List<GraphNode>> neighbours_fct, GraphNode root, Dictionary<GraphNode, GraphNode> visited = null)
        {
            List<GraphNode> dfsList = new List<GraphNode>();
            Dictionary<GraphNode, GraphNode> visitedNodes = new Dictionary<GraphNode, GraphNode>();
            visitedNodes.Add(root, null);

            Stack<GraphNode> dfsStack = new Stack<GraphNode>();
            dfsStack.Push(root);

            while (dfsStack.Count > 0)
            {
                var node = dfsStack.Pop();
                dfsList.Add(node);

                // visit the node
                if (visited != null)
                    visited[node] = null;

                // add all children to the stack (if not already visited)
                foreach (var child in neighbours_fct(node))
                {
                    if (!visitedNodes.ContainsKey(child))
                    {
                        visitedNodes.Add(child, null);
                        dfsStack.Push(child);
                    }
                }
            }

            return dfsList;
        }

        public NNResult GetNearestNodes(float[] sample) 
        {
            DataTable dt = GetDataTable();
            double[,] results = NearestNeighbour.GetTwoNearestNeighbours(metric, numThreads, this.insertNodeCount, dt.NumDimensions, sample, dt.GetData());

            int first_index = (int)results[0, 1];
            int second_index = (int)results[1, 1];

            NNResult nnresult = new NNResult();
            nnresult.nearestNeighbour0 = (GraphNode)dt.GetDataObject(first_index);
            nnresult.distance0 = results[0, 0];
            nnresult.index0 = first_index;

            nnresult.nearestNeightbour1 = (GraphNode)dt.GetDataObject(second_index);
            nnresult.distance1 = results[1, 0];
            nnresult.index1 = second_index;

            return nnresult;
        }

    }

}
