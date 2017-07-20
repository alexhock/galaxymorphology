// Member of the utility package.
namespace nom.tam.util
{
	/* Copyright: Thomas McGlynn 1997-1998.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	*/
	using System;
  using System.Collections;
	/// <summary>This is a package of static functions which perform
	/// computations on arrays.  Generally these routines attempt
	/// to complete without throwing errors by ignoring data
	/// they cannot understand.
	/// </summary>
	
	public class ArrayFuncs : PrimitiveInfo
	{
    public static int CountDimensions(Object o)
    {
      if(o == null || !o.GetType().IsArray)
      {
        return 0;
      }
      else
      {
        if(((Array)o).Rank > 1)
        {
          return ((Array)o).Rank;
        }
        else
        {
          return 1 + CountDimensions(((Array)o).GetValue(0));
        }
      }
    }

    public static bool IsArrayOfArrays(Object o)
    {
      if(o == null || !o.GetType().IsArray)
      {
        return false;
      }

      if(((Array)o).GetValue(0) != null && ((Array)o).GetValue(0).GetType().IsArray)
      {
        return true;
      }

      return !(!o.GetType().IsArray || ((Array)o).Rank > 1);
    }

		/// <summary>Compute the size of an object.  Note that this only handles
		/// arrays or scalars of the primitive objects and Strings.  It
		/// returns 0 for any object array element it does not understand.</summary>
		/// <param name="o">The object whose size is desired.</param>
		public static int ComputeSize(Object o)
		{
      int result = 0;

      if(o != null)
      {
        Type t = o.GetType();
        if(t.IsArray)
        {
          int size = 0;
        
          for(IEnumerator i = ((Array)o).GetEnumerator(); i.MoveNext();)
          {
            size += ComputeSize(i.Current);
          }

          //return size;
          result = size;
        }
        else if(t == typeof(String))
        {
          //return ((String)o).Length * (int)sizes[typeof(char)];
          result = ((String)o).Length * (int)sizes[typeof(char)];
        }
        else if(t == typeof(Troolean))
        {
          result = 1;
        }
        else if(t.IsPrimitive)
        {
          //return (int)sizes[t];
          result = (int)sizes[t];
        }
        else
        {
          //return 0;
          result = 0;
        }
      }

      return result;
		}

    /// <summary>Count the number of elements in an array</summary>
		public static int CountElements(Object o)
		{
      int result = 0;

      if(o.GetType().IsArray)
      {
        if(IsArrayOfArrays(o))
        {
          for(IEnumerator i = ((Array)o).GetEnumerator(); i.MoveNext();)
          {
            result += CountElements(i.Current);
          }
        }
        else
        {
          result = ((Array)o).Length;
        }
      }
      else
      {
        result = 1;
      }

      return result;
		}
		

    /// <summary>Try to create a deep clone of an Array or a standard clone of a scalar.
		/// The object may comprise arrays of any primitive type or any Object type which
		/// implements Cloneable.  However, if the Object is some kind of collection,
		/// e.g., a Vector then only a shallow copy of that object is made.  I.e., deep
		/// refers only to arrays.</summary>
		/// <param name="o">The object to be copied.</param>
		public static System.Object DeepClone(System.Object o)
		{
			if(o == null)
			{
				return null;
			}
			if(!o.GetType().IsArray)
			{
				return GenericClone(o);
			}

      Array a = (Array)o;
      if(ArrayFuncs.IsArrayOfArrays(o))
      {
        Array result = NewInstance(o.GetType().GetElementType(), a.Length);
        for(int i = 0; i < result.Length; ++i)
        {
          result.SetValue(DeepClone(a.GetValue(i)), i);
        }

        return result;
      }
      else
      {
        int[] lengths = new int[a.Rank];
        for(int i = 0; i < lengths.Length; ++i)
        {
          lengths[i] = a.GetLength(i);
        }
        Array result = ArrayFuncs.NewInstance(o.GetType().GetElementType(), lengths);
        Array.Copy(a, result, a.Length);

        return result;
      }
		}

		/// <summary>Clone an Object if possible.
		/// *
		/// This method returns an Object which is a clone of the
		/// input object.  It checks if the method implements the
		/// Cloneable interface and then uses reflection to invoke
		/// the clone method.  This can't be done directly since
		/// as far as the compiler is concerned the clone method for
		/// Object is protected and someone could implement Cloneable but
		/// leave the clone method protected.  The cloning can fail in a
		/// variety of ways which are trapped so that it returns null instead.
		/// This method will generally create a shallow clone.  If you
		/// wish a deep copy of an array the method DeepClone should be used.
		/// *
		/// </summary>
		/// <param name="o">The object to be cloned.
		/// 
		/// </param>
		public static Object GenericClone(Object o)
		{
      if(!(o is ICloneable))
			{
				return null;
			}

      return ((ICloneable)o).Clone();
		}


