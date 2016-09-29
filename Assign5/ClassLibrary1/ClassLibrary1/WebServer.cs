using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CS422
{
    public static class WebServer
    {
        private static readonly BlockingCollection<TcpClient> _collection = new BlockingCollection<TcpClient>();
        private static readonly List<Thread> _pool = new List<Thread>();
        private static readonly HashSet<WebService> _services = new HashSet<WebService> {new DemoService()};
        private static TcpListener _listener;
        private static Thread _listeningThread;
        private static int _threadCount;

        public static void Start(int listenPort, int threadCount)
        {
            if (_listeningThread != null)
            {
                throw new InvalidOperationException("Server already started.");
            }

            _threadCount = threadCount < 0 ? 64 : threadCount;

            _listeningThread = new Thread(() =>
            {
                #region Listener-ThreadWork
                _listener = new TcpListener(IPAddress.Any, listenPort);
                _listener.Start();

                // TODO: figure out a better conditional other than 'true'
                while (true)
                {
                    try
                    {
                        var client = _listener.AcceptTcpClient();
                        _collection.Add(client);
                    }
                    catch (SocketException)
                    {
                        break;
                    }
                }
                #endregion
            });
            _listeningThread.Start();

            // Initialize all threads to await a client connection.
            for (int i = 0; i < _threadCount; i++)
            {
                Thread t = new Thread(ClientThreadWork);
                t.Start();
                _pool.Add(t);
            }
        }

        public static void Stop()
        {
            _listener.Stop();
            _listeningThread.Join();
            
               
            for (int i = 0; i < _threadCount; i++)
            {
                _collection.Add(null);
            }

            foreach (Thread thread in _pool)
            {
                thread.Join();
            }
        }

        public static void AddService(WebService service)
        {
            _services.Add(service);
        }

        private static void ClientThreadWork()
        {
            while (true)
            {
                TcpClient client = _collection.Take();
                if (null == client)
                {
                    break;
                }

                WebRequest request = BuildRequest(client);

                if (null == request)
                {
                    client.Close();
                }
                else
                {
                    WebService handler = FindHandler(request);

                    if (null != handler)
                    {
                        handler.Handler(request);
                    }

                    //request.WriteNotFoundResponse();
                }

            }
        }

        private static WebRequest BuildRequest(TcpClient client)
        {
            WebRequest request = new WebRequest();

            var networkStream = client.GetStream();
            if (!request.Validate(networkStream))
            {
                // Sever the connection to the client.
                client.Close();
                networkStream.Dispose();
                return null;
            }

            return request;
        }

        private static WebService FindHandler(WebRequest request)
        {
            foreach (var service in _services)
            {
                if (string.Compare(
                    request.RequestedUri[0].ToString(), 
                    service.ServiceURI,
                    StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return service;
                }
            }

            return null;
        }
    }
}
