using System;
using System.Collections.Generic;
using NUnit.Framework;
using CS422;
using System.IO;
using System.Threading;

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

                for (int i = 0; i < 20; i++)
                {
                    toSort.Add((byte)rand.Next (0, 256));
                }

                sorter.Sort(toSort.ToArray());
                Thread.Sleep(5000);

                string s = writer.ToString();
                string[] sList = s.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                for (int i = 0; i < sList.Length - 1; i++)
                {
                    if (sList[i] == string.Empty || sList[i + 1] == string.Empty)
                        continue;
                    
                    Assert.IsFalse(Byte.Parse(sList[i]) > Byte.Parse(sList[i + 1]));
                }

                sorter.Sort(toSort.ToArray());
                Thread.Sleep(5000);

                s = writer.ToString();
                sList = s.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                for (int i = 0; i < sList.Length - 1; i++)
                {
                    if (sList[i] == string.Empty || sList[i + 1] == string.Empty)
                        continue;

                    Assert.IsFalse(Byte.Parse(sList[i]) > Byte.Parse(sList[i + 1]));
                }
			}
		}
	}
}

