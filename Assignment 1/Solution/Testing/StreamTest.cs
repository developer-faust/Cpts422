using System;
using System.IO;
using NUnit.Framework;
using CS422;

namespace Test
{
    [TestFixture ()]
    public class StreamTest
    {
        [Test ()]
        public void TestMethodBasic()
        {
            using (var stream = new IndexedNumsStream(0, 512))
            {
                byte[] buffer = new byte[512];

                stream.Read(buffer, 0, 512);

                /* Checks for values in range of 0-255 */
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
            }
        }

        [Test ()]
        public void TestMethodPositionAndLengthBounds()
        {
            // Initialization of stream wil negative values should automatically set each to 0.
            using (var stream = new IndexedNumsStream(-1, -1))
            {
            Assert.AreEqual(0, stream.Length); // Length is set back to zero first.
            Assert.AreEqual(0, stream.Position); // Position is set to 0 (if less than zero).

            stream.SetLength(512); // Changing the length
            Assert.AreEqual(512, stream.Length);

            stream.Position = 513; // The position cannot succeed the length.
            Assert.AreEqual(512, stream.Position); // Position is set to the length.
            }

            // Initialization with lowest-low bound.
            using (var stream = new IndexedNumsStream(long.MinValue, long.MinValue))
            {
                Assert.AreEqual(0, stream.Length);
                Assert.AreEqual(0, stream.Position);
            }

            // Initialization with a position greater than the length
            using (var stream = new IndexedNumsStream(11, 10))
            {
                Assert.AreEqual(10, stream.Length);
                Assert.AreEqual(10, stream.Position);
            }
        }

        [Test ()]
        public void TestMethodReadingAtBounds()
        {
            using (var stream = new IndexedNumsStream(0, 100))
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
                    byte[] tempBuff = new byte[50];
                    stream.Read(tempBuff, 50, 50);
                    Assert.Fail();
                }
                catch(ArgumentException) 
                {
                    //
                }
            }   
        }

        [Test ()]
        public void TestMethodSeek()
        {
            using (var stream = new IndexedNumsStream(0, 512))
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
    }
}