    /// <summary>Find the dimensions of an object.
		/// *
		/// This method returns an integer array with the dimensions
		/// of the object o which should usually be an array.
		/// *
		/// It returns an array of dimension 0 for scalar objects
		/// and it returns -1 for dimension which have not been allocated,
		/// e.g., int[][][] x = new int[100][][]; should return [100,-1,-1].
		/// *
		/// </summary>
		/// <param name="o">The object to get the dimensions of.</param>
		public static int[] GetDimensions(Object o)
		{			
			if(o == null)
			{
				return new int[0];
			}
			
      if(o.GetType().IsArray)
      {
        if(IsArrayOfArrays(o))
        {
          int ndim = 0;
          for(Object oPrime = o; oPrime is Array; ++ndim, oPrime = ((Array)oPrime).GetValue(0));
          int[] dimens = new int[ndim];
          for(int i = 0; i < ndim; i += 1)
          {
            dimens[i] = - 1; // So that we can distinguish a null from a 0 length.
          }
			
          for(int i = 0; i < ndim; i += 1)
          {
            dimens[i] = ((Array)o).Length;
            if(dimens[i] == 0)
            {
              return dimens;
            }
            if(i != ndim - 1)
            {
              o = ((Array)o).GetValue(0);
              if(o == null)
              {
                return dimens;
              }
            }
          }

          return dimens;
        }
        else
        {
          Array a = (Array)o;
          int ndim = a.Rank;
          int[] dimens = new int[ndim];
          for(int i = 0; i < ndim; i += 1)
          {
            dimens[i] = a.GetLength(i);
          }

          return dimens;
        }
      }
      else
      {
        return new int[]{1};
      }
		}
		

		/// <summary>This routine returns the base class of an object.  This is just
		/// the class of the object for non-arrays.</summary>
		public static Type GetBaseClass(Object o)
		{			
			if(o == null)
			{
				return Type.GetType("System.Void");
			}
      if(o.GetType().IsArray)
      {
        IEnumerator i = ((Array)o).GetEnumerator();
        i.MoveNext();

        return GetBaseClass(i.Current);
      }
      else
      {
				return o.GetType();
			}
		}


    /// <summary>This routine returns the size of the base element of an array.</summary>
		/// <param name="o">The array object whose base length is desired.</param>
		/// <returns>the size of the object in bytes, 0 if null, or -1 if not a primitive array.
		/// </returns>
		public static int GetBaseLength(Object o)
		{
			if(o == null)
			{
				return 0;
			}

      Type t = GetBaseClass(o);

      if(t.IsPrimitive)
      {
        return (int)sizes[t];
      }
      else
      {
        return -1;
      }
    }


    /// <summary>Generate a description of an array (presumed rectangular).</summary>
		/// <param name="o">The array to be described.</param>
		public static String ArrayDescription(Object o)
		{
      if(o == null)
      {
        return "null";
      }
      else if(o.GetType().IsArray)
      {
        return CountDimensions(o) + "D array of " + GetBaseClass(o).FullName;
      }
      else
      {
        return o.GetType().FullName;
      }
		}


    /// <summary>Given an array of arbitrary dimensionality return
		/// the array flattened into a single dimension.</summary>
		/// <param name="input">The input array.</param>
		public static Object Flatten(Object input)
		{
			int[] dimens = GetDimensions(input);
			if(dimens.Length <= 1)
			{
				return input;
			}
			int size = 1;
			for(int i = 0; i < dimens.Length; i += 1)
			{
				size *= dimens[i];
			}

			Array flat = NewInstance(GetBaseClass(input), size);

			if(size == 0)
			{
				return flat;
			}
			
			int offset = 0;
			DoFlatten((Array)input, flat, offset);

      return flat;
		}


		/// <summary>This routine does the actually flattening of multi-dimensional arrays.</summary>
		/// <param name="input"> The input array to be flattened.</param>
		/// <param name="output">The flattened array.</param>
		/// <param name="offset">The current offset within the output array.</param>
		/// <returns>The number of elements within the array.</returns>
		protected internal static int DoFlatten(Object input, Array output, int offset)
		{
      int result = 0;

      if(IsArrayOfArrays(input))
      {
        for(IEnumerator e = ((Array)input).GetEnumerator(); e.MoveNext();)
        {
          Array a = (Array)e.Current;
          result += DoFlatten(a, output, offset + result);
        }
      }
      else if(input.GetType().IsArray)
      {
        IEnumerator e = ((Array)input).GetEnumerator();
        for(int i = offset; e.MoveNext(); ++i)
        {
          output.SetValue(e.Current, i);
          ++result;
        }
      }
      else
      {
        output.SetValue(input, offset);
        result = 1;
      }

      return result;
		}

