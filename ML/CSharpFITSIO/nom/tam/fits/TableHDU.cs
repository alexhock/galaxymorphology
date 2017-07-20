namespace nom.tam.fits
{
	using System;
	
	
	/// <summary>This class allows FITS binary and ASCII tables to
	/// be accessed via a common interface.
	/// 
	/// Bug Fix: 3/28/01 to findColumn.
	/// </summary>
	
	public abstract class TableHDU:BasicHDU
	{
		/// <summary>Get the number of columns for this table</summary>
		/// <returns> The number of columns in the table.</returns>
		virtual public int NCols
		{
			get
			{
				return table.NCols;
			}
			
		}
		/// <summary>Get the number of rows for this table</summary>
		/// <returns> The number of rows in the table.</returns>
		virtual public int NRows
		{
			get
			{
				return table.NRows;
			}
			
		}
		virtual public int CurrentColumn
		{
			set
			{
				myHeader.PositionAfterIndex("TFORM", (value + 1));
			}
			
		}
		
		private TableData table;
		//private int currentColumn;
		
		
		internal TableHDU(TableData td)
		{
			table = td;
		}
		
		public virtual Array GetRow(int row)
		{
			return table.GetRow(row);
		}
		
		public virtual Object GetColumn(String colName)
		{
			return GetColumn(FindColumn(colName));
		}
		
		public virtual Object GetColumn(int col)
		{
			return table.GetColumn(col);
		}
		
		public virtual Object GetElement(int row, int col)
		{
			return table.GetElement(row, col);
		}
		
		public virtual void SetRow(int row, Array newRow)
		{
			table.SetRow(row, newRow);
		}
		
		public virtual void SetColumn(String colName, Object newCol)
		{
			SetColumn(FindColumn(colName), newCol);
		}
		
		public virtual void SetColumn(int col, Object newCol)
		{
			table.SetColumn(col, newCol);
		}
		
		public virtual void SetElement(int row, int col, Object element)
		{
			table.SetElement(row, col, element);
		}
		
		public virtual int AddRow(Array newRow)
		{
			int row = table.AddRow(newRow);
			myHeader.AddValue("NAXIS2", row, null);
			return row;
		}
		
		public virtual int FindColumn(String colName)
		{
			for (int i = 0; i < NCols; i += 1)
			{
				String val = myHeader.GetStringValue("TTYPE" + (i + 1));
				if (val != null && val.Trim().Equals(colName))
				{
					return i;
				}
			}
			return - 1;
		}
		
		public abstract int AddColumn(Object data);

    /// <summary>Get the name of a column in the table.</summary>
		/// <param name="index">The 0-based column index.</param>
		/// <returns> The column name.</returns>
		/// <exception cref=""> FitsException if an invalid index was requested.</exception>
		public virtual String GetColumnName(int index)
		{
			String ttype = myHeader.GetStringValue("TTYPE" + (index + 1));
			if (ttype != null)
			{
				ttype = ttype.Trim();
			}
			return ttype;
		}
		
		public virtual void SetColumnName(int index, String name, String comment)
		{
			if (NCols > index && index >= 0)
			{
				myHeader.PositionAfterIndex("TFORM", index + 1);
				myHeader.AddValue("TTYPE" + (index + 1), name, comment);
			}
		}
		
		/// <summary>Get the FITS type of a column in the table.</summary>
		/// <returns> The FITS type.</returns>
		/// <exception cref=""> FitsException if an invalid index was requested.</exception>
		public virtual String GetColumnFormat(int index)
		{
			int flds = myHeader.GetIntValue("TFIELDS", 0);
			if (index < 0 || index >= flds)
			{
				throw new FitsException("Bad column index " + index + " (only " + flds + " columns)");
			}
			
			return myHeader.GetStringValue("TFORM" + (index + 1)).Trim();
		}
	}
}
