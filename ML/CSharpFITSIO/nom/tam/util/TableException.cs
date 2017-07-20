namespace nom.tam.util
{
	using System;
	/*
	* Copyright: Thomas McGlynn 1997-1998.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	*/
	
	public class TableException:Exception
	{
		
		public TableException():base()
		{
		}
		
		public TableException(String msg):base(msg)
		{
		}
	}
}