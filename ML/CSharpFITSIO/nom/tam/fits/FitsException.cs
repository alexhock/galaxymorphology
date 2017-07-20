using System;

/*
* Copyright: Thomas McGlynn 1997-1999.
* This code may be used for any purpose, non-commercial
* or commercial so long as this copyright notice is retained
* in the source code or included in or referred to in any
* derived software.
* Many thanks to David Glowacki (U. Wisconsin) for substantial
* improvements, enhancements and bug fixes.
*/


public class FitsException:Exception
{
	public FitsException():base()
	{
	}
	
	public FitsException(String msg):base(msg)
	{
	}
}
