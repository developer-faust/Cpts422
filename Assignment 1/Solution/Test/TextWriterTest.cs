using System;
using System.IO;
using CS422;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
  [TestClass]
  public class TextWriterTest
  {
    [TestMethod]
    public void TestMethodDefaultCstr()
    {
      string[] testStrings =
      {
        "Hello",
        "World."
      };

      using (var stringWriter = new StringWriter())
      using (var writer = new NumberedTextWriter(stringWriter))
      {
        foreach(var testString in testStrings)
        {
          writer.WriteLine(testString);
        }

        Assert.AreEqual(
          "1: Hello" + Environment.NewLine + "2: World." + Environment.NewLine,
          stringWriter.ToString());
      }
    }

    [TestMethod]
    public void TestMethodAltCstr()
    {
      string[] testStrings =
      {
        "Hello",
        "World."
      };

      using (var stringWriter = new StringWriter())
      using (var writer = new NumberedTextWriter(stringWriter, 10))
      {
        foreach (var testString in testStrings)
        {
          writer.WriteLine(testString);
        }

        Assert.AreEqual(
          "10: Hello" + Environment.NewLine + "11: World." + Environment.NewLine,
          stringWriter.ToString());
      }
    }

    [TestMethod]
    public void TestMethodEmptyStrings()
    {
      using (var stringWriter = new StringWriter())
      using (var writer = new NumberedTextWriter(stringWriter))
      {
        for (int i = 0; i < 5; i++)
        {
          writer.WriteLine(string.Empty);
        }

        Assert.AreEqual(
          "1: " + Environment.NewLine + 
          "2: " + Environment.NewLine + 
          "3: " + Environment.NewLine + 
          "4: " + Environment.NewLine + 
          "5: " + Environment.NewLine,
          stringWriter.ToString());
      }
    }

    [TestMethod]
    public void TestMethodNullStrings()
    {
      using (var stringWriter = new StringWriter())
      using (var writer = new NumberedTextWriter(stringWriter))
      {
        string value = null;
        for (int i = 0; i < 5; i++)
        {
          // ReSharper disable once ExpressionIsAlwaysNull
          writer.WriteLine(value);
        }

        Assert.AreEqual(
          "1: " + Environment.NewLine +
          "2: " + Environment.NewLine +
          "3: " + Environment.NewLine +
          "4: " + Environment.NewLine +
          "5: " + Environment.NewLine,
          stringWriter.ToString());
      }
    }

    [TestMethod]
    public void TestMethodWriteVsWriteLine()
    {
      using (var stringWriter = new StringWriter())
      using (var writer = new NumberedTextWriter(stringWriter))
      {
        
      }
    }
  }
}
