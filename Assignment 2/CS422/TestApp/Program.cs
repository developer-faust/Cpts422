using System;
using CS422;

namespace TestApp
{
  class Program
  {
    static void Main(string[] args)
    {
      using (var sorter = new ThreadPoolSleepSorter(Console.Out, 100))
      {
        Random rand = new Random();
        byte[] toSort = new  byte[5];

        for (int i = 0; i < 5; i++)
        {
          toSort[i] =(byte)rand.Next(0, 10);
        }

        sorter.Sort(toSort);

        Console.ReadKey();

        for (int i = 0; i < 5; i++)
        {
          toSort[i] = (byte)rand.Next(0, 10);
        }

        sorter.Sort(toSort);
      }
    }
  }
}
