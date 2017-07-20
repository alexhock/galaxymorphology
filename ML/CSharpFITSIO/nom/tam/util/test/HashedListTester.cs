namespace nom.tam.util.test
{
	/* Copyright: Thomas McGlynn 1999.
	* This code may be used for any purpose, non-commercial
	* or commercial so long as this copyright notice is retained
	* in the source code or included in or referred to in any
	* derived software.
	*/
	using System;
	using nom.tam.util;
	/// <summary>This class tests and illustrates the use
	/// of the HashedList class.  Tests are in three
	/// parts.  
	/// <p>
	/// The first section tests the methods
	/// that are present in the Collection interface.
	/// All of the optional methods of that interface
	/// are supported.  This involves tests of the
	/// HashedClass interface directly.
	/// <p>
	/// The second set of tests uses the Cursor
	/// returned by the GetCursor() method and tests
	/// the standard Cursor methods to display
	/// and remove rows from the HashedList.
	/// <p>
	/// The third set of tests tests the extended
	/// capabilities of the HashedListCursor
	/// to add rows to the table, and to work
	/// as a cursor to move in a non-linear fashion
	/// through the list.
	/// <p>
	/// There is as yet no testing that the HashedList
	/// fails appropriately and gracefully.
	/// 
	/// </summary>
	public class HashedListTester
	{
		
		[STAThread]
		public static void Test(System.String[] args)
		{
			HashedList h1 = new HashedList();
			HashedList h2 = new HashedList();
			
			Cursor i = h1.GetCursor(0);
			Cursor j;
			
			// Add a few unkeyed rows.
			
			h1.Add("Row 1");
			h1.Add("Row 2");
			h1.Add("Row 3");

			System.Console.Out.WriteLine("***** Collection methods *****\n");
			show("Three unkeyed elements", h1);
			h1.RemoveUnkeyedObject("Row 2");
			show("Did we remove Row 2?", h1);
			h1.Clear();
			
			show("Cleared", h1);
			h1.Add("key 1", "Row 1");
			h1.Add("key 2", "Row 2");
			h1.Add("key 3", "Row 3");
			
			show("Three keyed elements", h1);
//			h1.RemoveKey("key 2");
      h1.Remove("key 2");
			show("Did we remove Row 2 using a key?", h1);
			h1.Clear();
			show("Cleared", h1);

			h1.Add("key 1", "Row 1");
			h1.Add("key 2", "Row 2");
			h1.Add("key 3", "Row 3");
			show("Three elements again!", h1);
			System.Console.Out.WriteLine("Check contains (true):" + h1.ContainsValue("Row 2"));
			
			
			h2.Add("key 4", "Row 4");
			h2.Add("key 5", "Row 5");
			
//			System.Console.Out.WriteLine("Check containsAll (false):" + h1.containsAll(h2));
			
//			h1.addAll(h2);
//			show("Should have 5 elements now", h1);
//			System.Console.Out.WriteLine("Check containsAll (true):" + h1.containsAll(h2));
//			System.Console.Out.WriteLine("Check contains (true):" + h1.Contains("Row 4"));
			
//			h1.RemoveUnkeyedObject("Row 4");
//			show("Dropped Row 4:", h1);
//			System.Console.Out.WriteLine("Check containsAll (false):" + h1.containsAll(h2));
			System.Console.Out.WriteLine("Check contains (false):" + h1.Contains("Row 4"));
			
			System.Console.Out.WriteLine("Check isEmpty (false):" + h1.Empty);
			h1.RemoveValue("Row 1");
			h1.RemoveValue("Row 2");
			h1.RemoveValue("Row 3");
			h1.RemoveValue("Row 5");
			show("Removed all elements", h1);
			System.Console.Out.WriteLine("Check isEmpty (true):" + h1.Empty);
			h1.Add("Row 1");
			h1.Add("Row 2");
			h1.Add("Row 3");
//			h1.addAll(h2);
//			show("Back to 5", h1);
//			h1.removeAll(h2);
//			show("Testing removeAll back to 3?", h1);
//			h1.addAll(h2);
//			h1.retainAll(h2);
//			show("Testing retainAll now just 2?", h1);
			System.Console.Out.WriteLine("\n\n**** Test Cursor **** \n");
			
			j = h1.GetCursor();
			while(j.MoveNext())
			{
				System.Console.Out.WriteLine("Cursor got:" + j.Current);
			}
			
			h1.Clear();
			h1.Add("key 1", "Row 1");
			h1.Add("key 2", "Row 2");
			h1.Add("Row 3");
			h1.Add("key 4", "Row 4");
			h1.Add("Row 5");
			j = h1.GetCursor();
			j.MoveNext();
			j.MoveNext();
			j.Remove(); // Should get rid of second row
			show("Removed second row with cursor", h1);
			System.Console.Out.WriteLine("Cursor should still be OK:" + j.MoveNext() + " " + j.Current);
			System.Console.Out.WriteLine("Cursor should still be OK:" + j.MoveNext() + " " + j.Current);
			System.Console.Out.WriteLine("Cursor should still be OK:" + j.MoveNext() + " " + j.Current);
			System.Console.Out.WriteLine("Cursor should be done:" + j.MoveNext());
			
			System.Console.Out.WriteLine("\n\n**** HashedListCursor ****\n");
			i = h1.GetCursor(0);
			System.Console.Out.WriteLine("Cursor should still be OK:" + i.MoveNext() + " " + i.Current);
			System.Console.Out.WriteLine("Cursor should still be OK:" + i.MoveNext() + " " + i.Current);
			System.Console.Out.WriteLine("Cursor should still be OK:" + i.MoveNext() + " " + i.Current);
			System.Console.Out.WriteLine("Cursor should still be OK:" + i.MoveNext() + " " + i.Current);
			System.Console.Out.WriteLine("Cursor should be done:" + i.MoveNext());
			
			i.Key = "key 1";
//			i.Current;
			i.Add("key 2", "Row 2");
      System.Console.Out.WriteLine("Cursor should still be OK:" + i.MoveNext() + " " + i.Current);
			System.Console.Out.WriteLine("Cursor should still be OK:" + i.MoveNext() + " " + i.Current);
			System.Console.Out.WriteLine("Cursor should still be OK:" + i.MoveNext() + " " + i.Current);
			System.Console.Out.WriteLine("Cursor should be done:" + i.MoveNext());

      i.Key = "key 4";
      System.Console.Out.WriteLine("Cursor should still be OK:" + i.MoveNext() + " " + i.Current);
//			System.Console.Out.WriteLine("Cursor should still be OK:" + i.MoveNext() + " " + i.Current);
      System.Console.Out.WriteLine("Cursor should be done:" + i.MoveNext());
			
			i.Key = "key 2";
			i.MoveNext();
			i.MoveNext();
			i.Add("Row 3.5");
			i.Add("Row 3.6");
			show("Added some rows... should be 7", h1);
			
			i = h1.GetCursor("key 2");
			i.Add("Row 1.5");
			i.Add("key 1.7", "Row 1.7");
			i.Add("Row 1.9");
			System.Console.Out.WriteLine("Cursor should point to 2:" + ((System.Collections.DictionaryEntry)i.Current).Key);
			i.Key = "key 1.7";
			System.Console.Out.WriteLine("Cursor should point to 1.7:" + ((System.Collections.DictionaryEntry)i.Current).Key);
 		}
		public static void  show(System.String descrip, HashedList h)
		{
			
			System.Console.Out.WriteLine(descrip + " :  " + h.Count);
//			Object[] o = h.toArray();
//			for (int i = 0; i < o.Length; i += 1)
//			{
//				System.Console.Out.WriteLine("  " + o[i]);
//			}
		}
	}
}