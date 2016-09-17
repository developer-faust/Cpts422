/* Colin Phillips
 * CS 422 - Fall 2016
 * Assignment 3
 */

namespace CS422
{
    /// <summary>
    /// Provides constants that are used to create HTTP repsonse strings and validate HTTP request strings.
    /// </summary>
    internal class Constants
    {
        public const int BufSize = 4096;
        public const char NullByte = '\0';
        public const char SingleSpace = ' ';
        public const string Crlf = "\r\n";
        public const string ColonSpace = ": ";
        public const string MyId = "11357836";
        public const string Version1Dot1 = "HTTP/1.1";
        public const string Response200 = "200 OK";
        public const string HtmlContentType = "Content-Type: text/html";
    }
}