		/// <summary>Curl an input array up into a multi-dimensional array.</summary>
		/// <param name="input">The one dimensional array to be curled.</param>
		/// <param name="dimens">The desired dimensions</param>
		/// <returns>The curled array.</returns>
		public static Array Curl(Array input, int[] dimens)
		{
			if(CountDimensions(input) != 1)
			{
				throw new SystemException("Attempt to curl non-1D array");
			}
			
			int test = 1;
			for(int i = 0; i < dimens.Length; i += 1)
			{
				test *= dimens[i];
			}
			
			if(test != input.Length)
			{
				throw new SystemException("Curled array does not fit desired dimensions");
			}
			
			Array newArray = NewInstance(GetBaseClass(input), dimens);

      int[] index = new int[dimens.Length];
      index[index.Length - 1] = -1;
      for(int i = 0; i < index.Length - 1; ++i)
      {
        index[i] = 0;
      }
      for(IEnumerator i = input.GetEnumerator(); i.MoveNext() && NextIndex(index, dimens);)
      {
        newArray.SetValue(i.Current, index);
      }
//			int offset = 0;
			//DoCurl(input, newArray, dimens, offset);
			return newArray;
		}

    public static bool NextIndex(int[] index, int[] dims)
    {
      bool ok = false;

      for(int i = index.Length - 1; i >= 0 && !ok; --i)
      {
        index[i] += 1;
        if(index[i] < dims[i])
        {
          ok = true;
        }
        else
        {
          index[i] = 0;
        }
      }

      return ok;
    }

    public static bool NextIndex(int[] index, int[] starts, int[] dims)
    {
      bool ok = false;

      for(int i = index.Length - 1; i >= 0 && !ok; --i)
      {
        index[i] += 1;
        if(index[i] < dims[i])
        {
          ok = true;
        }
        else
        {
          index[i] = starts[i];
        }
      }

      return ok;
    }

    /*
		/// <summary>Do the curling of the 1-d to multi-d array.</summary>
		/// <param name="input"> The 1-d array to be curled.</param>
		/// <param name="output">The multi-dimensional array to be filled.</param>
		/// <param name="dimens">The desired output dimensions.</param>
		/// <param name="offset">The current offset in the input array.</param>
		/// <returns>The number of elements curled.</returns>
		protected internal static int DoCurl(Array input, Array output, int[] dimens, int offset)
		{			
			if(dimens.Length == 1)
			{
				Array.Copy(input, offset, output, 0, dimens[0]);
				return dimens[0];
			}
			
			int total = 0;
			int[] xdimens = new int[dimens.Length - 1];
      Array.Copy(dimens, 1, xdimens, 0, xdimens.Length);
      for(int i = 0; i < dimens[0]; i += 1)
      {
        total += DoCurl(input, (Array)output.GetValue(i), xdimens, offset + total);
      }
			return total;
		}
*/

		/// <summary>Allocate an array dynamically. The Array.NewInstance method
		/// does not throw an error.</summary>
		/// <param name="cl	The">class of the array.</param>
		/// <param name="dim">The dimension of the array.</param>
		/// <returns> The allocated array.
		/// @throws An OutOfMemoryError if insufficient space is available.
		/// </returns>
		public static Array NewInstance(Type cl, int dim)
		{
			Array o = Array.CreateInstance(cl, dim);
			if(o == null)
			{
				String desc = cl + "[" + dim + "]";
				throw new System.OutOfMemoryException("Unable to allocate array: " + desc);
			}
			return o;
		}
		
		/// <summary>Allocate an array dynamically. The Array.NewInstance method
		/// does not throw an error.</summary>
		/// <param name="cl	The">class of the array.</param>
		/// <param name="dims">The dimensions of the array.</param>
		/// <returns>The allocated array.</returns>
		/// <throws>An OutOfMemoryError if insufficient space is available.</throws>
    public static Array NewInstance(Type cl, int[] dims)
    {
      return NewInstance(cl, dims, 0);
    }

		public static Array NewInstance(Type cl, int[] dims, int dim)
		{
      if(dim == dims.Length - 1)
      {
        return NewInstance(cl, dims[dim]);
      }
      else
      {
        Array a = new Array[dims[dim]];
        for(int i = 0; i < a.Length; ++i)
        {
          a.SetValue(NewInstance(cl, dims, dim + 1), i);
        }

        return a;
      }
/*
      Array o = Array.CreateInstance(cl, dims);
      if(o == null)
			{
				System.String desc = cl + "[";
				System.String comma = "";
				for(int i = 0; i < dims.Length; i += 1)
				{
					desc += comma + dims[i];
					comma = ",";
				}
				desc += "]";
				throw new System.OutOfMemoryException("Unable to allocate array: " + desc);
			}
			return o;
      */
		}

