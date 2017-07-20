namespace nom.tam.fits
{
	/*
	* Copyright: Thomas McGlynn 1997-1999.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	*/
	using System;
  using System.Data;
  using System.IO;
	using nom.tam.util;
	/// <summary>This class provides access to routines to allow users
	/// to read and write FITS files.
	/// <p>
	/// *
	/// <p>
	/// <b> Description of the Package </b>
	/// <p>
	/// This FITS package attempts to make using FITS files easy,
	/// but does not do exhaustive error checking.  Users should
	/// not assume that just because a FITS file can be read
	/// and written that it is necessarily legal FITS.
	/// *
	/// *
	/// <ul>
	/// <li> The Fits class provides capabilities to
	/// read and write data at the HDU level, and to
	/// add and delete HDU's from the current Fits object.
	/// A large number of constructors are provided which
	/// allow users to associate the Fits object with
	/// some form of external data.  This external
	/// data may be in a compressed format.
	/// <li> The HDU class is a factory class which is used to
	/// create HDUs.  HDU's can be of a number of types
	/// derived from the abstract class BasicHDU.
	/// The hierarchy of HDUs is:
	/// <ul>
	/// <li>BasicHDU
	/// <ul>
	/// <li> ImageHDU
	/// <li> RandomGroupsHDU
	/// <li> TableHDU
	/// <ul>
	/// <li> BinaryTableHDU
	/// <li> AsciiTableHDU
	/// </ul>
	/// </ul>
	/// </ul>
	/// *
	/// <li> The Header class provides many functions to
	/// add, delete and read header keywords in a variety
	/// of formats.
	/// <li> The HeaderCard class provides access to the structure
	/// of a FITS header card.
	/// <li> The Data class is an abstract class which provides
	/// the basic methods for reading and writing FITS data.
	/// Users will likely only be interested in the getData
	/// method which returns that actual FITS data.
	/// <li> The TableHDU class provides a large number of
	/// methods to access and modify information in
	/// tables.
	/// <li> The Column class
	/// combines the Header information and Data corresponding to
	/// a given column.
	/// </ul>
	/// *
	/// Copyright: Thomas McGlynn 1997-1999.
	/// This code may be used for any purpose, non-commercial
	/// or commercial so long as this copyright notice is retained
	/// in the source code or included in or referred to in any
	/// derived software.
	/// *
	/// </summary>
	/// <version>  0.96a  October 10, 2000
	/// 
	/// </version>
	public class Fits
	{
    public static readonly String DEFAULT_TEMP_DIR = Environment.CurrentDirectory;

    public static void Write(IDataReader reader, String filename)
    {
      Write(reader, filename, StreamedBinaryTableHDU.StringWriteMode.PAD, 128, true, ' ');
    }

      public static void Write(IDataReader reader, String filename,
      StreamedBinaryTableHDU.StringWriteMode writeMode, int stringTruncationLength,
        bool padStringsLeft, char stringPadChar)
    {
      Header header = new Header();
      header.Simple = true;
      header.Bitpix = 8;
      header.Naxes = 0;

      Cursor c = header.GetCursor();
      // move to the end of the header cards
      for(c.MoveNext(); c.MoveNext(););
      // we know EXTEND isn't there yet
      c.Add("EXTEND", new HeaderCard("EXTEND", true, null));

      ImageHDU hdu1 = new ImageHDU(header, null);

      StreamedBinaryTableHDU hdu2 =
        new StreamedBinaryTableHDU(new DataReaderAdapter(reader), 4096,
        writeMode, stringTruncationLength, padStringsLeft, stringPadChar);

      Fits fits = new Fits();
      fits.addHDU(hdu1);
      fits.addHDU(hdu2);

      Stream s = null;
      try
      {
        s = new FileStream(filename, FileMode.Create);
        fits.Write(s);
        s.Close();
      }
      catch(Exception e)
      {
        s.Close();
        throw(e);
      }
    }

    public static String TempDirectory
    {
      get
      {
        return _tempDir;
      }

      set
      {
        _tempDir = value;
      }
    }

    private void  InitBlock()
		{
			hduList = new System.Collections.ArrayList();
		}

    /// <summary>Get the current number of HDUs in the Fits object.</summary>
		/// <returns>The number of HDU's in the object.</returns>
		virtual public int NumberOfHDUs
		{
			get
			{
				return hduList.Count;
			}
		}

    /// <summary>Get the data stream used for the Fits Data.</summary>
		/// <returns> The associated data stream.  Users may wish to
		/// call this function after opening a Fits object when
		/// they wish detailed control for writing some part of the FITS file.
		/// </returns>
		/// <summary>Set the data stream to be used for future input.</summary>
		/// <param name="stream">The data stream to be used.</param>
		virtual public ArrayDataIO Stream
		{
			get
			{
				return dataStr;
			}
			
			set
			{
				dataStr = value;
				atEOF = false;
			}
		}
		
		/// <summary>The input stream associated with this Fits object.</summary>
		private ArrayDataIO dataStr;
		
		/// <summary>A vector of HDUs that have been added to this Fits object.</summary>
		private System.Collections.ArrayList hduList;
		
		/// <summary>Has the input stream reached the EOF?</summary>
		private bool atEOF;
		
    protected static String _tempDir = DEFAULT_TEMP_DIR;

		/// <summary>Indicate the version of these classes</summary>
		public static System.String version()
		{
			// Version 0.1: Original test FITS classes -- 9/96
			// Version 0.2: Pre-alpha release 10/97
			//              Complete rewrite using BufferedData*** and
			//              ArrayFuncs utilities.
			// Version 0.3: Pre-alpha release  1/98
			//              Incorporation of HDU hierarchy developed
			//              by Dave Glowacki and various bug fixes.
			// Version 0.4: Alpha-release 2/98
			//              BinaryTable classes revised to use
			//              ColumnTable classes.
			// Version 0.5: Random Groups Data 3/98
			// Version 0.6: Handling of bad/skipped FITS, FitsDate (D. Glowacki) 3/98
			// Version 0.9: ASCII tables, Tiled images, Faux, Bad and SkippedHDU class
			//              deleted. 12/99
			// Version 0.91: Changed visibility of some methods.
			//               Minor fixes.
			// Version 0.92: Fix bug in BinaryTable when reading from stream.
			// Version 0.93: Supports HIERARCH header cards.  Added FitsElement interface.
			//               Several bug fixes especially for null HDUs.
			// Version 0.96: Address issues with mandatory keywords.
			//               Fix problem where some keywords were not properly keyed.
			// Version 0.96a: Update version in FITS
			
			return "0.96a";
		}
		
		/// <summary>Create an empty Fits object which is not associated with an input stream.</summary>
		public Fits()
		{
			InitBlock();
		}
		
		/// <summary>Create a Fits object associated with
		/// the given uncompressed data stream.
		/// </summary>
		/// <param name="str">The data stream.</param>
		public Fits(Stream str):this(str, false)
		{
			InitBlock();
		}
		
		/// <summary>Create a Fits object associated with a possibly
		/// compressed data stream.
		/// </summary>
		/// <param name="str">The data stream.</param>
		/// <param name="compressed">Is the stream compressed?</param>
		public Fits(Stream str, bool compressed)
		{
			InitBlock();
			streamInit(str, compressed, false);
		}
		
		/// <summary>Do the stream initialization.</summary>
		/// <param name="str">The input stream.</param>
		/// <param name="compressed">Is this data compressed?  If so,
		/// then the GZIPInputStream class will be
		/// used to inflate it.</param>
		protected internal virtual void streamInit(Stream str, bool compressed, bool seekable)
		{
			if (str == null)
			{
				throw new FitsException("Null input stream");
			}
			
			if (compressed)
			{
        // UPGRADE_TODO: ***FIX THIS***
//				try
//				{
//					//UPGRADE_TODO: Constructor java.util.zip.GZIPInputStream.GZIPInputStream was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
//					str = new GZIPInputStream(str);
//				}
//				catch (System.IO.IOException e)
				{
					//throw new FitsException("Cannot inflate input stream" + e);
          throw new FitsException("GZipped files not yet supported.");
				}
			}
			
			if(str is ArrayDataIO)
			{
				dataStr = (ArrayDataIO)str;
			}
			else
			{
				// Use efficient blocking for input.
				dataStr = new BufferedDataStream(str);
			}
		}
		
		/// <summary>Initialize using buffered random access</summary>
		protected internal virtual void  randomInit(String filename)
		{
      FileInfo f = new FileInfo(filename);
      FileAccess access =
        SupportClass.FileCanWrite(f) ? FileAccess.ReadWrite : FileAccess.Read;

      if(!f.Exists)
			{
				throw new FitsException("File '" + filename + "' does not exist.");
			}
			try
			{
				dataStr = new BufferedFile(filename, access);
				
				((BufferedFile)dataStr).Seek(0);
			}
			catch(IOException)
			{
				throw new FitsException("Unable to open file " + filename);
			}
		}
		
		/// <summary>Associate FITS object with an uncompressed File</summary>
		/// <param name="myFile">The File object.</param>
		public Fits(FileInfo myFile):this(myFile, false)
		{
			InitBlock();
		}
		
		/// <summary>Associate the Fits object with a File</summary>
		/// <param name="myFile">The File object.</param>
		/// <param name="compressed">Is the data compressed?</param>
		public Fits(FileInfo myFile, bool compressed)
		{
			InitBlock();
			fileInit(myFile, compressed);
		}
		
		/// <summary>Get a stream from the file and then use the stream initialization.</summary>
		/// <param name="myFile">The File to be associated.</param>
		/// <param name="compressed">Is the data compressed?</param>
		protected internal virtual void  fileInit(FileInfo myFile, bool compressed)
		{
			try
			{
				FileStream str = new FileStream(myFile.FullName, FileMode.Open, FileAccess.Read);
				streamInit(str, compressed, true);
			}
			catch(IOException)
			{
				throw new FitsException("Unable to create Input Stream from File: " + myFile);
			}
		}
		
		/// <summary>Associate the FITS object with a file or URL.
		/// *
		/// The string is assumed to be a URL if it begins with
		/// http:  otherwise it is treated as a file name.
		/// If the string ends in .gz it is assumed that
		/// the data is in a compressed format.
		/// All string comparisons are case insensitive.
		/// *
		/// </summary>
		/// <param name="filename">The name of the file or URL to be processed.</param>
		/// <exception cref=""> FitsException Thrown if unable to find or open
		/// a file or URL from the string given.</exception>
		public Fits(String filename)
		{
			InitBlock();
			
			//Stream inp;
			
			if (filename == null)
			{
				throw new FitsException("Null FITS Identifier String");
			}
			
			bool compressed = FitsUtil.IsCompressed(filename);
			
			int len = filename.Length;
			if (len > 4 && filename.Substring(0, (5) - (0)).ToUpper().Equals("http:".ToUpper()))
			{
				// This seems to be a URL.
				System.Uri myURL;
				try
				{
					//UPGRADE_TODO: Class 'java.net.URL' was converted to a 'System.Uri' which does not throw an exception if a URL specifies an unknown protocol. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1132"'
					myURL = new Uri(filename);
				}
				catch(IOException)
				{
					throw new FitsException("Unable to convert string to URL: " + filename);
				}
				try
				{
					Stream is_Renamed = System.Net.WebRequest.Create(myURL).GetResponse().GetResponseStream();
					streamInit(is_Renamed, compressed, false);
				}
				catch(IOException e)
				{
					throw new FitsException("Unable to open stream from URL:" + filename + " Exception=" + e);
				}
			}
			else if (compressed)
			{
				fileInit(new FileInfo(filename), true);
			}
			else
			{
				randomInit(filename);
			}
		}
		
		/// <summary>Associate the FITS object with a given uncompressed URL</summary>
		/// <param name="myURL">The URL to be associated with the FITS file.</param>
		/// <exception cref="">FitsException Thrown if unable to use the specified URL.</exception>
		public Fits(Uri myURL):this(myURL, FitsUtil.IsCompressed(myURL.AbsolutePath))
		{
			InitBlock();
		}
		
		/// <summary>Associate the FITS object with a given URL</summary>
		/// <param name="myURL">The URL to be associated with the FITS file.</param>
		/// <param name="compressed">Is the data compressed?</param>
		/// <exception cref="">FitsException Thrown if unable to find or open
		/// a file or URL from the string given.</exception>
		public Fits(Uri myURL, bool compressed)
		{
			InitBlock();
			try
			{
				streamInit(System.Net.WebRequest.Create(myURL).GetResponse().GetResponseStream(), compressed, false);
			}
			catch(IOException)
			{
				throw new FitsException("Unable to open input from URL:" + myURL);
			}
		}
		
		/// <summary>Return all HDUs for the Fits object.   If the
		/// FITS file is associated with an external stream make
		/// sure that we have exhausted the stream.</summary>
		/// <returns> an array of all HDUs in the Fits object.  Returns
		/// null if there are no HDUs associated with this object.
		/// </returns>
		public virtual BasicHDU[] Read()
		{
			readToEnd();
			
			int size = NumberOfHDUs;
			
			if (size == 0)
			{
				return null;
			}
			
			BasicHDU[] hdus = new BasicHDU[size];
			hduList.CopyTo(hdus);
			return hdus;
		}
		
		/// <summary>Read the next HDU on the default input stream.</summary>
		/// <returns>The HDU read, or null if an EOF was detected.
		/// Note that null is only returned when the EOF is detected immediately
		/// at the beginning of reading the HDU.</returns>
		public virtual BasicHDU readHDU()
		{
			if(dataStr == null || atEOF)
			{
				return null;
			}
			
			Header hdr = Header.ReadHeader(dataStr);
			if (hdr == null)
			{
				atEOF = true;
				return null;
			}
			
			Data datum = hdr.MakeData();
			datum.Read(dataStr);
			BasicHDU nextHDU = FitsFactory.HDUFactory(hdr, datum);
			
			hduList.Add(nextHDU);
			return nextHDU;
		}
		
		/// <summary>Skip HDUs on the associate input stream.</summary>
		/// <param name="n">The number of HDUs to be skipped.</param>
		public virtual void  skipHDU(int n)
		{
			for (int i = 0; i < n; i += 1)
			{
				skipHDU();
			}
		}
		
		/// <summary>Skip the next HDU on the default input stream.</summary>
		public virtual void  skipHDU()
		{
			if (atEOF)
			{
				return ;
			}
			else
			{
				Header hdr = new Header(dataStr);
				if (hdr == null)
				{
					atEOF = true;
					return ;
				}
				int dataSize = (int) hdr.DataSize;
//				dataStr.Skip(dataSize);
   				dataStr.Seek(dataSize);
			}
		}
		
		/// <summary>Return the n'th HDU.
		/// If the HDU is already read simply return a pointer to the
		/// cached data.  Otherwise read the associated stream
		/// until the n'th HDU is read.
		/// </summary>
		/// <param name="n">The index of the HDU to be read.  The primary HDU is index 0.</param>
		/// <returns> The n'th HDU or null if it could not be found.</returns>
		public virtual BasicHDU getHDU(int n)
		{
			int size = NumberOfHDUs;
			
			for (int i = size; i <= n; i += 1)
			{
				BasicHDU hdu;
				hdu = readHDU();
				if (hdu == null)
				{
					return null;
				}
			}
			
			try
			{
				return (BasicHDU) hduList[n];
			}
			catch (System.Exception)
			{
				throw new FitsException("Internal Error: hduList build failed");
			}
		}
		
		/// <summary>Read to the end of the associated input stream</summary>
		private void  readToEnd()
		{
			while (dataStr != null && !atEOF)
			{
				try
				{
					if (readHDU() == null)
					{
						break;
					}
				}
				catch(IOException e)
				{
					throw new FitsException("IO error: " + e);
				}
			}
		}
		
		/// <summary>Return the number of HDUs in the Fits object.   If the
		/// FITS file is associated with an external stream make
		/// sure that we have exhausted the stream.
		/// </summary>
		/// <returns>number of HDUs.</returns>
		/// <deprecated>The meaning of size of ambiguous.  Use</deprecated>
		public virtual int size()
		{
			readToEnd();
			return NumberOfHDUs;
		}
		
		/// <summary>Add an HDU to the Fits object.  Users may intermix
		/// calls to functions which read HDUs from an associated
		/// input stream with the addHDU and insertHDU calls,
		/// but should be careful to understand the consequences.
		/// </summary>
		/// <param name="myHDU"> The HDU to be added to the end of the FITS object.</param>
		public virtual void  addHDU(BasicHDU myHDU)
		{
			insertHDU(myHDU, NumberOfHDUs);
		}
		
		/// <summary>Insert a FITS object into the list of HDUs.</summary>
		/// <param name="myHDU">The HDU to be inserted into the list of HDUs.</param>
		/// <param name="n">The location at which the HDU is to be inserted.</param>
		public virtual void  insertHDU(BasicHDU myHDU, int n)
		{
			if (myHDU == null)
			{
				return ;
			}
			
			if (n < 0 || n > NumberOfHDUs)
			{
				throw new FitsException("Attempt to insert HDU at invalid location: " + n);
			}
			
			try
			{
				
				if (n == 0)
				{
					
					// Note that the previous initial HDU is no longer the first.
					// If we were to insert tables backwards from last to first,
					// we could get a lot of extraneous DummyHDUs but we currently
					// do not worry about that.
					
					if (NumberOfHDUs > 0)
					{
						((BasicHDU) hduList[0]).PrimaryHDU = false;
					}
					
					if (myHDU.CanBePrimary)
					{
						myHDU.PrimaryHDU = true;
						hduList.Insert(0, myHDU);
					}
					else
					{
						insertHDU(BasicHDU.DummyHDU, 0);
						myHDU.PrimaryHDU = false;
						hduList.Insert(1, myHDU);
					}
				}
				else
				{
					myHDU.PrimaryHDU = false;
					hduList.Insert(n, myHDU);
				}
			}
			catch(Exception e)
			{
				throw new FitsException("hduList inconsistency in insertHDU: " + e);
			}
		}
		
		/// <summary>Delete an HDU from the HDU list.</summary>
		/// <param name="n"> The index of the HDU to be deleted.
		/// If n is 0 and there is more than one HDU present, then
		/// the next HDU will be converted from an image to
		/// primary HDU if possible.  If not a dummy header HDU
		/// will then be inserted.</param>
		public virtual void  deleteHDU(int n)
		{
			int size = NumberOfHDUs;
			if (n < 0 || n >= size)
			{
				throw new FitsException("Attempt to delete non-existent HDU:" + n);
			}
			try
			{
				hduList.RemoveAt(n);
				if (n == 0 && size > 1)
				{
					BasicHDU newFirst = (BasicHDU) hduList[0];
					if (newFirst.CanBePrimary)
					{
						newFirst.PrimaryHDU = true;
					}
					else
					{
						insertHDU(BasicHDU.DummyHDU, 0);
					}
				}
			}
			catch(Exception)
			{
				throw new FitsException("Internal Error: hduList Vector Inconsitency");
			}
		}

    /// <summary>Write a Fits Object to an external Stream.  The stream is left open.</summary>
		/// <param name="dos">A DataOutput stream</param>
		public virtual void Write(Stream os)
		{
			ArrayDataIO obs;
//			bool newOS = false;
			
			if(os is ArrayDataIO)
			{
				obs = (ArrayDataIO) os;
			}
			else
			{
//				newOS = true;
				obs = new BufferedDataStream(os);
			}
			
			BasicHDU hh;
			for(int i = 0; i < NumberOfHDUs; i += 1)
			{
				try
				{
					hh = (BasicHDU)hduList[i];
					hh.Write(obs);
				}
				catch(IndexOutOfRangeException e)
				{
					SupportClass.WriteStackTrace(e, Console.Error);
					throw new FitsException("Internal Error: Vector Inconsistency" + e);
				}
			}
      obs.Flush();
      #region oldCrap
      /*
			if(newOS)
			{
				try
				{
					obs.Flush();
					obs.Close();
				}
				catch(IOException)
				{
					Console.Error.WriteLine("Warning: error closing FITS output stream");
				}
			}
      */
      #endregion
		}

		/// <summary>Read a FITS file from an InputStream object.</summary>
		/// <param name="is">The InputStream stream whence the FITS information is found.</param>
		public virtual void  read(Stream is_Renamed)
		{
			bool newIS = false;
			
			if (is_Renamed is ArrayDataIO)
			{
				dataStr = (ArrayDataIO) is_Renamed;
			}
			else
			{
				dataStr = new BufferedDataStream(is_Renamed);
			}
			
			Read();
			
			if (newIS)
			{
				dataStr.Close();
				dataStr = null;
			}
		}
		
		/// <summary>Get the current number of HDUs in the Fits object.</summary>
		/// <returns>The number of HDU's in the object.</returns>
		/// <deprecated>See getNumberOfHDUs()</deprecated>
		public virtual int currentSize()
		{
			return hduList.Count;
		}

    /// <summary>Create an HDU from the given header.</summary>
		/// <param name="h"> The header which describes the FITS extension</param>
		public static BasicHDU makeHDU(Header h)
		{
			Data d = FitsFactory.DataFactory(h);
			return FitsFactory.HDUFactory(h, d);
		}
		
		/// <summary>Create an HDU from the given data kernel.</summary>
		/// <param name="o">The data to be described in this HDU.</param>
		public static BasicHDU makeHDU(System.Object o)
		{
			return FitsFactory.HDUFactory(o);
		}
		
		/// <summary>Create an HDU from the given Data.</summary>
		/// <param name="datum">The data to be described in this HDU.</param>
		public static BasicHDU makeHDU(Data datum)
		{
			Header hdr = new Header();
			datum.FillHeader(hdr);
			return FitsFactory.HDUFactory(hdr, datum);
		}
	}
}
