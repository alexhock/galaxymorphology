using System;
using System.Collections.Generic;
using System.Drawing;
using Utils;
using nom.tam.fits;
using nom.tam.image;

namespace MaskCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            String fitsPaths = args[0];
            String[] fitsFilePaths = fitsPaths.Split(',');
            for (int i = 0; i < fitsFilePaths.Length; i++)
                fitsFilePaths[i] = fitsFilePaths[i].Trim();

            String positionsFilePath = args[1];
            String outputFilePath = args[2];
            double threshold = double.Parse(args[3]);

            List<float[]> positions = CSVIO.Load<float>(positionsFilePath);

            List<int[]> mask = CreateMask(fitsFilePaths, positions, threshold);

            CSVIO.Save<int>(outputFilePath, mask);

        }

        public static List<int[]> CreateMask(String[] fitsFilePaths, List<float[]> positions, double threshold)
        {
            var mask = new List<int[]>();

            // get images
            var images = new List<double[,]>();
            foreach (String fitsFilePath in fitsFilePaths)
                images.Add(GetImage(fitsFilePath));

            int width = images[0].GetLength(0);
            int height = images[0].GetLength(1);

            foreach (float[] pos in positions)
            {
                int X = (int)pos[1];
                int Y = (int)pos[2];

                bool overThreshold = false;
                for (int i = 0; i < images.Count; i++)
                {
                    double[,] image = images[i];
                    if (image[X, Y] > threshold)
                    {
                        overThreshold = true;
                        break;
                    }

                }
                if (overThreshold)
                    mask.Add(new int[] { 1 });
                else
                    mask.Add(new int[] { 0 });
            }

            return mask;
        }

        private static double[,] GetImage(String fileName)
        {
            Fits fits = new Fits(fileName);
            ImageHDU hdu = GetImageHDU(fits);
            double[,] pixelValues = GetImageData(hdu);
            return pixelValues;
        }

        private static void SaveFITSAsImage(String inputFilename, String outputFilename)
        {
            Fits fits = new Fits(inputFilename);
            ImageHDU hdu = GetImageHDU(fits);
            int bitpix = hdu.BitPix;
            double bZero = hdu.BZero;
            double bScale = hdu.BScale;
            double min = hdu.MinimumValue;
            double max = hdu.MaximumValue;
            double[,] a = GetImageData(hdu);

            for (int x = 0; x < a.GetLength(0); ++x)
            {
                for (int y = 0; y < a.GetLength(1); ++y)
                {
                    min = Math.Min(min, a[x, y]);
                    max = Math.Max(max, a[x, y]);
                }
            }

            Console.Out.WriteLine("Bitpix = " + bitpix + " bzero = " + bZero + " bscale = " + bScale +
                                  " min = " + min + " max = " + max);
            Bitmap bmp = new Bitmap(a.GetLength(0), a.GetLength(1));

            double nBins = Math.Pow(2.0, 16.0) - 1.0;
            double linearScaleFactor = nBins / (max - min);
            double logScaleFactor = nBins / Math.Log10(nBins);
            double byteScaleFactor = 255.0 / nBins;
            double val = Double.NaN;

            for (int x = 0; x < a.GetLength(0); ++x)
            {
                for (int y = 0; y < a.GetLength(1); ++y)
                {
                    val = a[x, y] - min;
                    val = Math.Max(0.0, Math.Min(val, max - min));
                    val = val <= 0.0 ? 0.0 : (Math.Log10(val * linearScaleFactor) * logScaleFactor);
                    val *= byteScaleFactor;
                    bmp.SetPixel(x, y, Color.FromArgb((int)val, (int)val, (int)val));
                }
            }

            bmp.Save(outputFilename, System.Drawing.Imaging.ImageFormat.Png);
        }

        private static double[,] GetImageData(ImageHDU hdu)
        {
            double[,] result = new double[hdu.Axes[1], hdu.Axes[0]];
            double bZero = hdu.BZero;
            double bScale = hdu.BScale;
            double min = hdu.MinimumValue;
            double max = hdu.MaximumValue;
            Console.Out.WriteLine("Starting image read at " + System.DateTime.Now);
            Array[] a = (Array[])hdu.Data.DataArray;
            Console.Out.WriteLine("Done image read at " + System.DateTime.Now);

            switch (hdu.BitPix)
            {
                case 8:
                    {
                        byte[] b = null;
                        for (int y = 0; y < hdu.Axes[0]; ++y)
                        {
                            b = (byte[])a[y];
                            for (int x = 0; x < hdu.Axes[1]; ++x)
                            {
                                result[x, y] = bZero + bScale * (double)b[x];
                            }
                        }
                    }
                    break;
                case 16:
                    {
                        short[] b = null;
                        for (int y = 0; y < hdu.Axes[0]; ++y)
                        {
                            b = (short[])a[y];
                            for (int x = 0; x < hdu.Axes[1]; ++x)
                            {
                                result[x, y] = bZero + bScale * (double)b[x];
                            }
                        }
                    }
                    break;
                case 32:
                    {
                        int[] b = null;
                        for (int y = 0; y < hdu.Axes[0]; ++y)
                        {
                            b = (int[])a[y];
                            for (int x = 0; x < hdu.Axes[1]; ++x)
                            {
                                result[x, y] = bZero + bScale * (double)b[x];
                            }
                        }
                    }
                    break;
                case -32:
                    {
                        float[] b = null;
                        for (int y = 0; y < hdu.Axes[0]; ++y)
                        {
                            b = (float[])a[y];
                            for (int x = 0; x < hdu.Axes[1]; ++x)
                            {
                                result[x, y] = bZero + bScale * (double)b[x];
                            }
                        }
                    }
                    break;
                case -64:
                    {
                        double[] b = null;
                        for (int y = 0; y < hdu.Axes[0]; ++y)
                        {
                            b = (double[])a[y];
                            for (int x = 0; x < hdu.Axes[1]; ++x)
                            {
                                result[x, y] = bZero + bScale * (double)b[x];
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception("Data type not supported.");
            }

            return result;
        }

        private static ImageHDU GetImageHDU(Fits fits)
        {
            int i = 0;

            for (BasicHDU hdu = fits.getHDU(i); hdu != null; ++i)
            {
                if (hdu is ImageHDU)
                {
                    return (ImageHDU)hdu;
                }
            }

            return null;
        }

    }
}
