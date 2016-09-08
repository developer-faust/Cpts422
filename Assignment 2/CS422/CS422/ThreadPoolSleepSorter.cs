/* Colin Phillips
 * CS 422 - Assignment 2
 * Fall 2016 */

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
    /// <summary>
    /// Thread pool sleep sorter.
    /// </summary>
	public class ThreadPoolSleepSorter : IDisposable
  	{
    	private TextWriter _writer;
        private BlockingCollection<SleepSortTask> _taskCollection = new BlockingCollection<SleepSortTask>();
		private bool _isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CS422.ThreadPoolSleepSorter"/> class.
        /// </summary>
        /// <param name="output">The textwriter object to write the sorted values to.</param>
        /// <param name="threadCount">The number of threads to use during the sorting process.</param>
        /// <exception cref="ArgumentNullException"/>
	    public ThreadPoolSleepSorter(TextWriter output, ushort threadCount)
	    {
			if (output == null)
		    {
				throw new ArgumentNullException("output");
		    }

		    _writer = output;

            threadCount = (threadCount <= 0) ? (ushort) 64 : threadCount;

		    for (int i = 0; i < threadCount; i++)
		    {
                Thread thread = new Thread(new ParameterizedThreadStart(DoThreadWork));
                thread.Start(_taskCollection);
		    }
	    }

        /// <summary>
        /// Print the specified values in sorted order.
        /// </summary>
        /// <param name="values">The values to sort.</param>
        /// <exception cref="ArgumentNullException"/>
	    public void Sort(byte[] values)
	    {
            if (values == null)
            {
                throw new ArgumentNullException("value");
            }

			foreach (var value in values) 
			{
                _taskCollection.Add(new SleepSortTask
                    {
                        Data = value
                    });
			}
	    }	

        /// <summary>
        /// Releases all resource used by the <see cref="CS422.ThreadPoolSleepSorter"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="CS422.ThreadPoolSleepSorter"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="CS422.ThreadPoolSleepSorter"/> in an unusable state.
        /// After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CS422.ThreadPoolSleepSorter"/> so the garbage collector can reclaim the memory that the
        /// <see cref="CS422.ThreadPoolSleepSorter"/> was occupying.</remarks>
	    public void Dispose()
	    {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("This object has already been disposed.");
            }

            _isDisposed = true;
            _taskCollection.Dispose();
	    }
            
        private void DoThreadWork(object collection)
		{
            var coll = (BlockingCollection<SleepSortTask>)collection;

			while (!_isDisposed) 
			{
                coll.Take ().Execute(_writer);
			}
		}
  	}

    internal class SleepSortTask
    {
        public byte Data { get; set; }

        public void Execute(TextWriter writer)
        {
            Thread.Sleep(Data * 1000);
            writer.WriteLine(Data);
        }
    }
}
