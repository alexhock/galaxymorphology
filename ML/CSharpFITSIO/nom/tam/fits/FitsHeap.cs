namespace nom.tam.fits
{
	using System;
  using System.IO;
	using nom.tam.util;
	/// <summary>This class supports the FITS heap.  This
	/// is currently used for variable length columns
	/// in binary tables.</summary>
	
	public class FitsHeap : FitsElement
	{
    #region Properties
    public virtual bool Rewriteable
    {
      get
      {
        return !(fileOffset < 0);
      }
    }
		
    /// <summary>Get the current offset within the heap.</summary>
		virtual public int Offset
		{
			get
			{
				return heapOffset;
			}
			
		}

    virtual public long Size
		{
			get
			{
				return GetSize();
			}
		}

    virtual public long FileOffset
		{
			get
			{
				return fileOffset;
			}
		}
    #endregion

    #region Instance Variables
    /// <summary>The storage buffer</summary>
		private byte[] heap;
		
		/// <summary>The current used size of the buffer <= heap.length</summary>
		private int heapSize;
		
		/// <summary>The offset within a file where the heap begins</summary>
		private long fileOffset = - 1;
		
		/// <summary>The stream the last read used</summary>
		private ArrayDataIO input;
		
		/// <summary>Our current offset into the heap</summary>
		private int heapOffset = 0;
		
		/// <summary>A stream used to read the heap data</summary>
		private BufferedDataStream bstr;
    #endregion

		/// <summary>Create a heap of a given size.</summary>
		internal FitsHeap(int size)
		{
			heap = new byte[size];
			heapSize = size;
		}
		
		/// <summary>Read the heap</summary>
		public virtual void Read(ArrayDataIO str)
		{
			if (str is RandomAccess)
			{
				fileOffset = FitsUtil.FindOffset(str);
				input = str;
			}
			
			if (heap != null)
			{
				try
				{
					str.Read(heap, 0, heapSize);
				}
				catch(IOException e)
				{
					throw new FitsException("Error reading heap:" + e);
				}
			}
		}
		
		/// <summary>Write the heap</summary>
		public virtual void Write(ArrayDataIO str)
		{
			try
			{
				str.Write(heap, 0, heapSize);
			}
			catch(IOException e)
			{
				throw new FitsException("Error writing heap:" + e);
			}
		}
		
		/// <summary>Attempt to rewrite the heap with the current contents.
		/// Note that no checking is done to make sure that the
		/// heap does not extend past its prior boundaries.</summary>
		public virtual void Rewrite()
		{
			if(this.Rewriteable)
			{
				//ArrayDataIO str = (ArrayDataIO) input;
				//FitsUtil.Reposition(input, fileOffset);
        input.Seek(fileOffset, SeekOrigin.Begin);
				Write(input);
			}
			else
			{
				throw new FitsException("Invalid attempt to rewrite FitsHeap");
			}
		}
		
		/// <summary>Get data from the heap.</summary>
		/// <param name="offset">The offset at which the data begins.</param>
		/// <param name="array"> The array to be extracted.</param>
		public virtual void GetData(int offset, Object array)
		{
			try
			{
				if (bstr == null || heapOffset > offset)
				{
					heapOffset = 0;
					bstr = new BufferedDataStream(new MemoryStream(heap));
				}
				
				//System.IO.BinaryReader temp_BinaryReader;
				System.Int64 temp_Int64;
				//temp_BinaryReader = bstr;
				temp_Int64 = bstr.Position;  //temp_BinaryReader.BaseStream.Position;
				temp_Int64 = bstr.Seek(offset - heapOffset) - temp_Int64;  //temp_BinaryReader.BaseStream.Seek(offset - heapOffset, System.IO.SeekOrigin.Current) - temp_Int64;
				int generatedAux = (int)temp_Int64;
				heapOffset = offset;
				heapOffset += bstr.ReadArray(array);
			}
			catch(IOException e)
			{
				throw new FitsException("Error decoding heap area at offset=" + offset + ".  Exception: Exception " + e);
			}
		}
		
		/// <summary>Check if the Heap can accommodate a given requirement. If not expand the heap.</summary>
		internal virtual void ExpandHeap(int need)
		{
			if (heapSize + need > heap.Length)
			{
				int newlen = (heapSize + need) * 2;
				if (newlen < 16384)
				{
					newlen = 16384;
				}
				byte[] newHeap = new byte[newlen];
				Array.Copy(heap, 0, newHeap, 0, heapSize);
				heap = newHeap;
			}
		}
		
		/// <summary>Add some data to the heap.</summary>
		internal virtual int PutData(Object data)
		{
			int size = ArrayFuncs.ComputeSize(data);
			ExpandHeap(size);
			MemoryStream bo = new MemoryStream(size);
			
			try
			{
				BufferedDataStream o = new BufferedDataStream(bo);
				o.WriteArray(data);
				o.Flush();
				o.Close();
			}
			catch(IOException)
			{
				throw new FitsException("Unable to write variable column length data");
			}
			
			Array.Copy(bo.ToArray(), 0, heap, heapSize, size);
			int oldOffset = heapSize;
			heapSize += size;
			heapOffset = heapSize;
			
			return oldOffset;
		}

    public virtual int GetSize()
		{
			return heapSize;
		}
	}
}
