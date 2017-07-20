namespace nom.tam.fits
{
	using System;
	/// <summary>This class allows FITS binary and ASCII tables to be accessed via a common interface.</summary>
	
	public interface TableData
		{
			int NCols
			{
				get;
				
			}
			int NRows
			{
				get;
				
			}
			Array GetRow(int row);
			Object GetColumn(int col);
			Object GetElement(int row, int col);
			void SetRow(int row, Array newRow);
			void SetColumn(int col, Object newCol);
			void SetElement(int row, int col, Object element);
			int AddRow(Array newRow);
			int AddColumn(Object newCol);
		}
}
