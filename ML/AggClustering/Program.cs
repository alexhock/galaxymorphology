using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using AgglomLib;
using Utils;

namespace AggClustering
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting: {0}", DateTime.Now);

            String filePath = args[0];
            String outputFilePath = args[1];
            int k = Int32.Parse(args[2]);
            int distance = Int32.Parse(args[3]);
            int numThreads = Int32.Parse(args[4]);

            var samples = CSVIO.Load<float>(filePath);

            Similarity sim = new EuclideanDistance();
            if (distance == 2)
                sim = new PearsonSimilarity();
            if (distance == 3)
                sim = new CosineSimilarity();

            Console.WriteLine("Using distance measure: {0} on {1} samples of dimensionality: {2}", 
                sim, samples.Count, samples[0].Length);

            Console.WriteLine("Beginning Clustering: {0}", DateTime.Now);

            var clusters = Cluster(samples, sim, numThreads);
            Cluster root = clusters[0];

            Console.WriteLine("Finished Clustering: {0}", DateTime.Now);

            var classifications = Classify(samples, root, k);

            CSVIO.Save<int>(outputFilePath, classifications);

            Console.WriteLine("Finished: {0}", DateTime.Now);
        }

        public static List<Cluster> Cluster(List<float[]> samples, Similarity sim, int numThreads)
        {
            AgglomerativeClustering ac = new AgglomerativeClustering(numThreads);
            var clusters = ac.Cluster(samples, sim, samples.Count);
            return clusters;
        }

        public static List<int[]> Classify(List<float[]> samples, Cluster root, int k)
        {
            var result = new List<int[]>(samples.Count);
            for (int i = 0; i < samples.Count; i++)
                result.Add(new int[] { 0 });

            // Get the top k clusters            
            var roots = Tree.GetKRoots(root, k);

            // loop through the clusters to identify which samples belong to each branch. 
            // the branch id becomes the classification
            for(int i=0;i< roots.Count;i++)            
            {
                int classification = i;

                Cluster branchRoot = roots[i];
                List<int> branchLeafIds = new List<int>();
                Tree.GetLeafs(branchRoot, branchLeafIds);

                foreach (int sampleId in branchLeafIds)
                    result[sampleId] = new int[] { classification };
            }

            return result;
        }
    }
}
