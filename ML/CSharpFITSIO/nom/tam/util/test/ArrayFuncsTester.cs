namespace nom.tam.util.test
{
	using System;
	using ArrayFuncs = nom.tam.util.ArrayFuncs;
	
	public class ArrayFuncsTester
	{
		
		/// <summary>Test and demonstrate the ArrayFuncs methods.
		/// </summary>
		/// <param name="args">Unused.
		/// 
		/// </param>
		[STAThread]
//		public static void  Main(String[] args)
      public static void Test(String[] args)
		{
			
			int[][][] test1 = new int[10][][];
			for (int i = 0; i < 10; i++)
			{
				test1[i] = new int[9][];
				for (int i2 = 0; i2 < 9; i2++)
				{
					test1[i][i2] = new int[8];
          for(int i3 = 0; i3 < 8; ++i3)
          {
            test1[i][i2][i3] = SupportClass.Random.Next(9);
          }
				}
			}
			bool[][] test2 = new bool[4][];
			test2[0] = new bool[5];
			test2[1] = new bool[4];
			test2[2] = new bool[3];
			test2[3] = new bool[2];
			
			double[][] test3 = new double[10][];
			for (int i3 = 0; i3 < 10; i3++)
			{
				test3[i3] = new double[20];
			}
			System.Text.StringBuilder[][] test4 = new System.Text.StringBuilder[3][];
			for (int i4 = 0; i4 < 3; i4++)
			{
				test4[i4] = new System.Text.StringBuilder[2];
			}
			
			System.Console.Out.WriteLine("getBaseClass:  test1: Base type of integer array is:" + ArrayFuncs.GetBaseClass(test1));
			System.Console.Out.WriteLine("getBaseLength: test1: Base length is               : " + ArrayFuncs.GetBaseLength(test1));
			System.Console.Out.WriteLine("arrayDescription of test1: \n" + ArrayFuncs.ArrayDescription(test1));
			System.Console.Out.WriteLine("computeSize of test1 (10*9*8)*4:   " + ArrayFuncs.ComputeSize(test1));
			System.Console.Out.WriteLine("\n");
			System.Console.Out.WriteLine("getBaseClass:  test2: Base type of boolean array is:" + ArrayFuncs.GetBaseClass(test2));
			System.Console.Out.WriteLine("getBaseLength: test2: " + ArrayFuncs.GetBaseLength(test2));
			System.Console.Out.WriteLine("arrayDescription of  test2: \n" + ArrayFuncs.ArrayDescription(test2));
			System.Console.Out.WriteLine("computeSize of test2 (5+4+3+2)*1:   " + ArrayFuncs.ComputeSize(test2));
			System.Console.Out.WriteLine("\n");
			System.Console.Out.WriteLine("getBaseClass:  test3: Base type of double array is: " + ArrayFuncs.GetBaseClass(test3));
			System.Console.Out.WriteLine("getBaseLength: test3: " + ArrayFuncs.GetBaseLength(test3));
			System.Console.Out.WriteLine("arrayDescription of test3: \n" + ArrayFuncs.ArrayDescription(test3));
			System.Console.Out.WriteLine("computeSize of test3 (10*20)*8:   " + ArrayFuncs.ComputeSize(test3));
			System.Console.Out.WriteLine("\n");
			System.Console.Out.WriteLine("getBaseClass:  test4: Base type of StringBuffer array is: " + ArrayFuncs.GetBaseClass(test4));
			System.Console.Out.WriteLine("getBaseLength: test4: (should be -1)" + ArrayFuncs.GetBaseLength(test4));
			System.Console.Out.WriteLine("arrayDescription of test4: " + ArrayFuncs.ArrayDescription(test4));
			System.Console.Out.WriteLine("computeSize: test4 (should be 0):   " + ArrayFuncs.ComputeSize(test4));
			System.Console.Out.WriteLine("\n");
			System.Console.Out.WriteLine("\n");
			
			
//			System.Console.Out.WriteLine("examinePrimitiveArray: test1");
			//ArrayFuncs.ExaminePrimitiveArray(test1);
			//System.Console.Out.WriteLine("");
			//System.Console.Out.WriteLine("examinePrimitiveArray: test2");
			//ArrayFuncs.ExaminePrimitiveArray(test2);
			//System.Console.Out.WriteLine("");
			//System.Console.Out.WriteLine("    NOTE: this should show that test2 is not a rectangular array");
			//System.Console.Out.WriteLine("");
			
			System.Console.Out.WriteLine("Using aggregates:");
			System.Object[] agg = new System.Object[4];
			agg[0] = test1;
			agg[1] = test2;
			agg[2] = test3;
			agg[3] = test4;
			
			System.Console.Out.WriteLine("getBaseClass: agg: Base class of aggregate is:" + ArrayFuncs.GetBaseClass(agg));
			System.Console.Out.WriteLine("Size of aggregate is (2880+14+1600+0):" + ArrayFuncs.ComputeSize(agg));
			System.Console.Out.WriteLine("This ignores the array of StringBuffers");
			
/*
      ArrayFuncs.testPattern(test1, (sbyte) 0);
			System.Console.Out.WriteLine("testPattern:");
			for (int i = 0; i < test1.Length; i += 1)
			{
				for (int j = 0; j < test1[0].Length; j += 1)
				{
					for (int k = 0; k < test1[0][0].Length; k += 1)
					{
						System.Console.Out.Write(" " + test1[i][j][k]);
					}
					System.Console.Out.WriteLine("");
				}
				System.Console.Out.WriteLine(""); // Double space....
			}
	*/		
			
			int[][][] test5 = (int[][][]) ArrayFuncs.DeepClone(test1);
			System.Console.Out.WriteLine("deepClone: copied array");
			for (int i = 0; i < test5.Length; i += 1)
			{
				for (int j = 0; j < test5[0].Length; j += 1)
				{
					for (int k = 0; k < test5[0][0].Length; k += 1)
					{
						System.Console.Out.Write(" " + test5[i][j][k]);
					}
					System.Console.Out.WriteLine("");
				}
				System.Console.Out.WriteLine(""); // Double space....
			}
			
			
			test5[2][2][2] = 99;
			System.Console.Out.WriteLine("Demonstrating that this is a deep clone:" + test5[2][2][2] + " " + test1[2][2][2]);
			
			
			
			System.Console.Out.WriteLine("Flatten an array:");
			int[] test6 = (int[]) ArrayFuncs.Flatten(test1);
			System.Console.Out.WriteLine("    arrayDescription of test6:" + ArrayFuncs.ArrayDescription(test6));
			for (int i = 0; i < test6.Length; i += 1)
			{
				System.Console.Out.Write(" " + test6[i]);
				if (i > 0 && i % 10 == 0)
					System.Console.Out.WriteLine("");
			}
			System.Console.Out.WriteLine("");
			
			System.Console.Out.WriteLine("Curl an array, we'll reformat test1's data");
			int[] newdims = new int[]{8, 9, 10};

      Array a = ArrayFuncs.Curl(test6, newdims);
      Console.Out.WriteLine("type of a: " + a.GetType().FullName);
			int[,,] test7 = (int[,,])ArrayFuncs.Curl(test6, newdims);
			System.Console.Out.WriteLine("    arrayDescription of test7:" + ArrayFuncs.ArrayDescription(test7));
			
			for (int i = 0; i < test7.GetLength(0); i += 1)
			{
				for (int j = 0; j < test7.GetLength(1); j += 1)
				{
					for (int k = 0; k < test7.GetLength(2); k += 1)
					{
						System.Console.Out.Write(" " + test7[i,j,k]);
					}
					System.Console.Out.WriteLine("");
				}
				System.Console.Out.WriteLine(""); // Double space....
			}
			
			System.Console.Out.WriteLine("");
			System.Console.Out.WriteLine("Test array conversions");
			
//			sbyte[][][] xtest1 = (sbyte[][][]) ArrayFuncs.ConvertArray(test1, typeof(sbyte));
			//System.Console.Out.WriteLine("  xtest1 is of type:" + ArrayFuncs.ArrayDescription(xtest1));
			//System.Console.Out.WriteLine("   test1[3][3][3]=" + test1[3][3][3] + "  xtest1=" + xtest1[3][3][3]);
			
			System.Console.Out.WriteLine("Converting float[700][700] to byte");
			float[][] big = new float[700][];
			for (int i5 = 0; i5 < 700; i5++)
			{
				big[i5] = new float[700];
			}
//			long time = ((System.DateTime.Now.Ticks - 621355968000000000) / 10000) - (long) System.TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now).TotalMilliseconds;
			//sbyte[][] img = (sbyte[][]) ArrayFuncs.ConvertArray(big, typeof(sbyte));
			//long delta = ((System.DateTime.Now.Ticks - 621355968000000000) / 10000) - (long) System.TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now).TotalMilliseconds - time;
			
			//System.Console.Out.WriteLine("  img=" + ArrayFuncs.ArrayDescription(img) + " took " + delta + " ms");
			
			System.Console.Out.WriteLine("End of tests");
		}
	}
}