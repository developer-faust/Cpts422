using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace CS422
{
    internal enum ValidationState
    {
        Method = 0,
        Url = 1,
        Version = 2,
        Validated = 3
    }

    public class WebServer
    {
        private static readonly string[] ValidMethods = {"GET"};

        private static readonly string[] ValidVersions = {"HTTP/1.1"};

        public static bool Start(int port, string responseTemplate)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);

            listener.Start();
            var client = listener.AcceptTcpClient();

            using (var networkStream = client.GetStream())
            {
                byte[] buf = new byte[Constants.BufSize];
                StringBuilder builder = new StringBuilder();
                HttpRequest request = new HttpRequest();

                while (true)
                {
                    if (request.ValidationState == ValidationState.Validated)
                    {
                        // This request has been deemed valid. 
                        break;
                    }

                    int bytesRead = networkStream.Read(buf, 0, buf.Length);

                    if (bytesRead > 0)
                    {
                        builder.Append(Encoding.ASCII.GetString(buf));
                        string temp = builder.ToString();
                        int encounteredSpaceIndex;

                        if (!request.HasValidMethod && request.ValidationState == ValidationState.Method)
                        {
                            // We are (perhaps still) attempting to determine the request's method.
                            if ((encounteredSpaceIndex = GetIndexOfSpace(temp, 0)) >= 0)
                            {
                                // A space has been detected - we can check the input for a proper method:
                                string possibleMethod = temp.Substring(0, encounteredSpaceIndex);
                                if (!IsValidMethod(possibleMethod))
                                {
                                    // Invalid method.
                                    return false;
                                }

                                // This method has been validated.
                                request.SetMethod(possibleMethod);
                            }
                        }

                        if (!request.HasValidUri && request.ValidationState == ValidationState.Url)
                        {
                            // We are (perhaps still) attempting to determine the request's desired URL string.
                            if ((encounteredSpaceIndex = GetIndexOfSpace(temp, 1)) >= 0)
                            {
                                // A space has been detected - we can check the input for a uri:
                                // Because the "GET " has already been validated, we can skip that 
                                // portion of the request by taking a substring from just after the 
                                // method (request.Method.Length + 1; where '+ 1' is the space).
                                string requestUrl = temp.Substring(request.Method.Length + 1, encounteredSpaceIndex - (request.Method.Length + 1));

                                // This method has been validated.
                                request.SetRequestUrl(requestUrl);
                            }
                        }

                        if (request.ValidationState == ValidationState.Version)
                        {
                            // We are (perhaps still) attempting to determine the request's HTTP version.
                            if ((encounteredSpaceIndex = GetIndexOfCrlf(temp)) >= 0)
                            {
                                // A CRLF has been detected.

                                string possibleVersion = temp.Substring(
                                    request.Method.Length + request.RequestedUrl.Length + 2, 
                                    encounteredSpaceIndex - (request.Method.Length + request.RequestedUrl.Length + 2));

                                if (!IsValidVersion(possibleVersion))
                                {
                                    // Invalid version.
                                    return false;
                                }

                                request.SetVersion(possibleVersion);
                            }
                        }
                    }
                }

                string response = GetResponse(responseTemplate, request);

                byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                networkStream.Write(responseBytes, 0, responseBytes.Length);
            }

            return true;
        }

        private static bool IsValidMethod(string method)
        {
            return ValidMethods.Any(m => string.CompareOrdinal(m, method) == 0);
        }

        private static bool IsValidVersion(string version)
        {
            return ValidVersions.Any(v => string.CompareOrdinal(v, version) == 0);
        }

        private static int GetIndexOfSpace(string value, int numberOfSpacesToIgnore)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == Constants.SingleSpace)
                {
                    if (numberOfSpacesToIgnore == 0)
                    {
                        return i;
                    }

                    numberOfSpacesToIgnore--;
                }
            }

            return -1;
        }

        private static int GetIndexOfCrlf(string value)
        {
            for (int i = 0; i < value.Length - 1; i++)
            {
                if (value[i] == '\r' && value[i + 1] == '\n')
                {
                    return i;
                }
            }

            return -1;
        }

        private static string GetResponse(string template, HttpRequest request)
        {
            return 
                Constants.Version1Dot1 + Constants.SingleSpace + Constants.Response200 + Constants.Crlf +
                Constants.HtmlContentType + Constants.Crlf + Constants.Crlf + Constants.Crlf + 
                string.Format(template, Constants.MyId, DateTime.Now, request.RequestedUrl);
        }
    }

    internal class HttpRequest
    {
        public HttpRequest()
        {
            Method = RequestedUrl = Version = string.Empty;
            ValidationState = ValidationState.Method;
        }

        public ValidationState ValidationState { get; private set; }

        public string Method { get; private set; }
        public string RequestedUrl { get; private set; }
        public string Version { get; private set; }

        public bool HasValidMethod { get; private set; }
        public bool HasValidUri { get; private set; }
        public bool HasValidVersion { get; private set; }

        public void SetMethod(string method)
        {
            Method = method;
            HasValidMethod = true;
            ValidationState++;
        }

        public void SetRequestUrl(string requestUrl)
        {
            RequestedUrl = requestUrl;
            HasValidUri = true;
            ValidationState++;
        }

        public void SetVersion(string version)
        {
            Version = version;
            HasValidVersion = true;
            ValidationState++;
        }
    }

    internal class Constants
    {
        public const char SingleSpace = ' ';
        public const string Crlf = "\r\n";
        public const string MyId = "11357836";
        public const string Version1Dot1 = "HTTP/1.1";
        public const string Response200 = "200 OK";
        public const string HtmlContentType = "Content-Type: text/html";
        public const int BufSize = 4096;
    }
}
