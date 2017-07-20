using System;

namespace nom.tam.util
{
	/// <summary>
	///   To replace dumbass C# non-nullable bool struct.
	/// </summary>
	public class Troolean
	{
		public Troolean() : this(false, false)
		{
		}

    public Troolean(bool val) : this(val, false)
    {
    }

    public Troolean(bool val, bool isNull)
    {
      _val = val;
      _isNull = isNull;
    }

    public bool Val
    {
      get
      {
        return _val;
      }
      set
      {
        _val = value;
      }
    }

    public bool IsNull
    {
      get
      {
        return _isNull;
      }
      set
      {
        _isNull = value;
      }
    }

    protected bool _val;
    protected bool _isNull;
	}
}
