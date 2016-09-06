using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS422
{
  public class ThreadPoolSleepSorter : IDisposable
  {
    private ManualResetEvent mre = new ManualResetEvent(false);
    private TextWriter _writer;
    private BlockingCollection<SleepSortTask> _taskCollection = new BlockingCollection<SleepSortTask>();
    private List<Thread> _pool;

    public ThreadPoolSleepSorter(TextWriter output, ushort threadCount)
    {
      if (output == null)
      {
        throw new ArgumentNullException("output");
      }
      _writer = output;

      if (threadCount <= 0)
      {
        threadCount = 64;
      }

      _pool = new List<Thread>();

      for (int i = 0; i < threadCount; i++)
      {
        mre.Wa
        Thread thread = new Thread();
        _pool.Add(thread.Start());
      }
    }

    public void Sort(byte[] values)
    {
      
    }

    public void Dispose()
    {
      throw new NotImplementedException();
    }
  }

  internal class SleepSortTask
  {
    public int Value { get; set; }

    public TextWriter Writer { get; set; }

    public static void Execute(TextWriter writer, int value)
    {
      writer.WriteLine(value);
    }
  }
}
