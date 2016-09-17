/* Colin Phillips
 * CS 422 - Fall 2016
 * Assignment 3
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CS422
{
    /// <summary>
    /// A Web Server class that accepts a TCP connection with a client and validates incoming HTTP requests.
    /// </summary>
    public class WebServer
    {
        /// <summary>
        /// Launch the server and listen on the provided port.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        /// <param name="responseTemplate">
        /// The template that will be filled out and returned to the client upon receiving a valid HTTP request.
        /// </param>
        /// <returns>
        /// True if a client sends a valid HTTP request. False otherwise.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ArgumentNullException"/>
        public static bool Start(int port, string responseTemplate)
        {
            if (port < 0)
            {
                throw new ArgumentOutOfRangeException("port", "The port number cannot be less than zero.");
            }

            if (responseTemplate == null)
            {
                throw new ArgumentNullException("responseTemplate");
            }

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            TcpClient client;

            listener.Start();

            try
            {
                client = listener.AcceptTcpClient();
            }
            catch (Exception)
            {
                return false;
            }

            using (var networkStream = client.GetStream())
            {
                byte[] buf = new byte[Constants.BufSize];
                StringBuilder builder = new StringBuilder();
                HttpRequest request = new HttpRequest();

                while (request.ValidationState != ValidationState.Validated)
                {
                    int bytesRead = networkStream.Read(buf, 0, buf.Length);

                    if (bytesRead > 0)
                    {
                        builder.Append(Encoding.ASCII.GetString(buf).TrimEnd(Constants.NullByte));
                        string temp = builder.ToString();

                        // Attempt to validate the request provided.
                        // - If validated:
                        //      The request object's ValidationState will be set to 'Validated'.
                        // - If Indeterminate:
                        //      The request object's ValidationState will be set to 'Indeterminate'.
                        // - If Invalidated:
                        //      TryValidateHttpRequest will return 'Invalidated' and the connection will close.
                        if (request.TryValidate(temp) == ValidationState.Invalidated)
                        {
                            // Some portion of the request has been deemed invalid.
                            // Sever the connection to the client.
                            client.Close();
                            return false;
                        }
                    }

                    // Flush the buffer
                    for (int i = 0; i < bytesRead; i++) { buf[i] = (byte) Constants.NullByte; }
                }

                string response = GetResponse(responseTemplate, request);

                byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                networkStream.Write(responseBytes, 0, responseBytes.Length);
            }

            return true;
        }

        private static string GetResponse(string template, HttpRequest request)
        {
            return string.Format(template, Constants.MyId, DateTime.Now, request.RequestedUri);
        }
    }
}
