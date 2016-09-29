using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace CS422
{
    /// <summary>
    /// Provides a container for an HTTP request. Provides methods to validate portions of a request header.
    /// </summary>
    public class WebRequest
    {
        private readonly HashSet<string> _validMethods = new HashSet<string> {"GET"};
        private readonly HashSet<string> _validVersions = new HashSet<string> {"HTTP/1.1"};

        private bool _hasValidMethod;
        private bool _hasValidUri;
        private bool _hasValidVersion;
        private bool _hasFinishedAddingHeaders;

        private NetworkStream _clientStream;

        #region Properties

        public Stream Body { get; private set; }

        /// <summary>
        /// Get the validation state of this request.
        /// </summary>
        public ValidationState ValidationState { get; private set; }

        /// <summary>
        /// Gets the validated method, if available, of this request.
        /// </summary>
        public string Method { get; private set; }

        /// <summary>
        /// Gets the validated uri, if available, of this request.
        /// </summary>
        public string RequestedUri { get; private set; }

        /// <summary>
        /// Gets the validated version, if available, of this request.
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Gets a list of the headers associated with this request.
        /// </summary>
        public ConcurrentDictionary<string, string> Headers { get; }

        #endregion

        #region Constructors

        public WebRequest()
        {
            Method = RequestedUri = Version = string.Empty;
            ValidationState = ValidationState.Method;
            Headers = new ConcurrentDictionary<string, string>();
        }

        /// <param name="validMethods">A set of methods that should be used in the validation process.</param>
        /// <param name="validVersions">A set of versions that should be used in the validation process.</param>
        public WebRequest(IEnumerable<string> validMethods = null, IEnumerable<string> validVersions = null) : this()
        {
            if (validMethods != null)
            {
                foreach (var method in validMethods)
                {
                    _validMethods.Add(method);
                }
            }

            if (validVersions != null)
            {
                foreach (var version in validVersions)
                {
                    _validVersions.Add(version);
                }
            }
        }

        #endregion

        public void WriteNotFoundResponse(string pageHTML)
        {
            if (null == _clientStream || ValidationState != ValidationState.Validated)
            {
                throw new InvalidOperationException(
                    "The client's request has not been validated or the connection has been closed.");
            }
        }

        public bool WriteHTMLResponse(string htmlString)
        {
            if (null == _clientStream || ValidationState != ValidationState.Validated)
            {
                throw new InvalidOperationException(
                    "The client's request has not been validated or the connection has been closed.");
            }

            long bodyLength;

            try
            {
                bodyLength = Body.Length;
            }
            catch (NotSupportedException)
            {
                bodyLength = -1;
            }

            byte[] reponse =
                Encoding.ASCII.GetBytes(string.Format(htmlString, Method, RequestedUri, bodyLength, Constants.MyId));


            _clientStream.Write(
                reponse,
                0,
                reponse.Length);

            // TODO: return true??
            return true;
        }

        public bool Validate(NetworkStream clientStream)
        {
            _clientStream = clientStream; /* Store the client's network stream for later use (writing the response). */

            byte[] buf = new byte[Constants.BufSize];
            MemoryStream requestStream = new MemoryStream();

            while (ValidationState != ValidationState.Validated)
            {
                int bytesRead = _clientStream.Read(buf, 0, buf.Length);

                if (bytesRead > 0)
                {
                    requestStream.Write(buf, 0, bytesRead);

                    // Attempt to validate the request provided.
                    // - If validated:
                    //      The request object's ValidationState will be set to 'Validated'.
                    // - If Indeterminate:
                    //      The request object's ValidationState will be set to 'Indeterminate'.
                    // - If Invalidated:
                    //      TryValidateHttpRequest will return 'Invalidated' and the connection will close.
                    if (TryValidate(Encoding.ASCII.GetString(requestStream.ToArray())) == ValidationState.Invalidated)
                    {
                        // Some portion of the request has been deemed invalid.
                        requestStream.Dispose();
                        return false;
                    }
                }
            }

            SetBody(requestStream);

            return true;
        }

        /// <summary>
        /// Attempt to validate the provided HTTP request.
        /// </summary>
        /// <param name="value">The HTTP request to attempt to validate.</param>
        /// <returns>
        /// Returns the current state of the request validation process.
        /// </returns>
        private ValidationState TryValidate(string value)
        {
            if (!_hasValidMethod && ValidationState == ValidationState.Method)
            {
                // The method has not been validated yet.

                string validMethod = null;

                Verdict verdict = IsValidMethod(value, ref validMethod);

                if (verdict == Verdict.Invalid)
                {
                    // The method was deemed invalid. This will close the connection.
                    return ValidationState.Invalidated;
                }

                if (verdict == Verdict.Valid)
                {
                    // A valid method was supplied.
                    SetMethod(validMethod);
                }

                // If neither of the above if-statements were hit, the validity of the 
                // method is still indeterminate.
            }

            if (!_hasValidUri && ValidationState == ValidationState.Url)
            {
                // We are (perhaps still) attempting to determine the request's desired URL string.
                int encounteredSpaceIndex = -1;
                if (IsValidUriRequest(value, ref encounteredSpaceIndex) == Verdict.Valid)
                {
                    // A space has been detected - we can check the input for a uri:
                    // Because the "GET " has already been validated, we can skip that 
                    // portion of the request by taking a substring from just after the 
                    // method (request.Method.Length + 1; where '+ 1' is the space).
                    string requestUrl = value.Substring(Method.Length + 1, encounteredSpaceIndex - (Method.Length + 1));

                    if (requestUrl == string.Empty)
                    {
                        // Cannot be an empty string.
                        return ValidationState.Invalidated;
                    }

                    // This method has been validated.
                    SetRequestUri(requestUrl);
                }
                if (!_hasValidUri && value.IndexOf(Constants.Crlf, StringComparison.Ordinal) >= 0)
                {
                    // A space was missing in this request.
                    return ValidationState.Invalidated;
                }
            }

            if (!_hasValidVersion && ValidationState == ValidationState.Version)
            {
                string validVersion = null;

                Verdict verdict = IsValidVersion(value, ref validVersion);

                if (verdict == Verdict.Invalid)
                {
                    // The version number was deemed invalid. This will close the connection.
                    return ValidationState.Invalidated;
                }

                if (verdict == Verdict.Valid)
                {
                    // A valid version number was supplied.
                    SetVersion(validVersion);
                }

                // If neither of the above if-statements were hit, the validity of the 
                // version number is still indeterminate.
            }

            if (!_hasFinishedAddingHeaders && ValidationState == ValidationState.Headers)
            {
                string headerStart = value.Substring(GetRequestFirstLineLength());
                string validField = null, validFieldValue = null;
                uint headerCount = 0;

                // There may be more than one header, so keep adding until more information is needed, the request is 
                // invalidated, or all headers are added.
                while (!_hasFinishedAddingHeaders)
                {
                    bool isEndingCrlf;
                    Verdict verdict = IsValidHeaderOrEndingCrlf(headerStart, ref validField, ref validFieldValue, out isEndingCrlf);

                    if (verdict == Verdict.Invalid)
                    {
                        // A header was determined to be in an invalid format.
                        return ValidationState.Invalidated;
                    }

                    if (verdict == Verdict.Indeterminate)
                    {
                        // A header was not invalidate, but could not be validated either.
                        // Go get more information from the client.
                        break;
                    }

                    if (verdict == Verdict.Valid)
                    {
                        // A header (or lack thereof), was validated.
                        if (isEndingCrlf)
                        {
                            // The ending CRLF was encountered with no headers in between it. This signals the
                            // end of the request's headers.
                            FinishAddingHeaders();
                        }
                        else
                        {
                            if (headerCount >= Headers.Count)
                            {
                                // To avoid adding headers from previous reads, only allow inserting the encountered header
                                // if it never has been added before.
                                AddHeader(validField, validFieldValue);
                            }
                            // Trim the headers string of the header that was just added.
                            headerStart = headerStart.Substring(validField.Length + validFieldValue.Length + 3);
                            headerCount++;
                        }
                    }
                }
            }

            return ValidationState;
        }

        #region Portion Setters
        /// <summary>
        /// Set the method for this request and advance its validation state.
        /// </summary>
        /// <param name="method">The validated method string.</param>
        private void SetMethod(string method)
        {
            Method = method;
            _hasValidMethod = true;
            ValidationState++;
        }

        /// <summary>
        /// Set the URI for this request and advance its validation state.
        /// </summary>
        /// <param name="requestUrl">The validated URI string.</param>
        private void SetRequestUri(string requestUrl)
        {
            RequestedUri = requestUrl;
            _hasValidUri = true;
            ValidationState++;
        }

        /// <summary>
        /// Set the version for this request and advance its validation state.
        /// </summary>
        /// <param name="version">The validated version string.</param>
        private void SetVersion(string version)
        {
            Version = version;
            _hasValidVersion = true;
            ValidationState++;
        }

        /// <summary>
        /// Add a header (field and value pair) to the collection of headers.
        /// </summary>
        /// <param name="field">The field value.</param>
        /// <param name="value">The value of the field.</param>
        private void AddHeader(string field, string value)
        {
            if (_hasFinishedAddingHeaders)
            {
                throw new InvalidOperationException("Already finished adding headers.");
            }

            // Normalize the field to easily search for it later.
            Headers.TryAdd(field.ToLower(), value);
        }

        /// <summary>
        /// Mark this request as having all headers filled in.
        /// </summary>
        private void FinishAddingHeaders()
        {
            if (!_hasFinishedAddingHeaders)
            {
                // This method should only be called once, but just to be safe.
                // I also don't think it's worth throwing an exception here like
                // was done in "AddHeader".
                ValidationState++;
            }

            _hasFinishedAddingHeaders = true;
        }

        private void SetBody(MemoryStream requestStream)
        {
            int bodyStart =
                Encoding.ASCII.GetString(requestStream.ToArray())
                    .IndexOf(Constants.DoubleCrlf, StringComparison.Ordinal) + Constants.DoubleCrlf.Length;

            requestStream.Seek(bodyStart, SeekOrigin.Begin);

            if (Headers.ContainsKey(Constants.HttpContentLength))
            {
                long fixedLength;
                if (long.TryParse(Headers[Constants.HttpContentLength], out fixedLength))
                {
                    Body = new ConcatStream(requestStream, _clientStream, fixedLength);
                    return;
                }
            }

            Body = new ConcatStream(requestStream, _clientStream);
        }
        #endregion

        #region Validators
        /// <summary>
        /// Determines if the passed HTTP request begins with a valid method.
        /// </summary>
        /// <param name="value">The HTTP request string.</param>
        /// <param name="outMethod">The valid and trimmed HTTP method (if found).</param>
        /// <returns>
        /// The verdict (Valid, Invalid, Indeterminate) of whether or not the string starts with
        /// a valid HTTP method.
        /// </returns>
        /// <remarks>
        /// This method only accepts valid methods in the form of "METHOD " where the 
        /// space after the method is intentional. Values that begin with a valid method but are
        /// not followed by a space are considered 'Indeterminate'.
        /// </remarks>
        /// <example>
        /// value = "GET " will return VALID.
        /// value = "GET"  will return INDETERMINATE.
        /// value = "GPE"  will return INVALID.
        /// </example>
        public Verdict IsValidMethod(string value, ref string outMethod)
        {
            Verdict verdict = Verdict.Invalid;

            // Get the index of the space immediately following the method (if available).
            int spaceIndex = GetIndexOf(Constants.SingleSpace, value);

            // Determine the minimum length to filter out methods with a lesser length than the
            // one provided in the request.
            int minMethodLength = spaceIndex < 0 ? value.Length : spaceIndex;

            foreach (var method in _validMethods.Where(v => v.Length >= minMethodLength))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    // There may be a possible match (a method has been found with a least the 
                    // same length as the input value).
                    verdict = Verdict.Indeterminate;
                    if (value[i] != method[i])
                    {
                        // The possible match was found to be a miss-match.
                        verdict = Verdict.Invalid;
                        break;
                    }

                    if (i == method.Length - 1 && i + 1 == spaceIndex)
                    {
                        // One of the methods have been exhausted.
                        // In addition to this, the next character of the input value is
                        // a space - therefore we have a valid method with a space after it.
                        verdict = Verdict.Valid;
                        outMethod = method;
                        return verdict;
                    }
                }
            }

            return verdict;
        }

        /// <summary>
        /// Determines if the passed HTTP request contains a valid URI.
        /// </summary>
        /// <param name="value">The HTTP request.</param>
        /// <param name="outSpaceIndex">The index of the space immediately following the URI string.</param>
        /// <returns>
        /// The verdict (Valid, Indeterminate) of whether or not the string contains a valid URI.
        /// </returns>
        /// <example>
        /// value = "{method} https://google.com " will return VALID.
        /// value = "{method} https://google.com"  will return INDETERMINATE.
        /// </example>
        public Verdict IsValidUriRequest(string value, ref int outSpaceIndex)
        {
            // Get the index of the space that should immediately follow the URI.
            int encounteredSpaceIndex = GetIndexOf(Constants.SingleSpace, value, 1);

            if (encounteredSpaceIndex < 0)
            {
                // No space was encountered, the full URI has not been provided yet.
                return Verdict.Indeterminate;
            }

            outSpaceIndex = encounteredSpaceIndex;
            return Verdict.Valid;
        }

        /// <summary>
        /// Determines if the passed HTTP request (first line) ends with a valid version number.
        /// </summary>
        /// <param name="value">The HTTP request string.</param>
        /// <param name="outVersion">The valid and trimmed HTTP version number (if found).</param>
        /// <returns>
        /// The verdict (Valid, Invalid, Indeterminate) of whether or not the request's first line ends with
        /// a valid HTTP version number.
        /// </returns>
        /// <remarks>
        /// This method only accepts valid version numbers in the form of "VERSION\r\n" where the 
        /// CRLF after the version is intentional. Values that begin with a valid verison but are
        /// not followed by the CRLF are considered 'Indeterminate'.
        /// </remarks>
        /// <example>
        /// value = "{method} {uri} HTTP/1.1\r\n" will return VALID.
        /// value = "{method} {uri} HTTP/1.1\r\n" will return INDETERMINATE.
        /// value = "{method} {uri} HTTP/1.2\r\n" will return INVALID.
        /// </example>
        public Verdict IsValidVersion(string value, ref string outVersion)
        {
            Verdict verdict = Verdict.Invalid;

            // Get the starting index of the version string.
            int startIndex = GetIndexOf(Constants.SingleSpace, value, 1) + 1;

            // Get the index of the CRLF (if available) that follows this version number.
            int crlfIndex = value.IndexOf(Constants.Crlf, StringComparison.Ordinal);

            // Determine the minimum length to filter out version numbers with a lesser length than the
            // one provided in the request.
            int minVersionLength = crlfIndex > 0 ? crlfIndex - startIndex : value.Length - startIndex;

            foreach (var version in _validVersions.Where(v => v.Length >= minVersionLength))
            {
                // There may be a possible match (a verion number has been found with a least the 
                // same length as the input value).
                verdict = Verdict.Indeterminate;
                for (int i = 0; i < minVersionLength; i++)
                {
                    if (value[i + startIndex] != version[i])
                    {
                        // The possible match was found to be a miss-match.
                        verdict = Verdict.Invalid;
                        break;
                    }

                    if (i == version.Length - 1 && (i + startIndex) + 1 == crlfIndex)
                    {
                        // One of the version numbers have been exhausted.
                        // In addition to this, the next two characters of the input value 
                        // represent a CRLF. Therefore we have a valid version number followed
                        // by a CRLF.
                        verdict = Verdict.Valid;
                        outVersion = version;
                        return verdict;
                    }
                }
            }

            return verdict;
        }

        /// <summary>
        /// Determines if the passed value contains a valid header (field / value pair).
        /// </summary>
        /// <param name="value">The HTTP request value to parse.</param>
        /// <param name="outField">The valid and trimmed HTTP header field (if found).</param>
        /// <param name="outValue">The valid and trimmed HTTP header field value (if found).</param>
        /// <param name="isEndingCrlf">Returns whether or not the value is an ending CRLF.</param>
        /// <returns></returns>
        public Verdict IsValidHeaderOrEndingCrlf(string value, ref string outField, ref string outValue, out bool isEndingCrlf)
        {
            // Determine the index of the first color-space string in the value.
            int colonIndex = value.IndexOf(Constants.ColonSpace, StringComparison.Ordinal);

            // Determine the index of the first encountered CRLF in the value.
            int crlfIndex = value.IndexOf(Constants.Crlf, StringComparison.Ordinal);


            isEndingCrlf = false;

            if (crlfIndex > 0 && colonIndex < 0)
            {
                // Characters were encountered between the start and the first CRLF, but no colon was.
                // This is invalid.
                return Verdict.Invalid;
            }

            if (colonIndex < 0 && crlfIndex < 0 && string.CompareOrdinal(Constants.Crlf, value) != 0)
            {
                // No colon was encounter and no CRLF was encountered (and the string wasn't a lone CRLF).
                // This is not invalid, but not yet valid either as we may still receive these in future reads.
                return Verdict.Indeterminate;
            }

            if (crlfIndex > 0)
            {
                // A colon has been encountered. Furthermore, the index of the CRLF is not zero so we have
                // characters (including the colon) in-between the start and the CRLF. This must be a header.
                outField = value.Substring(0, colonIndex);
                outValue = value.Substring(colonIndex + 1, crlfIndex - (colonIndex + 1)).TrimEnd('\r', '\n');
            }
            else
            {
                // No color was encountered, but a CRLF was encountered. Furthermore, this string contains 
                // no characters between the start and this CRLF. This must be the ending CRLF.
                isEndingCrlf = true;
            }

            return Verdict.Valid;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Return the length of the first line of this HTTP request.
        /// </summary>
        /// <remarks>
        /// Total length = 
        ///     method's length   + 
        ///     uri's length      +
        ///     versions's length +
        ///     two spaces        +
        ///     '\r' and '\n' (2) 
        /// 
        /// This length is only guaratneed to be correct IF the request's 
        /// validation state is at HEADERS or greater.
        /// </remarks>
        internal int GetRequestFirstLineLength()
        {
            if (ValidationState < ValidationState.Headers)
            {
                throw new InvalidOperationException("Validiation state not sufficient to receive a valid value from GetRequestFirstLineLength()");
            }

            return Method.Length + RequestedUri.Length + Version.Length + 4;
        }

        /// <summary>
        /// Get the first occurance of the passed delimiter in the string provided.
        /// </summary>
        /// <param name="delim">The delimeter to chack for.</param>
        /// <param name="value">The string to look through.</param>
        /// <param name="numberToIgnore">If desired, the number of times to skip the delimeter before returning.</param>
        /// <returns>
        /// The index of the first occurance (after ignoring if applicable) of the delimeter in the string or -1 if not found.
        /// </returns>
        private static int GetIndexOf(char delim, string value, int numberToIgnore = 0)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == delim)
                {
                    // Only return this index if we have skipped "numberOfSpacesToIgnore" many spaces.
                    if (numberToIgnore == 0)
                    {
                        return i;
                    }

                    numberToIgnore--;
                }
            }

            return -1;
        }
        #endregion
    }

}
