using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CS422;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
  [TestClass]
  public class StreamTest
  {
    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
    public void TestMethodReadingAtBounds()
    {
      using (var stream = new IndexedNumsStream(0, 100))
      {
        var buffer = new byte[100];

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


        //read = stream.Read(buffer, -30, 0);
        //Assert.(0, read);
        //Assert.AreEqual(30, stream.Position);
      }
    }
  }
}
