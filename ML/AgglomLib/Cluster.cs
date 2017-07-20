using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgglomLib
{
    public class Cluster
    {
        public Cluster Parent;
        public Cluster Left;
        public Cluster Right;
        public float[] Vector;
        public int Id;
        public double Distance;
        public int depth;

        public Cluster(float[] vector, Cluster left = null, Cluster right = null, double distance = 0.0d, int id = 0)
        {
            Vector = vector;
            Left = left;
            Right = right;
            Distance = distance;
            Id = id;
        }
    }
}
