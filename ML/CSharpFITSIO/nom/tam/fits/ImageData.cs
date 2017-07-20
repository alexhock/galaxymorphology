namespace nom.tam.fits
{
	using System;
  using System.IO;
	using nom.tam.image;
	using nom.tam.util;
	/* Copyright: Thomas McGlynn 1997-1999.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	* 
	* Many thanks to David Glowacki (U. Wisconsin) for substantial
	* improvements, enhancements and bug fixes.
	*/
	
	
	
	/// <summary>This class instantiates FITS primary HDU and IMAGE extension data.
	/// Essentially these data are a primitive multi-dimensional array.
	/// <p>
	/// Starting in version 0.9 of the FITS library, this routine
	/// allows users to defer the reading of images if the FITS
	/// data is being read from a file.  An ImageTiler object is
	/// supplied which can return an arbitrary subset of the image
	/// as a one dimensional array -- suitable for manipulation by
	/// standard Java libraries.  A call to the getData() method
	/// will still return a multi-dimensional array, but the
	/// image data will not be read until the user explicitly requests.
	/// it.
	/// </summary>
	public class ImageData:Data
	{
		/// <summary>Get the size in bytes of the data 
		/// </summary>
		override internal int TrueSize
		{
			get
			{
				return (int) byteSize;
			}
			
		}
		/// <summary>Return the actual data.
		/// Note that this may return a null when
		/// the data is not readable.  It might be better
		/// to throw a FitsException, but this is
		/// a very commonly called method and we prefered
		/// not to change how users must invoke it.
		/// </summary>
		override public Object DataArray
		{
			get
			{
				if (dataArray == null && tiler != null)
				{
					try
					{
						dataArray = tiler.CompleteImage;
					}
					catch(Exception)
					{
            //Console.Out.WriteLine(e);
						return null;
					}
				}
				
				return dataArray;
			}
		}
		virtual public ImageTiler Tiler
		{
			get
			{
				return tiler;
			}
			
		}
		
		/// <summary>The size of the data 
		/// </summary>
		internal long byteSize;
		
		/// <summary>The actual array of data.  This
		/// is normally a multi-dimensional primitive array.
		/// It may be null until the getData() routine is
		/// invoked, or it may be filled by during the read
		/// call when a non-random access device is used.
		/// </summary>
		internal Object dataArray;
		
		/// <summary>This class describes an array</summary>
		protected internal class ArrayDesc
		{
			private void  InitBlock(ImageData enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private ImageData enclosingInstance;
			public ImageData Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			internal int[] dims;
			internal Type type;
			
			internal ArrayDesc(ImageData enclosingInstance, int[] dims, Type type)
			{
				InitBlock(enclosingInstance);
				this.dims = dims;
				this.type = type;
			}
		}
		
		/// <summary>A description of what the data should look like 
		/// </summary>
		internal ArrayDesc dataDescription;
		
		/// <summary>This inner class allows the ImageTiler
		/// to see if the user has read in the data.
		/// </summary>
		protected internal class ImageDataTiler:ImageTiler
		{
			private void  InitBlock(ImageData enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private ImageData enclosingInstance;
			override public Array MemoryImage
			{
				get
				{
					return (Array)Enclosing_Instance.dataArray;
				}
			}
			public ImageData Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			internal ImageDataTiler(ImageData enclosingInstance, RandomAccess o, long offset, ArrayDesc d):base(o, offset, d.dims, d.type)
			{
				InitBlock(enclosingInstance);
			}
			
		}
		
		/// <summary>The image tiler associated with this image.</summary>
		private ImageTiler tiler;
		
		
		/// <summary>Create an array from a header description.
		/// This is typically how data will be created when reading
		/// FITS data from a file where the header is read first.
		/// This creates an empty array.</summary>
		/// <param name="h">header to be used as a template.</param>
		/// <exception cref=""> FitsException if there was a problem with the header description.</exception>
		public ImageData(Header h)
		{
			dataDescription = ParseHeader(h);
		}
		
		protected internal virtual ArrayDesc ParseHeader(Header h)
		{
			int bitpix;
//			int type;
			int ndim;
			int[] dims;
			int i;
			
			//Object dataArray;
			Type baseClass;
			
			int gCount = h.GetIntValue("GCOUNT", 1);
			int pCount = h.GetIntValue("PCOUNT", 0);
			if (gCount > 1 || pCount != 0)
			{
				throw new FitsException("Group data treated as images");
			}
			
			bitpix = (int) h.GetIntValue("BITPIX", 0);
			
			if (bitpix == 8)
			{
				baseClass = Type.GetType("System.Byte");
			}
			else if (bitpix == 16)
			{
				baseClass = Type.GetType("System.Int16");
			}
			else if (bitpix == 32)
			{
				baseClass = Type.GetType("System.Int32");
			}
			else if (bitpix == 64)
			{
				/* This isn't a standard for FITS yet...*/
				baseClass = Type.GetType("System.Int64");
			}
			else if (bitpix == - 32)
			{
				baseClass = Type.GetType("System.Single");
			}
			else if (bitpix == - 64)
			{
				baseClass = Type.GetType("System.Double");
			}
			else
			{
				throw new FitsException("Invalid BITPIX:" + bitpix);
			}
			
			ndim = h.GetIntValue("NAXIS", 0);
			dims = new int[ndim];
			
			
			// Note that we have to invert the order of the axes
			// for the FITS file to get the order in the array we
			// are generating.
			
			byteSize = 1;
			for (i = 0; i < ndim; i += 1)
			{
				int cdim = h.GetIntValue("NAXIS" + (i + 1), 0);
				if (cdim < 0)
				{
					throw new FitsException("Invalid array dimension:" + cdim);
				}
				byteSize *= cdim;
				dims[ndim - i - 1] = (int) cdim;
			}
			byteSize *= Math.Abs(bitpix) / 8;
			if (ndim == 0)
			{
				byteSize = 0;
			}
			return new ArrayDesc(this, dims, baseClass);
		}
		
		/// <summary>Create the equivalent of a null data element.</summary>
		public ImageData()
		{
			dataArray = new byte[0];
			byteSize = 0;
		}
		
		/// <summary>Create an ImageData object using the specified object to
		/// initialize the data array.
		/// </summary>
		/// <param name="x">The initial data array.  This should be a primitive
		/// array but this is not checked currently.
		/// 
		/// </param>
		public ImageData(Object x)
		{
			dataArray = x;
			byteSize = ArrayFuncs.ComputeSize(x);
		}
		
		/// <summary>Fill header with keywords that describe image data.</summary>
		/// <param name="head">The FITS header</param>
		/// <exception cref=""> FitsException if the object does not contain valid image data.</exception>
		internal override void FillHeader(Header head)
		{
			if (dataArray == null)
			{
				head.NullImage();
				return ;
			}
			
			System.String classname = dataArray.GetType().FullName;
			
			int[] dimens = ArrayFuncs.GetDimensions(dataArray);
			
			if (dimens == null || dimens.Length == 0)
			{
				throw new FitsException("Image data object not array");
			}
			
			
			int bitpix;
			switch (classname[dimens.Length])
			{
				case 'B': 
					bitpix = 8;
					break;
				case 'S': 
					bitpix = 16;
					break;
				case 'I': 
					bitpix = 32;
					break;
				case 'J': 
					bitpix = 64;
					break;
				case 'F': 
					bitpix = - 32;
					break;
				case 'D': 
					bitpix = - 64;
					break;
				default: 
					throw new FitsException("Invalid Object Type for FITS data:" + classname[dimens.Length]);
			}
			
			// if this is neither a primary header nor an image extension,
			// make it a primary header
			head.Simple = true;
			head.Bitpix = bitpix;
			head.Naxes = dimens.Length;
			
			for (int i = 1; i <= dimens.Length; i += 1)
			{
				if (dimens[i - 1] == - 1)
				{
					throw new FitsException("Unfilled array for dimension: " + i);
				}
				head.SetNaxis(i, dimens[dimens.Length - i]);
			}
			head.AddValue("EXTEND", true, "Extension permitted"); // Just in case!
			head.AddValue("PCOUNT", 0, "No extra parameters");
			head.AddValue("GCOUNT", 1, "One group");
		}
		
		public override void Read(ArrayDataIO i)
		{
			// Don't need to read null data (noted by Jens Knudstrup)
			if (byteSize == 0)
			{
				return ;
			}
			SetFileOffset(i);
			
			//if(i is RandomAccess)
      if(i.CanSeek)
			{
				//tiler = new ImageDataTiler(this, (RandomAccess) i, ((RandomAccess) i).FilePointer, dataDescription);
        tiler = new ImageDataTiler(this, (RandomAccess) i, ((Stream)i).Position, dataDescription);
				try
				{
					double pos = i.Position;
					pos = i.Seek((int)byteSize) - pos;
				}
				catch(IOException e)
				{
					throw new FitsException("Unable to skip over data:" + e);
				}
			}
			else
			{
				dataArray = ArrayFuncs.NewInstance(dataDescription.type, dataDescription.dims);
				try
				{
					i.ReadArray(dataArray);
				}
				catch(IOException e)
				{
					throw new FitsException("Unable to read image data:" + e);
				}
				
				tiler = new ImageDataTiler(this, null, 0, dataDescription);
			}
			
			int pad = FitsUtil.Padding(TrueSize);
			try
			{
				long pos = i.Seek(pad);
        if(pos != pad)
				{
          throw new FitsException("Error skipping padding");
				}
			}
			catch(IOException e)
			{
				throw new FitsException("Error reading image padding:" + e);
			}
		}
		
		public override void Write(ArrayDataIO o)
		{
			// Don't need to write null data (noted by Jens Knudstrup)
			if (byteSize == 0)
			{
				return ;
			}
			
			if (dataArray == null)
			{
				if (tiler != null)
				{
					
					// Need to read in the whole image first.
					try
					{
						dataArray = tiler.CompleteImage;
					}
					catch(IOException)
					{
						throw new FitsException("Error attempting to fill image");
					}
				}
				else if (dataArray == null && dataDescription != null)
				{
					// Need to create an array to match a specified header.
					dataArray = ArrayFuncs.NewInstance(dataDescription.type, dataDescription.dims);
				}
				else
				{
					// This image isn't ready to be written!
					throw new FitsException("Null image data");
				}
			}
			
			try
			{
				o.WriteArray(dataArray);
			}
			catch(IOException e)
			{
				throw new FitsException("IO Error on image write" + e);
			}
			
			byte[] padding = new byte[FitsUtil.Padding(TrueSize)];
			try
			{
				o.Write(padding);
				o.Flush();
			}
			catch(IOException e)
			{
				throw new FitsException("Error writing padding: " + e);
			}
		}
	}
}
