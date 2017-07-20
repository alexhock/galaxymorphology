using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AgglomLib
{
    public class AgglomerativeClustering
    {
        private int numThreads = 1;
        public AgglomerativeClustering(int numThreads) { this.numThreads = numThreads; }

        public List<Cluster> Cluster(List<float[]> rows, Similarity similarity, int numNodes)
        {
            Stopwatch w = new Stopwatch();
            Int64 totalDurationMs = 0;

            int numDims = rows[0].Length;
            var distances = new Dictionary<Tuple<int, int>, double>();
            int currClusterId = -1;

            w.Start();

            // init: set the clusters to be the same as the rows
            var clusters = new List<Cluster>();
            for (int i = 0; i < rows.Count; i++)
                clusters.Add(new Cluster(rows[i], id: i));

            Console.WriteLine("Starting Clusters: {0}", clusters.Count);

            // create grid to cache distances
            // not very memory efficient....understatement of the year
            int numSamples = numNodes * 2;
            float[][] distances3 = new float[numSamples][];
            for (int i = 0; i < numSamples; i++)
                distances3[i] = new float[numSamples];
            for (int p = 0; p < numSamples; p++)
                for (int q = 0; q < numSamples; q++)
                    distances3[p][q] = -10.0f; // set to -10 for uninitialized

            int offset = distances3.Length / 2;

            w.Stop();
            Console.WriteLine("Initialization {0} duration: {1}", offset, w.ElapsedMilliseconds);
            w.Reset();

            long lastIterDuration = long.MaxValue;

            while (clusters.Count > 1)
            {
                w.Start();

                double[] nearestPair = Pairwise.FindNearestPair(this.numThreads, clusters, similarity, distances3, offset, lastIterDuration);

                int[] lowestPair = new int[] { (int)nearestPair[0], (int)nearestPair[1] };
                double closest = nearestPair[2];
                Int64 cacheMisses = (Int64) nearestPair[3];

                Cluster newCluster = CalculateAvgCluster(clusters, lowestPair, closest, currClusterId, numDims);

                currClusterId--;
                clusters.RemoveAt(lowestPair[1]); // remove the one to the right first of all.
                clusters.RemoveAt(lowestPair[0]);
                clusters.Add(newCluster);

                w.Stop();
                lastIterDuration = w.ElapsedMilliseconds;
                totalDurationMs += w.ElapsedMilliseconds;
                w.Reset();
                if (clusters.Count % 100 == 0)
                    Console.WriteLine("Clusters: {0} Duration: {1} CacheMisses: {2} cumulative mins:{3}", clusters.Count, lastIterDuration, cacheMisses, totalDurationMs/1000/60);
            }

            Console.WriteLine("TotalDuration MS: {0}   TotalDuration Secs: {1}  Total Duration Mins: {2}", totalDurationMs, totalDurationMs/1000, totalDurationMs/1000/60);

            return clusters;
        }


        private Cluster CalculateAvgCluster(List<Cluster> clusters, int[] lowestPair, double closest, int currClusterId, int numDims)
        {
            // we have radically different cluster sizes so halfing the distance won't work. Need to re-calculate centroid.
            var leafClusters1 = new List<Cluster>();
            Tree.GetLeafClusters(clusters[lowestPair[0]], leafClusters1);
            var leafClusters2 = new List<Cluster>();
            Tree.GetLeafClusters(clusters[lowestPair[1]], leafClusters2);
            
            float[] newCentroid = new float[numDims];
            for (int p = 0; p < leafClusters1.Count; p++)
            {
                for (int q = 0; q < numDims; q++)
                    newCentroid[q] += leafClusters1[p].Vector[q];
            }
            for (int p = 0; p < leafClusters2.Count; p++)
            {
                for (int q = 0; q < numDims; q++)
                    newCentroid[q] += leafClusters2[p].Vector[q];
            }
            for (int q = 0; q < numDims; q++)
                newCentroid[q] /= (leafClusters1.Count + leafClusters2.Count);

            // create the new cluster
            var newCluster = new Cluster(
                newCentroid,//newMergedVec, 
                left: clusters[lowestPair[0]],
                right: clusters[lowestPair[1]],
                distance: closest,
                id: currClusterId);
            
            if (newCluster.Left != null)
                newCluster.Left.Parent = newCluster;
            if (newCluster.Right != null)
                newCluster.Right.Parent = newCluster;

            int clusterId0 = clusters[lowestPair[0]].Id;
            int clusterId1 = clusters[lowestPair[1]].Id;
            if (clusterId0 < 0) clusterId0 += 20;
            if (clusterId1 < 0) clusterId1 += 20;

            //Console.WriteLine("{0}, {1}, {2}", clusterId0, clusterId1, closest);

            return newCluster;
        }
    }
}
