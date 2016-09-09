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
        public void TestCase_SingleThread()
        {
            PCQueue queue = new PCQueue();

            for (int i = 0; i < 10; i++)
            {
                queue.Enqueue(i);
            }


            int outVal = -1;
            for (int i = 0; i < 10; i++)
            {
                queue.Dequeue(ref outVal);
                Assert.AreEqual(outVal, i);
            }

            bool outcome = queue.Dequeue(ref outVal);
            Assert.AreEqual(false, outcome);

            queue.Enqueue(100);
            queue.Dequeue(ref outVal);
            Assert.AreEqual(100, outVal);
        }

		[Test ()]
		public void TestCase_Basic ()
		{
			PCQueue queue = new PCQueue ();
            int[] aIn = new int[10000000];
            int[] aOut = new int[10000000];

            for (int i = 0; i < 10000000; i++)
            {
                aIn[i] = i;
            }

			Thread enqueue = new Thread (() => 
			{
                    foreach(var value in aIn)
                    {
                        queue.Enqueue(value);
                    }
			});

			Thread dequeue = new Thread (() => 
			{
                    int value = -1;
                    for (int i = 0; i < 10000000; i++) 
    				{
                        while (!queue.Dequeue (ref value))
                        {
                            
                        }

                        aOut[i] = value;
    				}
                    Assert.AreEqual(false, queue.Dequeue(ref value));
			});
				
			enqueue.Start ();
			dequeue.Start ();

            enqueue.Join();
            dequeue.Join();

            for(int i = 0; i < 10000000; i++)
            {
                Assert.AreEqual(aIn[i], aOut[i]);
            }
		}
	}
}

