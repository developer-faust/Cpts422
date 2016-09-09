using System;
using System.Collections.Generic;
using CS422;
using System.Threading;

namespace TestConsole
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var sorter = new ThreadPoolSleepSorter(Console.Out, 20);

            Random rand = new Random();

            List<byte> toSort = new List<byte>();

            for (int i = 0; i < 20; i++)
            {
                toSort.Add((byte)rand.Next (0, 20));
            }

            sorter.Sort(toSort.ToArray());

            Thread.Sleep(3000);
            sorter.Dispose();

            sorter.Sort(toSort.ToArray());
        }
    }
}
