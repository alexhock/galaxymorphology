using System;
using System.Collections;

namespace nom.tam.util
{
	/// <summary>
	/// Summary description for DefaultValueHashtable.
	/// </summary>
  public class DefaultValueHashtable : Hashtable
  {
    public override Object this[Object key]
    {
      get
      {
        _result = base[key];
        if(_result == null)
        {
          _result = DefaultValue;
        }

        return _result;
      }

      set
      {
        if(key == null)
        {
          DefaultValue = value;
        }
        else
        {
          base[key] = value;
        }
      }
    }

    public Object DefaultValue
    {
      get
      {
        return _defaultValue;
      }
      set
      {
        _defaultValue = value;
      }
    }

    public DefaultValueHashtable() : this(null)
    {
    }

    public DefaultValueHashtable(Object defaultValue) : base()
    {
      DefaultValue = defaultValue;
    }

    protected Object _result = null;
    protected Object _defaultValue = null;
  }
}
