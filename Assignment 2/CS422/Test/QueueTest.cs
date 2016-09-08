using NUnit.Framework;
using System;
using CS422;
using System.Threading;

namespace Test
{
	[TestFixture ()]
	public class QueueTest
	{
		[Test ()]
		public void TestCase_Basic ()
		{
			PCQueue queue = new PCQueue ();

			Thread enqueue = new Thread (() => 
			{
				for(int i = 0; i < 10000000; i++)
				{
					queue.Enqueue (i);
				}
			});

			Thread dequeue = new Thread (() => 
			{
				for (int i = 0; i < 10000000; i++) 
				{
					int test = -i;
					queue.Dequeue (ref test);
				}
			});
				
			enqueue.Start ();
			dequeue.Start ();
		}
	}
}

