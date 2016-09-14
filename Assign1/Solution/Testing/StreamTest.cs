using System;
using System.IO;
using NUnit.Framework;
using CS422;
using System.Diagnostics;

namespace Test
{
    [TestFixture ()]
    public class StreamTest
    {
        [Test ()]
        public void TestMethod_Constructor()
        {
            using (var stream = new IndexedNumsStream (long.MaxValue)) 
            {
                Assert.AreEqual (long.MaxValue, stream.Length);
                Assert.AreEqual (0, stream.Position);
            }

            using (var stream = new IndexedNumsStream (long.MinValue)) 
            {
                Assert.AreEqual (0, stream.Position);
                Assert.AreEqual (0, stream.Length);
            }   
        }

        [Test ()]
        public void TestMethod_Basic()
        {
            using (var stream = new IndexedNumsStream(int.MaxValue))
            {
                byte[] buffer = new byte[int.MaxValue];

                stream.Read (buffer, 1898070800, 1898071050);
                Assert.AreEqual (1898070809 % 256, buffer[1898070809]);

                Assert.AreEqual (int.MaxValue, stream.Length);

                Assert.AreEqual (int.MaxValue, stream.Length);
                stream.Read(buffer, 0, 512);

                Assert.AreEqual(0, buffer[0]);
                Assert.AreEqual(1, buffer[1]);
                Assert.AreEqual(15, buffer[15]);
                Assert.AreEqual(100, buffer[100]);
                Assert.AreEqual(255, buffer[255]);

                /* Checks for values in range of 256-511 (expected should be the same as above) */
                Assert.AreEqual(0, buffer[256]);
                Assert.AreEqual(1, buffer[257]);
                Assert.AreEqual(15, buffer[271]);
                Assert.AreEqual(100, buffer[356]);
                Assert.AreEqual(255, buffer[511]);

                stream.Read (buffer, 249999999, 300000001);
                for (int i = 2500000; i < 3000000; i++) 
                {
                    Assert.AreEqual (i % 256, buffer [i]);
                }

                stream.SetLength (512);
                Assert.AreEqual (512, stream.Length);
                Assert.AreEqual (512, stream.Position);
            }
        }

        [Test ()]
        public void TestMethod_PositionAndLengthBounds()
        {
            // Initialization of stream with negative values should automatically set each to 0.
            using (var stream = new IndexedNumsStream(-1))
            {
                Assert.AreEqual(0, stream.Length); // Length is set back to zero first.
                Assert.AreEqual(0, stream.Position); // Position is set to 0 (if less than zero).

                stream.SetLength(512); // Changing the length
                Assert.AreEqual(512, stream.Length);

                stream.Position = 513; // The position cannot succeed the length.
                Assert.AreEqual(512, stream.Position); // Position is set to the length.

                stream.Position = long.MinValue;
                Assert.AreEqual (0, stream.Position);

                stream.Position = 512;
                Assert.AreEqual (stream.Length, stream.Position);
            }

            // Initialization with lowest-low bound.
            using (var stream = new IndexedNumsStream(long.MinValue))
            {
                Assert.AreEqual(0, stream.Length);
                Assert.AreEqual(0, stream.Position);
            }
        }

        [Test ()]
        public void TestMethod_ReadingAtBounds()
        {
            using (var stream = new IndexedNumsStream(100))
            {
                var buffer = new byte[512];

                // Attempt to read beyond the stream's length.
                int read = stream.Read(buffer, 80, 21);
                Assert.AreEqual(20, read);
                Assert.AreEqual(100, stream.Position);

                // Change position but nothing is read.
                read = stream.Read(buffer, 60, 0);
                Assert.AreEqual(0, read);
                Assert.AreEqual(60, stream.Position);

                try
                {
                    stream.Read(buffer, -1, 0);
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Assert.AreEqual("offset", ex.ParamName);
                }

                try
                {
                    stream.Read(buffer, 0, -1);
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Assert.AreEqual("count", ex.ParamName);
                }

                try
                {
                    byte[] nullBuff = null;
                    stream.Read(nullBuff, 0, 0);
                    Assert.Fail();
                }
                catch (ArgumentNullException ex)
                {
                    Assert.AreEqual("buffer", ex.ParamName);
                }

                try
                {
                    byte[] tempBuff = new byte[64];
                    stream.Read(tempBuff, 50, 50);
                    Assert.Fail();
                }
                catch(ArgumentException) { }
            }   
        }

        [Test ()]
        public void TestMethod_Seek()
        {
            using (var stream = new IndexedNumsStream(512))
            {
                /* From beginning: */
                stream.Seek(1000, SeekOrigin.Begin);
                Assert.AreEqual(stream.Length, stream.Position);
                stream.Seek(-1, SeekOrigin.Begin);
                Assert.AreEqual(0, stream.Position);
                stream.Seek(300, SeekOrigin.Begin);
                Assert.AreEqual(300, stream.Position);

                /* From current position: */
                stream.Seek(-150, SeekOrigin.Current);
                Assert.AreEqual(150, stream.Position);
                stream.Seek(1000, SeekOrigin.Current);
                Assert.AreEqual(stream.Length, stream.Position);
                stream.Seek(-1000, SeekOrigin.Current);
                Assert.AreEqual(0, stream.Position);

                /* From end position: */
                stream.Seek(50, SeekOrigin.End);
                Assert.AreEqual(stream.Length, stream.Position);
                stream.Seek(-150, SeekOrigin.End);
                Assert.AreEqual(362, stream.Position);
                stream.Seek(-1000, SeekOrigin.End);
                Assert.AreEqual(0, stream.Position);
            }
        }

        [Test ()]
        public void TestMethod_PropertyAccessors()
        {
            using (var stream = new IndexedNumsStream (512)) 
            {
                Assert.AreEqual (true, stream.CanRead);
                Assert.AreEqual (true, stream.CanSeek);
                Assert.AreEqual (false, stream.CanWrite);
                Assert.AreEqual (512, stream.Length);
                Assert.AreEqual (0, stream.Position);
            }

            using (var stream = new IndexedNumsStream(int.MinValue))
            {
                Assert.AreEqual(0, stream.Length);
                Assert.AreEqual(0, stream.Position);
            }
        }

        [Test ()]
        public void TestMethod_BytesRead()
        {
            using (var stream = new IndexedNumsStream (512)) 
            {
                var buffer = new byte[1024];

                int read = stream.Read (buffer, 0, 50);
                Assert.AreEqual (50, read);

                read = stream.Read (buffer, 450, 63);
                Assert.AreEqual (62, read);
            }
        }
    }
}
