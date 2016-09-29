using System;
using System.IO;
using CS422;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            byte[] buf1 = {1, 2, 3, 4, 5, 6, 7, 8};
            byte[] buf2 = { 10, 20, 30, 40, 50, 60, 70, 80 };

            using (MemoryStream mem1 = new MemoryStream(buf1))
            using (MemoryStream mem2 = new MemoryStream(buf2))
            using (ConcatStream stream = new ConcatStream(mem1, mem2))
            {
                byte[] buf = new byte[4096];

                stream.Read(buf, 0, 10);

                foreach (var value in buf)
                {
                    Console.WriteLine(value);
                }
            }
        }
    }
}
