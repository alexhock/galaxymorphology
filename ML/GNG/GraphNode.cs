using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNG
{
    public class GraphNode
    {
        public double cum_error = 0.0d;
        public int id = -1;
        public int lastUpdate = 0;
        private object data = null;

        private List<GraphEdge> ein = new List<GraphEdge>();
        private List<GraphEdge> eout = new List<GraphEdge>();

        public GraphNode() { }

        public GraphNode(object data)
        {
            this.data = data;
        }

        public GraphNode(int id, int lastUpdate)
        {
            this.id = id;
            this.lastUpdate = lastUpdate;
        }

        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public double getCumError(int ntlen, double d)
        {
            return this.cum_error * Math.Pow(d, (ntlen - this.lastUpdate - 1));
        }

        public void setCumError(int iteration, double cum_error)
        {
            this.lastUpdate = iteration;
            this.cum_error = cum_error;
        }

        public void setData(object data)
        {
            this.data = data;
        }

        public object getData()
        {
            return data;
        }

        public void addEdgeIn(GraphEdge edge)
        {
            ein.Add(edge);
        }

        public void addEdgeOut(GraphEdge edge)
        {
            eout.Add(edge);
        }

        public void removeEdgeIn(GraphEdge edge)
        {
            ein.Remove(edge);
        }

        public void removeEdgeOut(GraphEdge edge)
        {
            eout.Remove(edge);
        }

        public List<GraphEdge> getEdgesIn()
        {
            return this.ein;
        }

        public List<GraphEdge> getEdgesIn(GraphNode from)
        {
            List<GraphEdge> nodeInEdges = new List<GraphEdge>();

            foreach (GraphEdge edge in this.ein)
            {
                if (edge.getHead() == from)
                    nodeInEdges.Add(edge);
            }
            return nodeInEdges;
        }

        public List<GraphEdge> getEdgesOut(GraphNode to)
        {
            List<GraphEdge> nodeOutEdges = new List<GraphEdge>();

            foreach (GraphEdge edge in this.eout)
            {
                if (edge.getTail() == to)
                    nodeOutEdges.Add(edge);
            }
            return nodeOutEdges;
        }

        public List<GraphEdge> getEdges()
        {
            List<GraphEdge> edges = new List<GraphEdge>();
            edges.AddRange(this.ein);
            edges.AddRange(this.eout);
            return edges;
        }

        public List<GraphEdge> getEdges(GraphNode neighbour)
        {
            List<GraphEdge> edges = new List<GraphEdge>();
            edges.AddRange(getEdgesIn(neighbour));
            edges.AddRange(getEdgesOut(neighbour));
            return edges;
        }

        public int getInDegree()
        {
            return this.ein.Count;
        }

        public int getOutDegree()
        {
            return this.eout.Count;
        }

        public int getDegree()
        {
            return this.getInDegree() + this.getOutDegree();
        }

        public List<GraphNode> getInNeighbours()
        {
            List<GraphNode> neighbours = new List<GraphNode>();
            foreach (GraphEdge e in this.ein)
            {
                neighbours.Add(e.getHead());
            }
            return neighbours; // parents
        }

        public List<GraphNode> getOutNeighbours()
        {
            List<GraphNode> neighbours = new List<GraphNode>();
            foreach (GraphEdge e in this.eout)
            {
                neighbours.Add(e.getTail());
            }
            return neighbours; // parents
        }

        public List<GraphNode> getNeighbours()
        {
            List<GraphNode> neighbours = new List<GraphNode>();
            neighbours.AddRange(getInNeighbours());
            neighbours.AddRange(getOutNeighbours());
            return neighbours;
        }
    }
}
