using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgglomLib
{
    public class Tree
    {

        public static int GetRoots(Cluster cluster, List<Cluster> roots, float threshold, int depth = 0)
        {
            depth++;

            if (cluster.Distance < threshold)
            {
                roots.Add(cluster);
            }
            else
            {
                if (cluster.Left != null)
                    depth = GetRoots(cluster.Left, roots, threshold, depth);
                if (cluster.Right != null)
                    depth = GetRoots(cluster.Right, roots, threshold, depth);
            }
            return depth;
        }

        public static List<Cluster> GetKRoots(Cluster root, int k)
        {
            List<Cluster> topK = new List<Cluster>();

            // order by distance and size
            List<Cluster> allOrderedbyDistanceandSize = new List<Cluster>();

            GetAllRoots(root, allOrderedbyDistanceandSize);
            allOrderedbyDistanceandSize.Sort(new ClusterComparer());

            // loop through largest to smallest and build top k. Remove parents as you go.
            // scipy uses priorityqueue

            for(int i = 0; i < allOrderedbyDistanceandSize.Count;i++)
            {
                Cluster b = allOrderedbyDistanceandSize[i];

                //int leafCountLeft = GetLeafs()

                if (topK.Contains(b))
                    topK.Remove(b);

                if (b.Left != null)
                    topK.Add(b.Left);

                if (topK.Count == k)
                    break;

                if (b.Right != null)
                    topK.Add(b.Right);

                if (topK.Count == k)
                    break;

            }

            return topK;

        }

        public static List<Cluster> GetAllRoots(Cluster cluster, List<Cluster> allRoots)
        {
            allRoots.Add(cluster);

            if (cluster.Left != null)
                allRoots = GetAllRoots(cluster.Left, allRoots);
            if (cluster.Right != null)
                allRoots = GetAllRoots(cluster.Right, allRoots);

            return allRoots;
        }


        public static void GetLeafs(Cluster cluster, List<int> rootIds)
        {
            if (cluster.Id > -1) // it's a leaf
            {
                rootIds.Add(cluster.Id);
                return;
            }

            if (cluster.Left != null)
                GetLeafs(cluster.Left, rootIds);
            if (cluster.Right != null)
                GetLeafs(cluster.Right, rootIds);

            return;
        }

        public static void GetLeafClusters(Cluster cluster, List<Cluster> leafClusters)
        {
            if (cluster.Id > -1)
            {
                leafClusters.Add(cluster);
                return;
            }
            if (cluster.Left != null)
                GetLeafClusters(cluster.Left, leafClusters);
            if (cluster.Right != null)
                GetLeafClusters(cluster.Right, leafClusters);

            return;
        }
    }


}
