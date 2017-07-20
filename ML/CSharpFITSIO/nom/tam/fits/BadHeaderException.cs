namespace nom.tam.fits
{
	using System;
	
	/*
	* Copyright: Thomas McGlynn 1997-1998.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	*
	* Many thanks to David Glowacki (U. Wisconsin) for substantial
	* improvements, enhancements and bug fixes.
	*/
	
	/// <summary>This exception indicates that an error
	/// was detected while parsing a FITS header record.
	/// </summary>
	public class BadHeaderException:FitsException
	{
		public BadHeaderException():base()
		{
		}
		public BadHeaderException(String msg):base(msg)
		{
		}
	}
}
