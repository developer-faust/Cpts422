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

            Thread t = new Thread(() => WebServer.Start(4220, response));
            t.Start();

            
            TcpClient client = new TcpClient();

            client.Connect("localhost", 4220);
            using (var ns = client.GetStream())
            {
                byte[] buf = Encoding.ASCII.GetBytes("GET https://google.com HTTP/1.1\r\n");

                ns.Write(buf, 0, buf.Length);

                ns.Read(buf, 0, buf.Length);

                Console.WriteLine(Encoding.ASCII.GetString(buf));
            }

            Console.ReadKey();
        }
    }
}