    #region Useless Crap
    /*
      /// <summary>This routine returns the base array of a multi-dimensional
      /// array.  I.e., a one-d array of whatever the array is composed
      /// of.  Note that arrays are not guaranteed to be rectangular,
      /// so this returns o[0][0]....
      /// </summary>
      public static Object GetBaseArray(Object o)
      {
        if(o.GetType().IsArray)
        {
          IEnumerator i = ((Array)o).GetEnumerator();
          i.MoveNext();
          return i.Current;
        }
      }
  */

    /*
      /// <summary>Create an array and populate it with a test pattern.
      /// *
      /// </summary>
      /// <param name="baseType"> The base type of the array.  This is expected to
      /// be a numeric type, but this is not checked.
      /// </param>
      /// <param name="dims">     The desired dimensions.
      /// </param>
      /// <returns> An array object populated with a simple test pattern.
      /// 
      /// </returns>
		
      public static System.Object generateArray(System.Type baseType, int[] dims)
      {
			
        // Generate an array and populate it with a test pattern of
        // data.
			
        System.Object x = ArrayFuncs.NewInstance(baseType, dims);
        testPattern(x, (sbyte) 0);
        return x;
      }
		
      /// <summary>Just create a simple pattern cycling through valid byte values.
      /// We use bytes because they can be cast to any other numeric type.
      /// </summary>
      /// <param name="o">     The array in which the test pattern is to be set.
      /// </param>
      /// <param name="start"> The value for the first element.
      /// 
      /// </param>
      public static sbyte testPattern(System.Object o, sbyte start)
      {
			
        int[] dims = getDimensions(o);
        if (dims.Length > 1)
        {
          for (int i = 0; i < ((System.Object[]) o).Length; i += 1)
          {
            start = testPattern(((System.Object[]) o)[i], start);
          }
        }
        else if (dims.Length == 1)
        {
          for (int i = 0; i < dims[0]; i += 1)
          {
            ((System.Array) o).SetValue(start, i);
            start = (sbyte) (start + 1);
          }
        }
        return start;
      }
      */
    /*
      /// <summary>Copy one array into another.
      /// This function copies the contents of one array
      /// into a previously allocated array.
      /// The arrays must agree in type and size.
      /// </summary>
      /// <param name="original">The array to be copied.
      /// </param>
      /// <param name="copy">    The array to be copied into.  This
      /// array must already be fully allocated.
      /// 
      /// </param>
      public static void  copyArray(System.Object original, System.Object copy)
      {
        System.String oname = original.GetType().FullName;
        System.String cname = copy.GetType().FullName;
			
        if(!original.GetType().IsArray() || !copy.GetType().IsArray ||
           !original.GetType().GetElementType().Equals(copy.GetType().GetElementType()) ||
           !original.GetType().GetArrayRank().Equals(copy.GetType().GetArrayRank()))
        {
          return ;
        }
			
        if(original.GetType().GetArrayRank() >= 2)
        {
          System.Object[] x = (System.Object[]) original;
          System.Object[] y = (System.Object[]) copy;
          for(int i = 0; i < x.Length; i += 1)
          {
            copyArray(x, y);
          }
        }
        else
        {
          Array.Copy(original, 0, copy, 0, ((Array)original).Length);
        }
      }
      */
    /*
      /// <summary>Create an array of a type given by new type with
      /// the dimensionality given in array.
      /// </summary>
      /// <param name="array">  A possibly multidimensional array to be converted.
      /// </param>
      /// <param name="newType">The desired output type.  This should be one of the
      /// class descriptors for primitive numeric data, e.g., double.type.
      /// 
      /// </param>		
      public static System.Object mimicArray(System.Object array, System.Type newType)
      {
        if(array == null || !array.GetType().IsArray)
        {
          return null;
        }
			
        int dims = array.GetType().GetArrayRank();			
			
        System.Object mimic;
			
        if(dims > 1)
        {
          System.Object[] xarray = (System.Object[]) array;
          int[] dimens = new int[dims];
          dimens[0] = xarray.Length; // Leave other dimensions at 0.

          mimic = ArrayFuncs.NewInstance(newType, dimens);

          for(int i = 0; i < xarray.Length; i += 1)
          {
            System.Object temp = mimicArray(xarray[i], newType);
            ((System.Object[]) mimic)[i] = temp;
          }
        }
        else
        {
          mimic = ArrayFuncs.NewInstance(newType, ((System.Array)array).Length);
        }

        return mimic;
      }		
  */
    #endregion
  }
}
