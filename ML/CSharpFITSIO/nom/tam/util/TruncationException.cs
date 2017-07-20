namespace nom.tam.util
{
	using System;
	
	public class TruncationException:Exception
	{
		
		public TruncationException():base()
		{
		}
		public TruncationException(String msg):base(msg)
		{
		}
	}
}