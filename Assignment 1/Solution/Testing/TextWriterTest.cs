using System;
using System.IO;
using NUnit;
using NUnit.Framework;
using CS422;

namespace Test
{
    [TestFixture ()]
    public class TextWriterTest
    {
	    [Test ()]
        public void TestMethod_Constructor()
        {
            string[] testStrings =
            {
                "Hello",
                "World."
            };
                
            /* Attempt to create a new NumberTextWriter object by passing in null for the wrapped TextWriter*/
            try
            {
#pragma warning disable CSO219
                var writer = new NumberedTextWriter (null);
#pragma warning restore CS0219
                Assert.Fail ();
            }
            catch(ArgumentNullException) { }

            /* Default constructor test */
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

            /* Alternate constructor test */
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

            using (var stringWriter = new StringWriter())
            using (var writer = new NumberedTextWriter(stringWriter, -1))
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

	    [Test ()]
        public void TestMethod_EmptyStrings()
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

	    [Test ()]
        public void TestMethod_NullStrings()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new NumberedTextWriter(stringWriter))
            {
                string value = null;
                for (int i = 0; i < 5; i++)
                {
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

    	[Test ()]
        public void TestMethod_WriteVsWriteLine()
        {
            using (var stringWriter = new StringWriter ())
            using (var writer = new NumberedTextWriter (stringWriter)) 
            {
            
            }
        }

        [Test ()]
        public void TestMethod_Disposing()
        {
            using (var stringWriter = new StringWriter ()) 
            {
                var writer = new NumberedTextWriter (stringWriter);

                writer.Dispose ();

                try
                {
                    writer.WriteLine ();
                    Assert.Fail ();
                }
                catch(ObjectDisposedException) {
                }
            }
        }
    }
}
