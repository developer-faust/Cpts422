using System;
using System.Collections.Generic;
using NUnit.Framework;
using CS422;
using System.IO;

namespace Test
{
	[TestFixture ()]
	public class SleepSortTest
	{
		[Test ()]
		public void SleepSortTest_Basic ()
		{
            using (var writer = new StringWriter())
            using (var sorter = new ThreadPoolSleepSorter (writer, 100)) 
			{
                Random rand = new Random();

                List<byte> toSort = new List<byte>();

                for (int i = 0; i < 100; i++)
                {
                    toSort.Add((byte)rand.Next (0, 256));
                }

                sorter.Sort(toSort.ToArray());

                string s = writer.ToString();
                Console.Write(s);
			}
		}
	}
}

