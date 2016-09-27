using System;
using System.Collections.Generic;
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

            Console.ReadKey();
            /*
            Thread t = new Thread(() =>
            {
                WebServer.Start(4220, 64);
            });
                    
            t.Start();

            byte[] buf;
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 4220);

            using (var ns = client.GetStream())
            {
                buf = Encoding.ASCII.GetBytes("G");
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
    }
}
