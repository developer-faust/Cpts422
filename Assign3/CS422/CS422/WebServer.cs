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
        /// <exception cref="SocketException"/>
        /// <exception cref="System.IO.IOException"/>
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

            listener.Start();
            var client = listener.AcceptTcpClient();

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
                        if (TryValidateHttpRequest(temp, request) == ValidationState.Invalidated)
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

        private static ValidationState TryValidateHttpRequest(string value, HttpRequest request)
        {
            if (!request.HasValidMethod && request.ValidationState == ValidationState.Method)
            {
                // The method has not been validated yet.

                string validMethod = null;

                Verdict verdict = request.IsValidMethod(value, ref validMethod);

                if (verdict == Verdict.Invalid)
                {
                    // The method was deemed invalid. This will close the connection.
                    return ValidationState.Invalidated;
                }

                if (verdict == Verdict.Valid)
                {
                    // A valid method was supplied.
                    request.SetMethod(validMethod);
                }

                // If neither of the above if-statements were hit, the validity of the 
                // method is still indeterminate.
            }

            if (!request.HasValidUri && request.ValidationState == ValidationState.Url)
            {
                // We are (perhaps still) attempting to determine the request's desired URL string.
                int encounteredSpaceIndex = -1;
                if (request.IsValidUriRequest(value, ref encounteredSpaceIndex) == Verdict.Valid)
                {
                    // A space has been detected - we can check the input for a uri:
                    // Because the "GET " has already been validated, we can skip that 
                    // portion of the request by taking a substring from just after the 
                    // method (request.Method.Length + 1; where '+ 1' is the space).
                    string requestUrl = value.Substring(request.Method.Length + 1, encounteredSpaceIndex - (request.Method.Length + 1));

                    if (requestUrl == string.Empty)
                    {
                        // Cannot be an empty string.
                        return ValidationState.Invalidated;
                    }

                    // This method has been validated.
                    request.SetRequestUri(requestUrl);
                }
            }

            if (!request.HasValidVersion && request.ValidationState == ValidationState.Version)
            {
                string validVersion = null;

                Verdict verdict = request.IsValidVersion(value, ref validVersion);

                if (verdict == Verdict.Invalid)
                {
                    // The version number was deemed invalid. This will close the connection.
                    return ValidationState.Invalidated;
                }

                if (verdict == Verdict.Valid)
                {
                    // A valid version number was supplied.
                    request.SetVersion(validVersion);
                }

                // If neither of the above if-statements were hit, the validity of the 
                // version number is still indeterminate.
            }

            if (!request.HasFinishedAddingHeaders && request.ValidationState == ValidationState.Headers)
            {
                string headerStart = value.Substring(request.HeaderFirstLineLength);
                while (!request.HasFinishedAddingHeaders)
                {
                    string validField = null, validFieldValue = null;

                    Verdict verdict = request.IsValidHeader(headerStart, ref validField, ref validFieldValue);

                    if (verdict == Verdict.Indeterminate)
                    {
                        // The request is not complete - go grab more from the stream.
                        break;
                    }

                    if (verdict == Verdict.Valid)
                    {
                        request.AddHeader(validField, validFieldValue);
                    }

                }
            }

            return request.ValidationState;
        }

        private static string GetResponse(string template, HttpRequest request)
        {
            return string.Format(template, Constants.MyId, DateTime.Now, request.RequestedUri);
        }
    }
}
