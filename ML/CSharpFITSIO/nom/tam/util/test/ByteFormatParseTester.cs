namespace nom.tam.util.test
{
	/// <summary>This class tests the ByteFormatter and ByteParser classes.
	/// </summary>
	using System;
	using nom.tam.util;
	
	public class ByteFormatParseTester
	{
		
		[STAThread]
		public static void Test(String[] args)
		{
			byte[] buffer = new byte[100000];
			ByteFormatter bf = new ByteFormatter();
			ByteParser bp = new ByteParser(buffer);

			bf.Align = true;
			bf.TruncationThrow = false;

			int[] tint = new int[100];

			tint[0] = Int32.MinValue;
			tint[1] = Int32.MaxValue;
			tint[2] = 0;

			for(int i = 3; i < tint.Length; i += 1)
			{
				tint[i] = (int)(Int32.MaxValue * (2 * (SupportClass.Random.NextDouble() - .5)));
			}

			Console.Out.WriteLine("Check formatting options...\n");
			Console.Out.WriteLine("\n\nFilled, right aligned");
			int colSize = 12;
			int cnt = 0;
			int offset = 0;
			while (cnt < tint.Length)
			{
				offset = bf.format(tint[cnt], buffer, offset, colSize);
				cnt += 1;
				if (cnt % 8 == 0)
				{
					offset = bf.format("\n", buffer, offset, 1);
				}
			}
			Console.Out.WriteLine(BytesToString(buffer));
			
			bp.Offset = 0;
			
			bool error = false;
			for (int i = 0; i < tint.Length; i += 1)
			{
				int chk = bp.getInt(colSize);
				if (chk != tint[i])
				{
					error = true;
					Console.Out.WriteLine("Mismatch at:" + i + " " + tint[i] + "!=" + chk);
				}
				if ((i + 1) % 8 == 0)
				{
					bp.skip(1);
				}
			}
			if (!error)
			{
				Console.Out.WriteLine("No errors in ByteParser: getInt");
			}
			
			Console.Out.WriteLine("\n\nFilled, left aligned");
			bf.Align = false;
			offset = 0;
			colSize = 12;
			cnt = 0;
			offset = 0;
			while (cnt < tint.Length)
			{
				int oldOffset = offset;
				offset = bf.format(tint[cnt], buffer, offset, colSize);
				int nb = colSize - (offset - oldOffset);
				if (nb > 0)
				{
					offset = bf.alignFill(buffer, offset, nb);
				}
				cnt += 1;
				if (cnt % 8 == 0)
				{
					offset = bf.format("\n", buffer, offset, 1);
				}
			}
      Console.Out.WriteLine(BytesToString(buffer));
			
			Console.Out.WriteLine("\n\nUnfilled, left aligned -- no separators (hard to read)");
			offset = 0;
			colSize = 12;
			cnt = 0;
			offset = 0;
			while (cnt < tint.Length)
			{
				offset = bf.format(tint[cnt], buffer, offset, colSize);
				cnt += 1;
				if (cnt % 8 == 0)
				{
					offset = bf.format("\n", buffer, offset, 1);
				}
			}
      Console.Out.WriteLine(BytesToString(buffer));
			
			Console.Out.WriteLine("\n\nUnfilled, left aligned -- single space separators");
			bf.Align = false;
			offset = 0;
			colSize = 12;
			cnt = 0;
			offset = 0;
			while (cnt < tint.Length)
			{
				
				offset = bf.format(tint[cnt], buffer, offset, colSize);
				offset = bf.format(" ", buffer, offset, 1);
				
				cnt += 1;
				if (cnt % 8 == 0)
				{
					offset = bf.format("\n", buffer, offset, 1);
				}
			}
      Console.Out.WriteLine(BytesToString(buffer));
			
			
			Console.Out.WriteLine("\n\nTest throwing of trunction exception");
			
			bf.TruncationThrow = false;
			int val = 1;
			for (int i = 0; i < 10; i += 1)
			{
				offset = bf.format(val, buffer, 0, 6);
        Console.Out.WriteLine("At power:" + i + " in six chars we get:" + BytesToString(buffer));
				val *= 10;
			}
			
			Console.Out.WriteLine("Now enabling TruncationExceptions");
			bf.TruncationThrow = true;
			val = 1;
			for (int i = 0; i < 10; i += 1)
			{
				try
				{
					offset = bf.format(val, buffer, 0, 6);
				}
				catch(TruncationException)
				{
					Console.Out.WriteLine("Caught TruncationException for power " + i);
				}
				Console.Out.WriteLine("At power:" + i + " in six chars we get:" + BytesToString(buffer));
				val *= 10;
			}
			
			long[] lng = new long[100];
			for (int i = 0; i < lng.Length; i += 1)
			{
				lng[i] = (long) (Int64.MaxValue * (2 * (SupportClass.Random.NextDouble() - 0.5)));
			}
			
			lng[0] = Int64.MaxValue;
			lng[1] = Int64.MinValue;
			lng[2] = 0;
			
			bf.TruncationThrow = false;
			bf.Align = true;
			offset = 0;
			cnt = 0;
			while (cnt < lng.Length)
			{
				offset = bf.format(lng[cnt], buffer, offset, 20);
				cnt += 1;
				if (cnt % 4 == 0)
				{
					offset = bf.format("\n", buffer, offset, 1);
				}
			}
			Console.Out.WriteLine("\n\nLongs:\n" + BytesToString(buffer));
			bp.Offset = 0;
			
			error = false;
			for (int i = 0; i < lng.Length; i += 1)
			{
				long chk = bp.getLong(20);
				if (chk != lng[i])
				{
					Console.Out.WriteLine("Error in getLong:" + i + "  " + lng[i] + " != " + chk);
					error = true;
				}
				if ((i + 1) % 4 == 0)
				{
					bp.skip(1);
				}
			}
			if (!error)
			{
				Console.Out.WriteLine("No errors in ByteParser: getLong!");
			}
			
			
			float[] flt = new float[100];
			for (int i = 0; i < flt.Length; i += 1)
			{
				flt[i] = (float) (2 * (SupportClass.Random.NextDouble() - 0.5) * Math.Pow(10, 60 * (SupportClass.Random.NextDouble() - 0.5)));
			}
			
			flt[0] = Single.MaxValue;
			flt[1] = Single.Epsilon;
			flt[2] = 0;
			flt[3] = Single.NaN;
			flt[4] = Single.PositiveInfinity;
			flt[5] = Single.NegativeInfinity;
			
			
			bf.TruncationThrow = false;
			bf.Align = true;
			
			offset = 0;
			cnt = 0;
			while (cnt < flt.Length)
			{
				offset = bf.format(flt[cnt], buffer, offset, 24);
				cnt += 1;
				if (cnt % 4 == 0)
				{
					offset = bf.format("\n", buffer, offset, 1);
				}
			}
			Console.Out.WriteLine("\n\nFloats:\n" + BytesToString(buffer));
			
			bp.Offset = 0;
			double delta = 0;
			for (int i = 0; i < flt.Length; i += 1)
			{
				
				// Skip NaNs and Infinities.
				if (i > 2 && i < 6)
				{
					bp.skip(24);
				}
				else
				{
					
					float chk = bp.getFloat(24);
					
					float dx = Math.Abs(chk - flt[i]);
					if (dx > delta)
					{
						Console.Out.WriteLine("Float  High delta:" + i + " " + flt[i] + " " + chk);
						Console.Out.WriteLine("       Relative error:" + dx / flt[i]);
						delta = dx;
					}
				}
				if ((i + 1) % 4 == 0)
				{
					bp.skip(1);
				}
			}
			
			double[] dbl = new double[100];
			for (int i = 0; i < dbl.Length; i += 1)
			{
				dbl[i] = 2 * (SupportClass.Random.NextDouble() - 0.5) * Math.Pow(10, 60 * (SupportClass.Random.NextDouble() - 0.5));
			}
			
			dbl[0] = Double.MaxValue;
			dbl[1] = Double.MinValue;
			dbl[2] = 0;
			dbl[3] = Double.NaN;
			dbl[4] = Double.PositiveInfinity;
			dbl[5] = Double.NegativeInfinity;
			
			
			bf.TruncationThrow = false;
			bf.Align = true;
			offset = 0;
			cnt = 0;
			while (cnt < lng.Length)
			{
				offset = bf.format(dbl[cnt], buffer, offset, 25);
				cnt += 1;
				if (cnt % 4 == 0)
				{
					offset = bf.format("\n", buffer, offset, 1);
				}
			}
			Console.Out.WriteLine("\n\nDoubles:\n" + BytesToString(buffer));
			
			bp.Offset = 0;
			delta = 0;
			for (int i = 0; i < dbl.Length; i += 1)
			{
				
				// Skip NaNs and Infinities.
				if (i > 2 && i < 6)
				{
					bp.skip(25);
				}
				else
				{
					
					double chk = bp.getDouble(25);
					
					double dx = Math.Abs(chk - dbl[i]);
					if (dx > delta)
					{
						Console.Out.WriteLine("Double  High delta:" + i + " " + dbl[i] + " " + chk);
						Console.Out.WriteLine("       Relative error:" + dx / dbl[i]);
						delta = dx;
					}
				}
				if ((i + 1) % 4 == 0)
				{
					bp.skip(1);
				}
			}
			
			bp.Offset = 0;
			bp.skip(4 * 25 + 1 + 2 * 25);
			for (int i = 0; i < 30; i += 1)
			{
				Console.Out.WriteLine("Reading doubles..." + bp.Double);
			}
			
			bool[] btst = new bool[100];
			for (int i = 0; i < btst.Length; i += 1)
			{
				btst[i] = SupportClass.Random.NextDouble() > 0.5;
			}
			offset = 0;
			for (int i = 0; i < btst.Length; i += 1)
			{
				offset = bf.format(btst[i], buffer, offset, 1);
			}
			Console.Out.WriteLine("Booleans are:" + BytesToString(buffer));
			
			
			bp.Offset = 0;
			for (int i = 0; i < btst.Length; i += 1)
			{
				bool bt = bp.Boolean;
				if (bt != btst[i])
				{
					Console.Out.WriteLine("Mismatch at:" + i + " " + btst[i] + " != " + bt);
				}
			}
			
			offset = 0;
			String bigStr = "abcdefghijklmnopqrstuvwxyz";
			for (int i = 0; i < 100; i += 1)
			{
				offset = bf.format(bigStr.Substring(i % 27), buffer, offset, 13);
				offset = bf.format(" ", buffer, offset, 1);
			}
			
			bp.Offset = 0;
			for (int i = 0; i < 100; i += 1)
			{
				String s = bp.getString(13);
				Console.Out.WriteLine(i + ":" + s);
				bp.skip(1);
			}
		}

    public static String BytesToString(byte[] bytes)
    {
      char[] c = new char[bytes.Length];
      for(int i = 0; i < c.Length; ++i)
      {
        c[i] = (char)bytes[i];
      }
      return new String(c).Trim();
    }
	}
}