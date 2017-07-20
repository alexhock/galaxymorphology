using System;
using System.IO;

namespace nom.tam.util
{
  /* Copyright: Thomas McGlynn 1997-1999.
  * This code may be used for any purpose, non-commercial
  * or commercial so long as this copyright notice is retained
  * in the source code or included in or referred to in any
  * derived software.
  */

  /// <summary>Summary description for BufferedFile.</summary>
  //public class BufferedFile : BufferedDataStream, RandomAccess
  public class BufferedFile : BufferedDataStream
  {
    #region Constructors
    /// <summary>Create a read-only buffered file</summary>
    public BufferedFile(String filename):this(filename, FileAccess.Read, 32768)
    {
    }
		
    /// <summary>Create a buffered file with the given mode.</summary>
    /// <param name="filename">The file to be accessed.</param>
    /// <param name="access">Read/write</param>
    public BufferedFile(String filename, FileAccess access):this(filename, access, 32768)
    {
    }
		
    /// <summary>Create a buffered file with the given mode and a specified
    /// buffer size.
    /// </summary>
    /// <param name="filename">The file to be accessed.</param>
    /// <param name="mode">Read/write</param>
    /// <param name="buffer">The buffer size to be used. This should be
    /// substantially larger than 100 bytes and
    /// defaults to 32768 bytes in the other
    /// constructors.</param>
    public BufferedFile(String filename, FileAccess access, int bufferSize):base(new FileStream(filename, FileMode.OpenOrCreate, access, FileShare.Read, bufferSize))
    {
    }
    #endregion

    #region RandomAccess Members
    /// <summary>Get the current position in the stream</summary>
    public long FilePointer
    {
      get
      {
        return _s.Position;
      }
    }
    #endregion
  }
}
