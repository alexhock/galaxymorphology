namespace nom.tam.util
{
	using System;
	
	/// <summary>This class provides routines
	/// for efficient parsing of data stored in a byte array.
	/// This routine is optimized (in theory at least!) for efficiency
	/// rather than accuracy.  The values read in for doubles or floats
	/// may differ in the last bit or so from the standard input
	/// utilities, especially in the case where a float is specified
	/// as a very long string of digits (substantially longer than
	/// the precision of the type).
	/// <p>
	/// The get methods generally are available with or without a length
	/// parameter specified.  When a length parameter is specified only
	/// the bytes with the specified range from the current offset will
	/// be search for the number.  If no length is specified, the entire
	/// buffer from the current offset will be searched.
	/// <p>
	/// The getString method returns a string with leading and trailing
	/// white space left intact.  For all other get calls, leading
	/// white space is ignored.  If fillFields is set, then the get
	/// methods check that only white space follows valid data and a
	/// FormatException is thrown if that is not the case.  If
	/// fillFields is not set and valid data is found, then the
	/// methods return having read as much as possible.  E.g., for
	/// the sequence "T123.258E13", a getBoolean, getInteger and
	/// getFloat call would return true, 123, and 2.58e12 when
	/// called in succession.
	/// 
	/// </summary>
	public class ByteParser
	{
    #region Properties
    /// <summary>Get the buffer being used by the parser</summary>
		/// <summary>Set the buffer for the parser</summary>
		virtual public byte[] Buffer
		{
			get
			{
				return input;
			}
			
			set
			{
				this.input = value;
				this.offset = 0;
			}
		}
		/// <summary>Get the current offset</summary>
		/// <returns>The current offset within the buffer.</returns>
		/// <summary>Set the offset into the array.</summary>
		/// <param name="offset	The">desired offset from the beginning of the array.</param>
		virtual public int Offset
		{
			get
			{
				return offset;
			}
			
			set
			{
				this.offset = value;
			}
		}
		/// <summary>Do we require a field to completely fill up the specified
		/// length (with optional leading and trailing white space.
		/// </summary>
		/// <param name="flag	Is">filling required?</param>
		virtual public bool FillFields
		{
			set
			{
				fillFields = value;
			}
		}
		/// <summary>Get the number of characters used to parse the previous
		/// number (or the length of the previous String returned).
		/// </summary>
		virtual public int NumberLength
		{
			get
			{
				return numberLength;
			}
		}
		/// <summary>Read in the buffer until a double is read.  This will read
		/// the entire buffer if fillFields is set.
		/// </summary>
		/// <returns> The value found.
		/// 
		/// </returns>
		virtual public double Double
		{
			get
			{
				return getDouble(input.Length - offset);
			}
		}
		/// <summary>Get a floating point value from the buffer.  (see getDouble(int())
		/// </summary>
		virtual public float Float
		{
			get
			{
				return (float) getDouble(input.Length - offset);
			}
			
		}
		/// <summary>Look for an integer at the beginning of the buffer 
		/// </summary>
		virtual public int Int
		{
			get
			{
				return getInt(input.Length - offset);
			}
			
		}
		/// <summary>Get a boolean value from the beginning of the buffer 
		/// </summary>
		virtual public bool Boolean
		{
			get
			{
				return getBoolean(input.Length - offset);
			}
			
		}
		
    #endregion

    #region Variables
		/// <summary>Array being parsed</summary>
		private byte[] input;
		
		/// <summary>Current offset into input.</summary>
		private int offset;
		
		/// <summary>Length of last parsed value</summary>
		private int numberLength;
		
		/// <summary>Did we find a sign last time we checked?</summary>
		private bool foundSign;
		
		/// <summary>Do we fill up fields?</summary>
		private bool fillFields = false;
    #endregion

		/// <summary>Construct a parser.</summary>
		/// <param name="input	The">byte array to be parsed.
		/// Note that the array can be re-used by
		/// refilling its contents and resetting the offset.</param>
		public ByteParser(byte[] input)
		{
			this.input = input;
			this.offset = 0;
		}

    /// <summary>Look for a double in the buffer.  Leading spaces are ignored.</summary>
		/// <param name="length">The maximum number of characters used to parse this number.
		/// If fillFields is specified then exactly only whitespace may follow
		///  a valid double value.</param>
		public virtual double getDouble(int length)
		{
			//	System.out.println("Checking: "+new String(input, offset, length));
			
			int startOffset = offset;
			
			bool error = true;
			
			double number = 0;
//			int i = 0;
			
			// Skip initial blanks.
			length -= skipWhite(length);
			if (length == 0)
			{
				return 0;
			}
			
			double mantissaSign = checkSign();
			if (foundSign)
			{
				length -= 1;
			}
			
			number = getBareInteger(length); // This will update offset
			length -= numberLength; // Set by getBareInteger
			
			if (numberLength > 0)
			{
				error = false;
			}
			
			// Check for fractional values after decimal
			if (length > 0 && input[offset] == '.')
			{
				
				offset += 1;
				length -= 1;
				
				double numerator = getBareInteger(length);
				if (numerator > 0)
				{
					number += numerator / System.Math.Pow(10.0, numberLength);
				}
				length -= numberLength;
				if (numberLength > 0)
				{
					error = false;
				}
			}
			
			if (error)
			{
				offset = startOffset;
				numberLength = 0;
				throw new FormatException("Invalid real field");
			}
			
			// Look for an exponent
			if (length > 0)
			{
				
				// Our Fortran heritage means that we allow 'D' for the exponent indicator.
				if (input[offset] == 'e' || input[offset] == 'E' || input[offset] == 'd' || input[offset] == 'D')
				{
					
					offset += 1;
					length -= 1;
					if (length > 0)
					{
						int sign = checkSign();
						if (foundSign)
						{
							length -= 1;
						}
						
						//UPGRADE_WARNING: Narrowing conversions may produce unexpected results in C#. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1042"'
						int exponent = (int) getBareInteger(length);
						number *= System.Math.Pow(10.0, exponent * sign);
						length -= numberLength;
					}
				}
			}
			
			if (fillFields && length > 0)
			{
				
				if (isWhite(length))
				{
					offset += length;
				}
				else
				{
					numberLength = 0;
					offset = startOffset;
					throw new FormatException("Non-blanks following real.");
				}
			}
			
			numberLength = offset - startOffset;
			return mantissaSign * number;
		}

    /// <summary>Get a floating point value in a region of the buffer</summary>
		public virtual float getFloat(int length)
		{
			return (float) getDouble(length);
		}
		
		/// <summary>Convert a region of the buffer to an integer</summary>
		public virtual int getInt(int length)
		{
			int startOffset = offset;
			
			length -= skipWhite(length);
			
			int number = 0;
			bool error = true;
			
			int sign = checkSign();
			if (foundSign)
			{
				length -= 1;
			}
			
			while (length > 0 && input[offset] >= '0' && input[offset] <= '9')
			{
				number = number * 10 + input[offset] - '0';
				offset += 1;
				length -= 1;
				error = false;
			}
			
			if (error)
			{
				numberLength = 0;
				offset = startOffset;
				throw new FormatException("Invalid Integer");
			}
			
			if (length > 0 && fillFields)
			{
				if (isWhite(length))
				{
					offset += length;
				}
				else
				{
					numberLength = 0;
					offset = startOffset;
					throw new FormatException("Non-white following integer");
				}
			}
			
			numberLength = offset - startOffset;
			return sign * number;
		}

    /// <summary>Look for a long in a specified region of the buffer</summary>
		public virtual long getLong(int length)
		{
			int startOffset = offset;
			
			// Skip white space.
			length -= skipWhite(length);
			
			long number = 0;
			bool error = true;
			
			long sign = checkSign();
			if (foundSign)
			{
				length -= 1;
			}
			
			while (length > 0 && input[offset] >= '0' && input[offset] <= '9')
			{
				number = number * 10 + input[offset] - '0';
				error = false;
				offset += 1;
				length -= 1;
			}
			
			if (error)
			{
				numberLength = 0;
				offset = startOffset;
				throw new FormatException("Invalid long number");
			}
			
			if (length > 0 && fillFields)
			{
				if (isWhite(length))
				{
					offset += length;
				}
				else
				{
					offset = startOffset;
					numberLength = 0;
					throw new FormatException("Non-white following long");
				}
			}
			numberLength = offset - startOffset;
			return sign * number;
		}
		
		/// <summary>Get a string</summary>
		/// <param name="length">The length of the string.</param>
		public virtual String getString(int length)
		{
			//char[] tmpChar;
			//tmpChar = new char[input.Length];
			//input.CopyTo(tmpChar, 0);
      /// TODO: FIGURE OUT WHY THIS HANGS ALL THE TIME
			//String s = new String(tmpChar, offset, length);
      char[] tmpChar = new char[length];
      Array.Copy(input, offset, tmpChar, 0, length);
      String s = new String(tmpChar);//new String(input, offset, length);
			offset += length;
			numberLength = length;
			return s;
		}

    /// <summary>Get a boolean value from a specified region of the buffer</summary>
		public virtual bool getBoolean(int length)
		{
			int startOffset = offset;
			length -= skipWhite(length);
			if (length == 0)
			{
				throw new FormatException("Blank boolean field");
			}
			
			bool value_Renamed = false;
			if (input[offset] == 'T' || input[offset] == 't')
			{
				value_Renamed = true;
			}
			else if (input[offset] != 'F' && input[offset] != 'f')
			{
				numberLength = 0;
				offset = startOffset;
				throw new FormatException("Invalid boolean value");
			}
			offset += 1;
			length -= 1;
			
			if (fillFields && length > 0)
			{
				if (isWhite(length))
				{
					offset += length;
				}
				else
				{
					numberLength = 0;
					offset = startOffset;
					throw new FormatException("Non-white following boolean");
				}
			}
			numberLength = offset - startOffset;
			return value_Renamed;
		}
		
		/// <summary>Skip bytes in the buffer 
		/// </summary>
		public virtual void  skip(int nBytes)
		{
			offset += nBytes;
		}
		
		
		/// <summary>Get the integer value starting at the current position.
		/// This routine returns a double rather than an int/long
		/// to enable it to read very long integers (with reduced
		/// precision) such as 111111111111111111111111111111111111111111.
		/// Note that this routine does set numberLength.
		/// *
		/// </summary>
		/// <param name="length">The maximum number of characters to use.</param>
		private double getBareInteger(int length)
		{
			int startOffset = offset;
			double number = 0;
			
			while (length > 0 && input[offset] >= '0' && input[offset] <= '9')
			{
				
				number *= 10;
				number += input[offset] - '0';
				offset += 1;
				length -= 1;
			}
			numberLength = offset - startOffset;
			return number;
		}
		
		/// <summary>Skip white space.  This routine skips with space in
		/// the input and returns the number of character skipped.
		/// White space is defined as ' ', '\t', '\n' or '\r'
		/// *
		/// </summary>
		/// <param name="length">The maximum number of characters to skip.
		/// 
		/// </param>
		public virtual int skipWhite(int length)
		{
			int i;
			for (i = 0; i < length; i += 1)
			{
				if (input[offset + i] != ' ' && input[offset + i] != '\t' && input[offset + i] != '\n' && input[offset + i] != '\r')
				{
					break;
				}
			}
			
			offset += i;
			return i;
		}
		
		/// <summary>Find the sign for a number .
		/// This routine looks for a sign (+/-) at the current location
		/// and return +1/-1 if one is found, or +1 if not.
		/// The foundSign boolean is set if a sign is found and offset is
		/// incremented.
		/// </summary>
		private int checkSign()
		{
			
			foundSign = false;
			
			if (input[offset] == '+')
			{
				foundSign = true;
				offset += 1;
				return 1;
			}
			else if (input[offset] == '-')
			{
				foundSign = true;
				offset += 1;
				return - 1;
			}
			
			return 1;
		}
		
		
		/// <summary>Is a region blank?
		/// </summary>
		/// <param name="length">The length of the region to be tested
		/// 
		/// </param>
		private bool isWhite(int length)
		{
			int oldOffset = offset;
			bool value_Renamed = skipWhite(length) == length;
			offset = oldOffset;
			return value_Renamed;
		}
	}
}