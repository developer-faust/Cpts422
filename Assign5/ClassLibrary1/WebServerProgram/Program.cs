using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CS422;

namespace WebServerProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            WebServer.Start(4220, 5);
            Thread.Sleep(1000);
            /*
            Thread t = new Thread(() =>
            {
                WebServer.Start(4220, 64);
            });
                    
            t.Start();*/

            string req =
                "GET / HTTP/1.1\r\nHost: localhost: 4220\r\nContent-Length: cool\r\nConnection: keep-alive\r\nCache-Control: max-age = 0\r\nUpgrade-Insecure-Requests: 1\r\nUser-Agent: Mozilla/5.0(Windows NT 10.0; WOW64) AppleWebKit/537.36(KHTML, like Gecko) Chrome/53.0.2785.116 Safari/537.36\r\nAccept: text/html,application/xhtml + xml,application/xml; q = 0.9,image/webp,*/*;q=0.8\r\nAccept-Encoding: gzip, deflate, sdch\r\nAccept-Language: en-US,en;q=0.8\r\n\r\n";
            
            byte[] buf;
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 4220);

            using (var ns = client.GetStream())
            {
                buf = Encoding.ASCII.GetBytes(req);
                ns.Write(buf, 0, buf.Length);
                int read = 0, total = 0;

                buf = new byte[4096];
                ns.Read(buf, total, buf.Length);

                Console.WriteLine(Encoding.ASCII.GetString(buf).TrimEnd('\0'));

                /*buf = Encoding.ASCII.GetBytes("G");
                ns.Write(buf, 0, buf.Length);
                //Thread.Sleep(1000);

                buf = Encoding.ASCII.GetBytes("E");
                ns.Write(buf, 0, buf.Length);
                //Thread.Sleep(1000);

                buf = Encoding.ASCII.GetBytes("T ");
                ns.Write(buf, 0, buf.Length);
                //Thread.Sleep(1000);

                buf = Encoding.ASCII.GetBytes("https://goog");
                ns.Write(buf, 0, buf.Length);
                //Thread.Sleep(1000);

                buf = Encoding.ASCII.GetBytes("le.com ");
                ns.Write(buf, 0, buf.Length);
                //Thread.Sleep(1000);

                buf = Encoding.ASCII.GetBytes("HT");
                ns.Write(buf, 0, buf.Length);
                //Thread.Sleep(1000);

                buf = Encoding.ASCII.GetBytes("TP/");
                ns.Write(buf, 0, buf.Length);
                //Thread.Sleep(1000);

                buf = Encoding.ASCII.GetBytes("1.1\r\n\r\nHello, my name is Colin Phillips.");
                ns.Write(buf, 0, buf.Length);*/
            }

            Console.ReadKey();
        }
    }
}
