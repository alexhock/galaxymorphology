using System;
using System.Collections.Generic;
using System.Diagnostics;
using NearestNeighbourLib;
using Utils;

namespace NNClassifier
{
    class Program
    {
        static void Main(string[] args)
        {
            String dataFilePath = args[0];
            String weightsFilePath = args[1];
            String outputFilePath = args[2];
            int metric = Int32.Parse(args[3]);
            int numThreads = Int32.Parse(args[4]);


            Stopwatch w = new Stopwatch();
            w.Start();

            List<double[]> samples = CSVIO.Load<double>(dataFilePath);
            List<double[]> weights = CSVIO.Load<double>(weightsFilePath);
            
            w.Stop();

            long loadingMS = w.ElapsedMilliseconds;
            w.Reset();

            w.Start();

            DistanceFunctions distFunc = distFunc = new SquareEuclideanDistanceFunction();
            if (metric == 2)
                distFunc = new Cosine();
            if (metric == 3)
                distFunc = new Pearson();

            Console.WriteLine("Using distance function with brute force: {0} and numthreads: {1}", metric, numThreads);

            NNAlgorithm nnMethod = null;
            // if euclidean then can use fast kdtree
            if (metric == 1)
                nnMethod = new KDTreeNN(weights, distFunc);
            else
                nnMethod = new BruteForceNN(weights, distFunc);

            List<int[]> nearestNeighbours = NearestNeighbour.GetNearestNeighbours(samples, nnMethod, numThreads);

            w.Stop();

            long vqMS = w.ElapsedMilliseconds;
            w.Reset();


            w.Start();
            CSVIO.Save<int>(outputFilePath, nearestNeighbours);
            w.Stop();

            long savingMS = w.ElapsedMilliseconds;

            Console.WriteLine("Loading Time: {0} NN Time: {1} Saving Time: {2}", loadingMS, vqMS, savingMS);
        }
    }
}
