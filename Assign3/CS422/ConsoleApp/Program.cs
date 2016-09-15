using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CS422;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var response = "HTTP/1.1 200 OK\r\n" + "Content-Type: text/html\r\n" + "\r\n\r\n" + "<html>ID Number: {0}<br>" + "DateTime.Now: {1}<br>" + "Requested URL: {2}</html>";

            //WebServer.Start(4220, response);
            
            Thread t = new Thread(() =>
            {
                WebServer.Start(4220, response);
                Thread.Sleep(5000);
            });
            t.Start();

            
            TcpClient client = new TcpClient();

            client.Connect("localhost", 4220);
            using (var ns = client.GetStream())
            {
                //byte[] buf = Encoding.ASCII.GetBytes("GET goog.le HTTP/1.1\r\n");
                byte[] buf = Encoding.ASCII.GetBytes("GET goog.le HTTP/1.1\r\nconnection: keep-alive\r\n");
                ns.Write(buf, 0, buf.Length);

                /*
                buf = Encoding.ASCII.GetBytes("E");
                ns.Write(buf, 0, buf.Length);

                buf = Encoding.ASCII.GetBytes("T ");
                ns.Write(buf, 0, buf.Length);

                buf = Encoding.ASCII.GetBytes("https://goog");
                ns.Write(buf, 0, buf.Length);

                buf = Encoding.ASCII.GetBytes("le.com ");
                ns.Write(buf, 0, buf.Length);

                buf = Encoding.ASCII.GetBytes("HT");
                ns.Write(buf, 0, buf.Length);

                buf = Encoding.ASCII.GetBytes("TP/");
                ns.Write(buf, 0, buf.Length);
                
                buf = Encoding.ASCII.GetBytes("1.1\r\n");
                ns.Write(buf, 0, buf.Length);*/

                ns.Read(buf, 0, buf.Length);

                string fromServer = Encoding.ASCII.GetString(buf);
                Console.WriteLine(fromServer);
            }

            Console.ReadKey();
        }
    }
}
