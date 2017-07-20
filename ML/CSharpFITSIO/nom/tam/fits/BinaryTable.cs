namespace nom.tam.fits
{
	/* Copyright: Thomas McGlynn 1997-2000.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	*
	* Many thanks to David Glowacki (U. Wisconsin) for substantial
	* improvements, enhancements and bug fixes.
	*/
	using System;
  using System.Collections;
  using System.IO;
	using nom.tam.util;
	/// <summary>This class defines the methods for accessing FITS binary table data.
	/// </summary>
	
	public class BinaryTable:Data, TableData
	{
		/// <summary>Get the number of rows in the table</summary>
		public int NRows
		{
			get
			{
				return nRow;
			}
		}
		/// <summary>Get the number of columns in the table.</summary>
		public int NCols
		{
			get
			{
				return nCol;
			}
		}
		/// <summary>Get the size of the data in the HDU sans padding.</summary>
		override internal int TrueSize
		{
			get
			{
				int len = nRow * rowLen;
				if (heap.Size > 0)
				{
					len += (int)(heap.Size + heapOffset);
				}
				return len;
			}
		}

    override public Object DataArray
		{
			get
			{
				if (table == null)
				{
					
					if (currInput == null)
					{
						throw new FitsException("Cannot find input for deferred read");
					}
					
					table = CreateTable();
					
					long currentOffset = FitsUtil.FindOffset(currInput);
					//FitsUtil.Reposition(currInput, fileOffset);
          currInput.Seek(fileOffset, SeekOrigin.Begin);
					//ReadTrueData(input);
          ReadTrueData(currInput);
					//FitsUtil.Reposition(currInput, currentOffset);
          currInput.Seek(currentOffset, SeekOrigin.Begin);
				}
				
				return table;
			}
		}

    public int[][] Dimens
		{
			get
			{
				return dimens;
			}
		}

    public Type[] Bases
		{
			get
			{
				return table.Bases;
			}
		}

    /*
    public char[] Types
		{
			get
			{
				if (table == null)
				{
					try
					{
						Object generatedAux = DataArray;
					}
					catch (FitsException e)
					{
					}
				}
				return table.Types;
			}
		}
*/

    public Object[] FlatColumns
		{
			get
			{
				if (table == null)
				{
					try
					{
						Object generatedAux = DataArray;
					}
					catch(FitsException)
					{
					}
				}
				return table.Columns;
			}
		}

		public int[] Sizes
		{
			get
			{
				return sizes;
			}
		}

    public int HeapSize
		{
			get
			{
				return (int)(heapOffset + heap.Size);
			}
		}

    public int HeapOffset
		{
			get
			{
				return heapOffset;
			}
		}
		
		/// <summary>This is the area in which variable length column data lives.</summary>
		internal FitsHeap heap;
		
		/// <summary>The number of bytes between the end of the data and the heap</summary>
		internal int heapOffset;
		
		/// <summary>The sizes of each column (in number of entries per row)</summary>
		internal int[] sizes;
		
		/// <summary>The dimensions of each column.</summary>
		internal int[][] dimens;
		
		/// <summary>Info about column</summary>
		internal int[] flags;
		internal const int COL_CONSTANT = 0;
		internal const int COL_VARYING = 1;
		internal const int COL_COMPLEX = 2;
		internal const int COL_STRING = 4;
		internal const int COL_BOOLEAN = 8;
		internal const int COL_BIT = 16;
		
		/// <summary>The number of rows in the table.</summary>
		internal int nRow;
		
		/// <summary>The number of columns in the table.</summary>
		internal int nCol;
		
		/// <summary>The length in bytes of each row.</summary>
		internal int rowLen;
		
		/// <summary>The base classes for the arrays in the table.</summary>
		internal Type[] bases;
		
		/// <summary>An example of the structure of a row</summary>
		internal Object[] modelRow;
		
		/// <summary>A pointer to the data in the columns.  This
		/// variable is only used to assist in the
		/// construction of BinaryTable's that are defined
		/// to point to an array of columns.  It is
		/// not generally filled.  The ColumnTable is used
		/// to store the actual data of the BinaryTable.
		/// </summary>
		internal Object[] columns;
		
		/// <summary>Where the data is actually stored.</summary>
		internal ColumnTable table;
		
		/// <summary>The stream used to input the image</summary>
		internal ArrayDataIO currInput;
		
		/// <summary>Create a null binary table data segment.</summary>
		public BinaryTable()
		{
			try
			{
				table = new ColumnTable(new Object[0], new int[0]);
			}
			catch (TableException e)
			{
				Console.Error.WriteLine("Impossible exception in BinaryTable() constructor" + e);
			}
			heap = new FitsHeap(0);
			ExtendArrays(0);
			nRow = 0;
			nCol = 0;
			rowLen = 0;
		}
		
		/// <summary>Create a binary table from given header information.</summary>
		/// <param name="header	A">header describing what the binary table should look like.</param>
		public BinaryTable(Header myHeader)
		{
			int heapSize = myHeader.GetIntValue("PCOUNT");
			heapOffset = myHeader.GetIntValue("THEAP");
			nRow = myHeader.GetIntValue("NAXIS2");
			int rwsz = myHeader.GetIntValue("NAXIS1");
			
			// Subtract out the size of the regular table from
			// the heap offset.
			
			if (heapOffset > 0)
			{
				heapOffset -= nRow * rwsz;
			}
			
			if (heapOffset > heapSize)
			{
				throw new FitsException("Inconsistent THEAP and PCOUNT");
			}
			
			heap = new FitsHeap(heapSize - heapOffset);
			nCol = myHeader.GetIntValue("TFIELDS");
			rowLen = 0;
			
			ExtendArrays(nCol);
			for (int col = 0; col < nCol; col += 1)
			{
				rowLen += ProcessCol(myHeader, col);
			}
		}
		
		private int ProcessCol(Header header, int col)
		{
			String tform = header.GetStringValue("TFORM" + (col + 1)).Trim();
			String tdims = header.GetStringValue("TDIM" + (col + 1));
			
			if (tform == null)
			{
				throw new FitsException("No TFORM for column:" + col);
			}
			if (tdims != null)
			{
				tdims = tdims.Trim();
			}
			
			char type = GetTformType(tform);
			if (type == 'P')
			{
				flags[col] |= COL_VARYING;
				type = GetTformVarType(tform);
			}

      int size = GetTformLength(tform);
			
			// Get number of bytes for a bit array.
			if (type == 'X')
			{
				size = (size + 7) / 8;
				flags[col] |= COL_BIT;
			}
			else if ((flags[col] & COL_VARYING) != 0)
			{
				size = 2;
			}
			int bSize = size;
			
			int[] dims = null;
			
			// Cannot really handle arbitrary arrays of bits.
			if (tdims != null && type != 'X' && (flags[col] & COL_VARYING) == 0)
			{
				dims = GetTDims(tdims);
			}
			
			if (dims == null)
			{
				dims = new int[]{size};
			}
			
			if (type == 'C' || type == 'M')
			{
				flags[col] |= COL_COMPLEX;
			}
			
			Type colBase = null;
			
			switch (type)
			{
				case 'A': 
					colBase = typeof(byte);
					flags[col] |= COL_STRING;
					bases[col] = typeof(String);
					break;
				case 'L': 
					colBase = typeof(byte);
					bases[col] = typeof(bool);
					flags[col] |= COL_BOOLEAN;
					break;
				case 'X': 
				case 'B':
          colBase = typeof(byte);
					bases[col] = typeof(byte);
					break;
				case 'I':
          colBase = typeof(short);
					bases[col] = typeof(short);
					bSize *= 2;
					break;
				case 'J':
          colBase = typeof(int);
					bases[col] = typeof(int);
					bSize *= 4;
					break;
				case 'K':
          colBase = typeof(long);
					bases[col] = typeof(long);
					bSize *= 8;
					break;
				case 'E': 
				case 'C':
          colBase = typeof(float);
					bases[col] = typeof(float);
					bSize *= 4;
					break;
				case 'D': 
				case 'M':
          colBase = typeof(double);
					bases[col] = typeof(double);
					bSize *= 8;
					break;
				default:
          throw new FitsException("Invalid type in column:" + col);
			}
			
			if ((flags[col] & COL_VARYING) != 0)
			{
				dims = new int[]{nRow, 2};
				colBase = typeof(int);
				bSize = 8;
			}
			else if ((flags[col] & COL_COMPLEX) != 0)
			{
				int[] xdims = new int[dims.Length + 1];
				Array.Copy(dims, 0, xdims, 0, dims.Length);
				xdims[dims.Length] = 2;
				dims = xdims;
			}
			
			modelRow[col] = ArrayFuncs.NewInstance(colBase, dims);
			dimens[col] = dims;
			sizes[col] = size;
			
			return bSize;
		}
		
		/// <summary>Get the type in the TFORM field</summary>
		private char GetTformType(String tform)
		{
			for (int i = 0; i < tform.Length; i += 1)
			{
				if (!Char.IsDigit(tform[i]))
				{
					return tform[i];
				}
			}

			return Char.MinValue;
		}
		
		/// <summary>Get the type in a varying length column TFORM</summary>
		private char GetTformVarType(String tform)
		{
			int ind = tform.IndexOf("P");
			if (tform.Length > ind + 1)
			{
				return tform[ind + 1];
			}
			else
			{
				return Char.MinValue;
			}
		}
		
		/// <summary>Get the explicit or implied length of the TFORM field</summary>
		private int GetTformLength(String tform)
		{
			if(Char.IsDigit(tform[0]))
			{
				return InitialNumber(tform);
			}
			else
			{
				String xform = tform.Substring(1).Trim();
				if (xform.Length == 0)
				{
					return 1;
				}
				if (Char.IsDigit(xform[0]))
				{
					return InitialNumber(xform);
				}
			}
			return 1;
		}
		
		/// <summary>Get an unsigned number at the beginning of a string</summary>
		private int InitialNumber(String tform)
		{
			int i;
			for (i = 0; i < tform.Length; i += 1)
			{
				if (!Char.IsDigit(tform[i]))
				{
					break;
				}
			}
			
			return Int32.Parse(tform.Substring(0, (i) - (0)));
		}
		
		
		/// <summary>Parse the TDIMS value.
		/// *
		/// If the TDIMS value cannot be deciphered a one-d
		/// array with the size given in arrsiz is returned.
		/// *
		/// </summary>
		/// <param name="tdims">The value of the TDIMSn card.</param>
		/// <param name="arraySize">The size field found on the TFORMn card.</param>
		/// <returns>An int array of the desired dimensions.
		/// Note that the order of the tdims is the inverse
		/// of the order in the TDIMS key.</returns>
		public static int[] GetTDims(String tdims)
		{
			// The TDIMs value should be of the form: "(iiii,jjjj,kkk,...)"
			
			int[] dims = null;
			
			int first = tdims.IndexOf('(');
			int last = tdims.LastIndexOf(')');
			if (first >= 0 && last > 0 && first < last)
			{
				tdims = tdims.Substring(first + 1, (last - first) - (first + 1));
				
				SupportClass.Tokenizer st = new SupportClass.Tokenizer(tdims, ",");
				int dim = st.Count;
				if (dim > 0)
				{
					dims = new int[dim];
					
					for (int i = dim - 1; i >= 0; i -= 1)
					{
						dims[i] = Int32.Parse(st.NextToken().Trim());
					}
				}
			}
			return dims;
		}
		
		
		internal override void FillHeader(Header h)
		{
			try
			{
				h.Xtension = "BINTABLE";
				h.Bitpix = 8;
				h.Naxes = 2;
				h.SetNaxis(1, rowLen);
				h.SetNaxis(2, nRow);
				h.AddValue("PCOUNT", heap.Size, null);
				h.AddValue("GCOUNT", 1, null);
				Cursor c = h.GetCursor();
				c.Key = "GCOUNT";
				c.MoveNext();
        c.Add("TFIELDS", new HeaderCard("TFIELDS", modelRow.Length, null));
        c.Add("THEAP", new HeaderCard("THEAP", 0, null));
        
        for(int i = 0; i < modelRow.Length; i += 1)
				{
					if(i > 0)
					{
						h.PositionAfterIndex("TFORM", i);
					}
					FillForColumn(h, i, c);
				}
			}
			catch(HeaderCardException)
			{
				Console.Error.WriteLine("Impossible exception");
			}
		}

		internal void PointToColumn(int col, Header hdr)
		{
			Cursor c = hdr.GetCursor();
			if(col > 0)
			{
				hdr.PositionAfterIndex("TFORM", col);
			}
			FillForColumn(hdr, col, c);
		}
		
		internal void FillForColumn(Header h, int col, Cursor cursor)
		{
			String tform;
			if ((flags[col] & COL_VARYING) != 0)
			{
				tform = "1P";
			}
			else
			{
				tform = "" + sizes[col];
			}
      if (bases[col] == typeof(int))
      {
        tform += "J";
      }
        //else if (bases[col] == typeof(short) || bases[col] == typeof(char))
      else if (bases[col] == typeof(short))// || bases[col] == typeof(char))
      {
        tform += "I";
      }
      else if (bases[col] == typeof(byte))
      {
        tform += "B";
      }
      else if (bases[col] == typeof(float))
      {
        tform += "E";
      }
      else if (bases[col] == typeof(double))
      {
        tform += "D";
      }
      else if (bases[col] == typeof(long))
      {
        tform += "K";
      }
      else if (bases[col] == typeof(bool))
      {
        tform += "L";
      }
      else if (bases[col] == typeof(char) || bases[col] == typeof(String))
      {
        tform += "A";
      }
      else
      {
        throw new FitsException("Invalid column data class:" + bases[col]);
      }
			
			
			String key = "TFORM" + (col + 1);
			cursor.Add(key, new HeaderCard(key, tform, null));
			
			if (dimens[col].Length > 0 && ((flags[col] & COL_VARYING) == 0))
			{
				System.Text.StringBuilder tdim = new System.Text.StringBuilder();
				char comma = '(';
				for (int i = dimens[col].Length - 1; i >= 0; i -= 1)
				{
					tdim.Append(comma);
					tdim.Append(dimens[col][i]);
					comma = ',';
				}
				tdim.Append(')');
				key = "TDIM" + (col + 1);
				cursor.Add(key, new HeaderCard(key, new String(tdim.ToString().ToCharArray()), null));
			}
		}
		
		/// <summary>Create a column table given the number of
		/// rows and a model row.  This is used when
		/// we defer instantiation of the ColumnTable until
		/// the user requests data from the table.
		/// </summary>
		private ColumnTable CreateTable()
		{
			int nfields = modelRow.Length;
			
			Object[] arrCol = new Object[nfields];
			
			for (int i = 0; i < nfields; i += 1)
			{
				arrCol[i] = ArrayFuncs.NewInstance(ArrayFuncs.GetBaseClass(modelRow[i]), sizes[i] * nRow);
			}
			
			ColumnTable table;
			
			try
			{
				table = new ColumnTable(arrCol, sizes);
			}
			catch (TableException e)
			{
				throw new FitsException("Unable to create table:" + e);
			}
			
			return table;
		}
		
		/// <summary>Create a binary table from existing data in row order.</summary>
		/// <param name="data">The data used to initialize the binary table.</param>
		public BinaryTable(Object[][] data):this(ConvertToColumns(data))
		{
		}
		
		/// <summary>Convert a two-d table to a table of columns.  Handle
		/// String specially.  Every other element of data should be
		/// a primitive array of some dimensionality.</summary>
		private static Object[] ConvertToColumns(Object[][] data)
		{
      if(data == null)
      {
        return new Object[0];
      }

			Object[] row = data[0];
			int nrow = data.Length;
			
			Object[] results = new Object[row.Length];
			
			for (int col = 0; col < row.Length; col += 1)
			{
				if (row[col] is String)
				{
					String[] sa = new String[nrow];
					for (int irow = 0; irow < nrow; irow += 1)
					{
						sa[irow] = (String) data[irow][col];
					}
					
					results[col] = sa;
				}
				else
				{
					Type base_Renamed = ArrayFuncs.GetBaseClass(row[col]);
					int[] dims = ArrayFuncs.GetDimensions(row[col]);
					
					if(dims != null && (dims.Length > 1 || (dims.Length == 1 && dims[0] > 1)))
					{
						int[] xdims = new int[dims.Length + 1];
						xdims[0] = nrow;
						
						Object[] arr = (Object[])ArrayFuncs.NewInstance(base_Renamed, xdims);
						for (int irow = 0; irow < nrow; irow += 1)
						{
							arr[irow] = data[irow][col];
						}
						results[col] = arr;
					}
					else if(base_Renamed != null)
					{
						Array arr = ArrayFuncs.NewInstance(base_Renamed, nrow);
            for(int irow = 0; irow < nrow; irow += 1)
            {
              if(data[irow][col] != null && arr != null)
              {
                Array.Copy((Array)data[irow][col], 0, arr, irow, 1);
              }
            }
            results[col] = arr;
					}
				}
			}
			return results;
		}
		
		/// <summary>Create a binary table from existing data in column order.</summary>
		public BinaryTable(Object[] o)
		{
			heap = new FitsHeap(0);
			modelRow = new Object[o.Length];
			ExtendArrays(o.Length);
			
			for (int i = 0; i < o.Length; i += 1)
			{
				AddColumn(o[i]);
			}
		}
		
		/// <summary>Create a binary table from an existing ColumnTable</summary>
		public BinaryTable(ColumnTable tab)
		{
			nCol = tab.NCols;
			
			ExtendArrays(nCol);
			
			bases = tab.Bases;
			sizes = tab.Sizes;
			
			modelRow = new Object[nCol];
			
			dimens = new int[nCol][];
			for (int i = 0; i < nCol; i++)
			{
				dimens[i] = new int[1];
			}
			
			// Set all flags to 0.
			flags = new int[nCol];
			
			for (int col = 0; col < nCol; col += 1)
			{
				dimens[col][0] = sizes[col];
			}
			
			for (int col = 0; col < nCol; col += 1)
			{
				modelRow[col] = ArrayFuncs.NewInstance(bases[col], sizes[col]);
			}
			
			columns = null;
			table = tab;
		}
		
		
		/// <summary>Get a given row</summary>
		/// <param name="row">The index of the row to be returned.</param>
		/// <returns>A row of data.</returns>
		public Array GetRow(int row)
		{
			if (!ValidRow(row))
			{
				throw new FitsException("Invalid row");
			}
			
			Array res;
			if (table != null)
			{
				res = GetMemoryRow(row);
			}
			else
			{
				res = GetFileRow(row);
			}
			return res;
		}
		
		/// <summary>Get a row from memory.</summary>
		private Array GetMemoryRow(int row)
		{
			Object[] data = new Object[modelRow.Length];
			for (int col = 0; col < modelRow.Length; col += 1)
			{
				Object o = table.GetElement(row, col);
				o = ColumnToArray(col, o);
				data[col] = Encurl(o, col, 1);
				if (data[col] is Object[])
				{
					data[col] = ((Object[]) data[col])[0];
				}
			}
			
			return data;
		}
		
		/// <summary>Get a row from the file.</summary>
		private Array GetFileRow(int row)
		{
			Object[] data = new Object[nCol];
			for (int col = 0; col < data.Length; col += 1)
			{
				data[col] = ArrayFuncs.NewInstance(ArrayFuncs.GetBaseClass(modelRow[col]), sizes[col]);
			}
			
			try
			{
        currInput.Seek(fileOffset + row * rowLen, SeekOrigin.Begin);
				currInput.ReadArray(data);
			}
			catch(IOException)
			{
				throw new FitsException("Error in deferred row read");
			}
			for (int col = 0; col < data.Length; col += 1)
			{
				data[col] = ColumnToArray(col, data[col]);
				data[col] = Encurl(data[col], col, 1);
				if(data[col] is Array)
				{
					data[col] = ((Array)data[col]).GetValue(0);
				}
			}
			return data;
		}
		
		
		/// <summary>Replace a row in the table.</summary>
		/// <param name="row"> The index of the row to be replaced.</param>
		/// <param name="data">The new values for the row.</param>
		/// <exception cref=""> FitsException Thrown if the new row cannot
		/// match the existing data.</exception>
		public void SetRow(int row, Array data)
		{
			if(table == null)
			{
				Object generatedAux = DataArray;
			}
			
			if (data.Length != NCols)
			{
				throw new FitsException("Updated row size does not agree with table");
			}
			
			Object[] ydata = new Object[data.Length];
			
			for (int col = 0; col < data.Length; col += 1)
			{
				Object o = ArrayFuncs.Flatten(data.GetValue(col));
				ydata[col] = ArrayToColumn(col, o);
			}
			
			try
			{
				table.SetRow(row, ydata);
			}
			catch(TableException e)
			{
				throw new FitsException("Error modifying table: " + e);
			}
		}
		
		/// <summary>Replace a column in the table.</summary>
		/// <param name="col">The index of the column to be replaced.</param>
		/// <param name="xcol">The new data for the column</param>
		/// <exception cref=""> FitsException Thrown if the data does not match
		/// the current column description.</exception>
		public void SetColumn(int col, Object xcol)
		{
			xcol = ArrayFuncs.Flatten(xcol);
			xcol = ArrayToColumn(col, xcol);
			SetFlattenedColumn(col, xcol);
		}
		
		/// <summary>Set a column with the data aleady flattened.</summary>
		/// <param name="col"> The index of the column to be replaced.</param>
		/// <param name="data">The new data array.  This should be a one-d
		/// primitive array.</param>
		/// <exception cref=""> FitsException Thrown if the type of length of
		/// the replacement data differs from the original.</exception>
		public void SetFlattenedColumn(int col, Object data)
		{
			if (table == null)
			{
				Object generatedAux = DataArray;
			}
			
			data = ArrayToColumn(col, data);
			
			Object oldCol = table.GetColumn(col);
			if (data.GetType() != oldCol.GetType() || ((Array) data).Length != ((Array) oldCol).Length)
			{
				throw new FitsException("Replacement column mismatch at column:" + col);
			}
			try
			{
				table.SetColumn(col, data);
			}
			catch (TableException e)
			{
				throw new FitsException("Unable to set column:" + col + " error:" + e);
			}
		}
		
		/// <summary>Get a given column</summary>
		/// <param name="col">The index of the column.</param>
		public  Object GetColumn(int col)
		{
			if (table == null)
			{
				Object generatedAux = DataArray;
			}
			
			Object res = GetFlattenedColumn(col);
			return Encurl(res, col, nRow);
		}
		
		private Object Encurl(Object res, int col, int rows)
		{
			if (bases[col] != typeof(String))
			{
				if (((flags[col] & COL_VARYING) == 0) && (dimens[col].Length > 1 || dimens[col][0] != 1))
				{
					
					int[] dims = new int[dimens[col].Length + 1];
					Array.Copy(dimens[col], 0, dims, 1, dimens[col].Length);
					dims[0] = rows;
					res = ArrayFuncs.Curl((Array)res, dims);
				}
			}
			else
			{
				// Handle Strings.  Remember the last element
				// in dimens is the length of the Strings and
				// we already used that when we converted from
				// byte arrays to strings.  So we need to ignore
				// the last element of dimens, and add the row count
				// at the beginning to curl.
				if (dimens[col].Length > 2)
				{
					int[] dims = new int[dimens[col].Length];
					
					Array.Copy(dimens[col], 0, dims, 1, dimens[col].Length - 1);
					dims[0] = rows;
					res = ArrayFuncs.Curl((Array)res, dims);
				}
			}
			
			return res;
		}
		
		/// <summary>Get a column in flattened format.
		/// For large tables getting a column in standard format can be
		/// inefficient because a separate object is needed for
		/// each row.  Leaving the data in flattened format means
		/// that only a single object is created.</summary>
		/// <param name="">col</param>
		public  Object GetFlattenedColumn(int col)
		{
			if (table == null)
			{
				Object generatedAux = DataArray;
			}
			
			if (!ValidColumn(col))
			{
				throw new FitsException("Invalid column");
			}
			
			return ColumnToArray(col, table.GetColumn(col));
		}
		
		/// <summary>Get a particular element from the table.</summary>
		/// <param name="i">The row of the element.</param>
		/// <param name="j">The column of the element.</param>
		public Object GetElement(int i, int j)
		{
			if (!ValidRow(i) || !ValidColumn(j))
			{
				throw new FitsException("No such element");
			}
			
			Object ele;
			if (table == null)
			{
				// This is really inefficient.
				// Need to either save the row, or just read the one element.
				Array row = GetRow(i);
				ele = row.GetValue(j);
			}
			else
			{
				ele = table.GetElement(i, j);
				ele = ColumnToArray(j, ele);
				ele = Encurl(ele, j, 1);
				if (ele is Object[])
				{
					ele = ((Object[]) ele)[0];
				}
			}
			
			return ele;
		}
		
		/// <summary>Add a row at the end of the table.</summary>
		/// <param name="o">An array of objects instantiating the data.  These
		/// should have the same structure as any existing rows.</param>
		public int AddRow(Array o)
		{
			if (table == null)
			{
				Object generatedAux = DataArray;
			}
			
			if (nCol == 0 && nRow == 0)
			{
				for (int i = 0; i < o.Length; i += 1)
				{
					AddColumn(o);
				}
			}
			else
			{
				Object[] flatRow = new Object[NCols];
				for (int i = 0; i < NCols; i += 1)
				{
					Object x = ArrayFuncs.Flatten(o.GetValue(i));
					flatRow[i] = ArrayToColumn(i, x);
				}
				try
				{
					table.AddRow(flatRow);
				}
				catch(TableException)
				{
					throw new FitsException("Error add row to table");
				}
				
				nRow += 1;
			}
			
			return nRow;
		}
		
		/// <summary>Add a column to the end of a table.</summary>
		/// <param name="o">An array of identically structured objects with the
		/// same number of elements as other columns in the table.</param>
		public int AddColumn(Object o)
		{
			ExtendArrays(nCol + 1);
			Type base_Renamed = ArrayFuncs.GetBaseClass(o);
			
			// A varying length column is a two-d primitive
			// array where the second index is not constant.
			
			if(IsVarying(o))
			{
				flags[nCol] |= COL_VARYING;
				dimens[nCol] = new int[]{2};
			}

      // Flatten out everything but 1-D arrays and the
			// two-D arrays associated with variable length columns.
			
			if ((flags[nCol] & COL_VARYING) == 0)
			{
				int[] allDim = ArrayFuncs.GetDimensions(o);
				
				// Add a dimension for the length of Strings.
				if (base_Renamed == typeof(String))
				{
					int[] xdim = new int[allDim.Length + 1];
					Array.Copy(allDim, 0, xdim, 0, allDim.Length);
					xdim[allDim.Length] = - 1;
					allDim = xdim;
				}
				
				if (allDim.Length == 1)
				{
					dimens[nCol] = new int[]{1};
				}
				else
				{
					dimens[nCol] = new int[allDim.Length - 1];
					Array.Copy(allDim, 1, dimens[nCol], 0, allDim.Length - 1);
					o = ArrayFuncs.Flatten(o);
				}
			}
			
			AddFlattenedColumn(o, dimens[nCol]);
			return NCols;
		}
		
		/// <summary>Is this a variable length column?
		/// It is if it's a two-d primitive array and
		/// the second dimension is not constant.</summary>
		private bool IsVarying(Object o)
		{
			String classname = o.GetType().FullName;
			
			if (classname.Length != 3 || classname[0] != '[' || classname[1] != '[')
			{
				return false;
			}
			
			Object[] ox = (Object[]) o;
			if (ox.Length < 2)
			{
				return false;
			}
			
			int flen = ((Array) ox[0]).Length;
			for (int i = 1; i < ox.Length; i += 1)
			{
				if (((Array) ox[i]).Length != flen)
				{
					return true;
				}
			}
			return false;
		}
		
		/// <summary>Add a column where the data is already flattened.</summary>
		/// <param name="o">The new column data.  This should be a one-dimensional primitive array.</param>
		/// <param name="dimens">The dimensions of one row of the column./param>
		public int AddFlattenedColumn(Object o, int[] dims)
		{
			ExtendArrays(nCol + 1);
			
			bases[nCol] = ArrayFuncs.GetBaseClass(o);
			
			if (bases[nCol] == typeof(bool))
			{
				flags[nCol] |= COL_BOOLEAN;
			}
			else if (bases[nCol] == typeof(String))
			{
				flags[nCol] |= COL_STRING;
			}
			
			// Convert to column first in case
			// this is a String array.  This sets
			// the size of the column.
			
			o = ArrayToColumn(nCol, o);
			
			int size = 1;
			
			for (int dim = 0; dim < dims.Length; dim += 1)
			{
				size *= dims[dim];
			}
			sizes[nCol] = size;
			
			
			int xRow = ((Array) o).Length / size;
			if (NCols == 0)
			{
				nRow = xRow;
			}
			else if (xRow > 0)
			{
				if (xRow != nRow)
				{
					throw new FitsException("Added column does not have correct row count");
				}
			}
			
			if ((flags[nCol] & COL_VARYING) == 0)
			{
				modelRow[nCol] = ArrayFuncs.NewInstance(ArrayFuncs.GetBaseClass(o), dims);
				rowLen += size * ArrayFuncs.GetBaseLength(o);
			}
			else
			{
				modelRow[nCol] = new int[2];
				rowLen += 8;
			}
			
			// Only add to table if table already exists or if we
			// are filling up the last element in columns.
			// This way if we allocate a bunch of columns at the beginning
			// we only create the column table after we have all the columns
			// ready.
			
			columns[nCol] = o;
			
			try
			{
				if (table != null)
				{
					table.AddColumn((Array)o, sizes[nCol]);
				}
				else if (nCol == columns.Length - 1)
				{
					table = new ColumnTable(columns, sizes);
				}
			}
			catch (TableException e)
			{
				throw new FitsException("Error in ColumnTable:" + e);
			}
			
			nCol += 1;
			return nCol;
		}
		
		/// <summary>Check to see if this is a valid row.</summary>
		/// <param name="i">The Java index (first=0) of the row to check.</param>
		protected internal bool ValidRow(int i)
		{
			if (NRows > 0 && i >= 0 && i < NRows)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		/// <summary>Check if the column number is valid.</summary>
		/// <param name="j">The Java index (first=0) of the column to check.</param>
		protected internal bool ValidColumn(int j)
		{
			return (j >= 0 && j < NCols);
		}
		
		/// <summary>Replace a single element within the table.</summary>
		/// <param name="i">The row of the data.</param>
		/// <param name="j">The column of the data.</param>
		/// <param name="o">The replacement data.</param>
		public void SetElement(int i, int j, Object o)
		{
			try
			{
				table.SetElement(i, j, ArrayFuncs.Flatten(o));
			}
			catch (TableException e)
			{
				throw new FitsException("Error modifying table:" + e);
			}
		}
		
		/// <summary>Read the data -- or defer reading on random access</summary>
		public override void Read(ArrayDataIO i)
		{
			SetFileOffset(i);
			currInput = i;
			
			if (i is RandomAccess)
			{
				try
				{
					//BinaryReader temp_BinaryReader;
					Int64 temp_Int64;
					//temp_BinaryReader = i;
					temp_Int64 = i.Position;//temp_BinaryReader.BaseStream.Position;
					temp_Int64 = i.Seek(TrueSize, SeekOrigin.Current) - temp_Int64; //temp_BinaryReader.BaseStream.Seek(TrueSize, IO.SeekOrigin.Current) - temp_Int64;
					int generatedAux = (int)temp_Int64;
					//IO.BinaryReader temp_BinaryReader2;
					Int64 temp_Int65;
					//temp_BinaryReader2 = i;
					temp_Int65 = i.Position;//temp_BinaryReader2.BaseStream.Position;
					temp_Int65 = i.Seek(FitsUtil.Padding(TrueSize), SeekOrigin.Current) - temp_Int65;//temp_BinaryReader2.BaseStream.Seek(FitsUtil.padding(TrueSize), IO.SeekOrigin.Current) - temp_Int65;
					int generatedAux2 = (int)temp_Int65;
				}
				catch(IOException e)
				{
					throw new FitsException("Unable to skip binary table HDU:" + e);
				}
			}
			else
			{
				/// <summary>Read the data associated with the HDU including the hash area if present.</summary>
				/// <param name="i">The input stream</param>
				if (table == null)
				{
					table = CreateTable();
				}
				
				ReadTrueData(i);
			}
		}
		
		/// <summary>Read table, heap and padding</summary>
		protected internal  void ReadTrueData(ArrayDataIO i)
		{
			int len;
			
			try
			{
				len = table.Read(i);
//				IO.BinaryReader temp_BinaryReader;
				Int64 temp_Int64;
				//temp_BinaryReader = i;
				temp_Int64 = i.Position; //temp_BinaryReader.BaseStream.Position;
				temp_Int64 = i.Seek(heapOffset, SeekOrigin.Current) - temp_Int64; //temp_BinaryReader.BaseStream.Seek(heapOffset, IO.SeekOrigin.Current) - temp_Int64;
				int generatedAux = (int)temp_Int64;
				heap.Read(i);
				//IO.BinaryReader temp_BinaryReader2;
				Int64 temp_Int65;
				//temp_BinaryReader2 = i;
				temp_Int65 = i.Position; //temp_BinaryReader2.BaseStream.Position;
				temp_Int65 = i.Seek(FitsUtil.Padding(TrueSize), SeekOrigin.Current) - temp_Int65;  //temp_BinaryReader2.BaseStream.Seek(FitsUtil.padding(TrueSize), IO.SeekOrigin.Current) - temp_Int65;
				int generatedAux2 = (int)temp_Int65;
			}
			catch(IOException e)
			{
				throw new FitsException("Error reading binary table data:" + e);
			}
		}

    /// <summary>Write the table, heap and padding</summary>
		public override void Write(ArrayDataIO os)
		{
			if(table == null)
			{
				long currentOffset = FitsUtil.FindOffset(os);
				Object generatedAux = DataArray;
				//FitsUtil.Reposition(os, currentOffset);
        os.Seek(currentOffset, SeekOrigin.Begin);
			}
			
			int len;
			try
			{
				// First write the table.
				len = table.Write(os);
				if (heapOffset > 0)
				{
          os.Write(new byte[heapOffset]);
				}
				if (heap.Size > 0)
				{
					heap.Write(os);
				}
				
        os.Write(new byte[FitsUtil.Padding(TrueSize)]);
			}
			catch(IOException e)
			{
				throw new FitsException("Unable to write table:" + e);
			}
		}

    /// <summary>Convert the external representation to the
		/// BinaryTable representation.  Transformation include
		/// boolean -> T/F, Strings -> byte arrays,
		/// variable length arrays -> pointers (after writing data
		/// to heap).
		/// </summary>
		private Object ArrayToColumn(int col, Object o)
		{
			if (flags[col] == 0)
			{
				return o;
			}
			
			if ((flags[col] & COL_VARYING) == 0)
			{
				if ((flags[col] & COL_STRING) != 0)
				{
					// Convert strings to array of bytes.
					int[] dims = dimens[col];
					
					// Set the length of the string if we are just adding the column.
					if (dims[dims.Length - 1] < 0)
					{
						dims[dims.Length - 1] = FitsUtil.MaxLength((String[]) o);
					}
					if (o is String)
					{
						o = new String[]{(String) o};
					}
					o = FitsUtil.StringsToByteArray((String[]) o, dims[dims.Length - 1]);
				}
				else if ((flags[col] & COL_BOOLEAN) != 0)
				{
					// Convert true/false to 'T'/'F'
					o = FitsUtil.BooleanToByte((bool[]) o);
				}
			}
			else
			{
				if((flags[col] & COL_BOOLEAN) != 0)
				{
					// Handle addRow/addElement
					if(o is bool[])
					{
						o = new bool[][]{(bool[]) o};
					}
					
					// Convert boolean to byte arrays
					bool[][] to = (bool[][]) o;
					byte[][] xo = new byte[to.Length][];
					for (int i = 0; i < to.Length; i += 1)
					{
						xo[i] = FitsUtil.BooleanToByte(to[i]);
					}
					o = xo;
				}
				
				// Write all rows of data onto the heap.
				int offset = heap.PutData(o);
				
				int blen = ArrayFuncs.GetBaseLength(o);
				
				// Handle an addRow of a variable length element.
				// -- We only get a one-d array, but the following
				//    lets us use the same code as when we add in a column!
				if (!(o is Object[]))
				{
					o = new Object[]{o};
				}
				
				// Create the array descriptors
				int nrow = ((Array) o).Length;
				int[] descrip = new int[2 * nrow];
				
				Object[] x = (Object[]) o;
				
				// Fill the descriptor for each row.
				for (int i = 0; i < nrow; i += 1)
				{
					descrip[2 * i] = ((Array) x[i]).Length;
					descrip[2 * i + 1] = offset;
					offset += descrip[2 * i] * blen;
					if ((flags[col] & COL_COMPLEX) != 0)
					{
						// We count each pair in a complex number as
						// a single element.
						descrip[2 * i] /= 2;
					}
				}
				o = descrip;
			}
			
			return o;
		}
		
		/// <summary>Convert data from binary table representation to external Java representation.</summary>
		private Object ColumnToArray(int col, Object o)
		{
			// Most of the time we need do nothing!
			if (flags[col] == 0)
			{
				return o;
			}
			
			// If a varying length column use the descriptors to
			// extract appropriate information from the headers.
			
			if ((flags[col] & COL_VARYING) != 0)
			{
				int[] descrip = (int[]) o;
				int nrow = descrip.Length / 2;
				
				Object[] res; // Res will be the result of extracting from the heap.
				int[] dims; // Used to create result arrays.
				
				
				if ((flags[col] & COL_COMPLEX) != 0)
				{
					// Complex columns have an extra dimension for each row
					dims = new int[]{nrow, 0, 0};
					res = (Object[]) ArrayFuncs.NewInstance(bases[col], dims);
					// Set up dims for individual rows.
					dims = new int[2];
					dims[1] = 2;
				}
				else
				{
					// Non-complex data has a simple primitive array for each row
					dims = new int[]{nrow, 0};
					res = (Object[]) ArrayFuncs.NewInstance(bases[col], dims);
				}
				
				// Now read in each requested row.
				for (int i = 0; i < nrow; i += 1)
				{
					Object row;
					int offset = descrip[2 * i + 1];
					int dim = descrip[2 * i];
					
					if ((flags[col] & COL_COMPLEX) != 0)
					{
						dims[0] = dim;
						row = ArrayFuncs.NewInstance(bases[col], dims);
					}
					else if ((flags[col] & COL_BOOLEAN) != 0)
					{
						// For boolean data, we need to read bytes and convert
						// to booleans.
						row = ArrayFuncs.NewInstance(typeof(byte), dim);
					}
					else
					{
						
						row = ArrayFuncs.NewInstance(bases[col], dim);
					}
					
					heap.GetData(offset, row);
					
					// Now do the boolean conversion.
					if ((flags[col] & COL_BOOLEAN) != 0)
					{
						row = FitsUtil.ByteToBoolean((byte[]) row);
					}
					
					res[i] = row;
				}
				o = res;
			}
			else
			{
				// Fixed length columns
				
				// Need to convert String byte arrays to appropriate Strings.
				if ((flags[col] & COL_STRING) != 0)
				{
					int[] dims = dimens[col];
					o = FitsUtil.ByteArrayToStrings((byte[]) o, dims[dims.Length - 1]);
				}
				else if ((flags[col] & COL_BOOLEAN) != 0)
				{
					o = FitsUtil.ByteToBoolean((byte[]) o);
				}
			}
			
			return o;
		}
		
		/// <summary>Make sure the arrays which describe the columns are
		/// long enough, and if not extend them.</summary>
		private void ExtendArrays(int need)
		{
			bool wasNull = false;
			if (sizes == null)
			{
				wasNull = true;
			}
			else if (sizes.Length > need)
			{
				return ;
			}
			
			// Allocate the arrays.
			int[] newSizes = new int[need];
			int[][] newDimens = new int[need][];
			int[] newFlags = new int[need];
			Object[] newModel = new Object[need];
			Object[] newColumns = new Object[need];
			Type[] newBases = new Type[need];
			
			if (!wasNull)
			{
				int len = sizes.Length;
				Array.Copy(sizes, 0, newSizes, 0, len);
				Array.Copy(dimens, 0, newDimens, 0, len);
				Array.Copy(flags, 0, newFlags, 0, len);
				Array.Copy(modelRow, 0, newModel, 0, len);
				Array.Copy(columns, 0, newColumns, 0, len);
				Array.Copy(bases, 0, newBases, 0, len);
			}
			
			sizes = newSizes;
			dimens = newDimens;
			flags = newFlags;
			modelRow = newModel;
			columns = newColumns;
			bases = newBases;
		}
	}
}
