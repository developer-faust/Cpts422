using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CS422
{
    /// <summary>
    /// Provides a container for an HTTP request. Provides methods to validate portions of a request header.
    /// </summary>
    internal class HttpRequest
    {
        private readonly HashSet<string> _validMethods = new HashSet<string>{ "GET" };
        private readonly HashSet<string> _validVersions = new HashSet<string> {"HTTP/1.1"};

        #region Properties
        /// <summary>
        /// Get the validation state of this request.
        /// </summary>
        public ValidationState ValidationState { get; private set; }

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
        /// </remarks>
        public int HeaderFirstLineLength
        {
            get { return Method.Length + RequestedUri.Length + Version.Length + 4; }
        }

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
        public List<HttpHeader> Headers { get; private set; }

        /// <summary>
        /// Gets whether or not this request has a valid method.
        /// </summary>
        public bool HasValidMethod { get; private set; }

        /// <summary>
        /// Gets whether or not this request has a valid URI.
        /// </summary>
        public bool HasValidUri { get; private set; }

        /// <summary>
        /// Gets whether or not this request has a valid version number.
        /// </summary>
        public bool HasValidVersion { get; private set; }

        /// <summary>
        /// Gets whether or not all headers have been added.
        /// </summary>
        public bool HasFinishedAddingHeaders { get; private set; }
        #endregion

        #region Constructors
        public HttpRequest()
        {
            Method = RequestedUri = Version = string.Empty;
            ValidationState = ValidationState.Method;
            Headers = new List<HttpHeader>();
        }

        /// <param name="validMethods">A set of methods that should be used in the validation process.</param>
        /// <param name="validVersions">A set of versions that should be used in the validation process.</param>
        public HttpRequest(IEnumerable<string> validMethods = null, IEnumerable<string> validVersions = null) : this()
        {
            if (validMethods != null)
            {
                foreach (var method in validMethods) { _validMethods.Add(method); }
            }

            if (validVersions != null)
            {
                foreach (var version in validVersions) { _validVersions.Add(version); }
            }
        }
        #endregion

        #region Portion Setters
        /// <summary>
        /// Set the method for this request and advance its validation state.
        /// </summary>
        /// <param name="method">The validated method string.</param>
        public void SetMethod(string method)
        {
            Method = method;
            HasValidMethod = true;
            ValidationState++;
        }

        /// <summary>
        /// Set the URI for this request and advance its validation state.
        /// </summary>
        /// <param name="requestUrl">The validated URI string.</param>
        public void SetRequestUri(string requestUrl)
        {
            RequestedUri = requestUrl;
            HasValidUri = true;
            ValidationState++;
        }

        /// <summary>
        /// Set the version for this request and advance its validation state.
        /// </summary>
        /// <param name="version">The validated version string.</param>
        public void SetVersion(string version)
        {
            Version = version;
            HasValidVersion = true;
            ValidationState++;
        }

        /// <summary>
        /// Add a header (field and value pair) to the collection of headers.
        /// </summary>
        /// <param name="field">The field value.</param>
        /// <param name="value">The value of the field.</param>
        public void AddHeader(string field, string value)
        {
            if (HasFinishedAddingHeaders)
            {
                throw new InvalidOperationException("Already finished adding headers.");
            }

            Headers.Add(new HttpHeader
            {
                Field = field,
                Value = value
            });
        }

        /// <summary>
        /// Mark this request as having all headers filled in.
        /// </summary>
        public void FinishAddingHeaders()
        {
            if (!HasFinishedAddingHeaders)
            {
                // This method should only be called once, but just to be safe.
                // I also don't think it's worth throwing an exception here like
                // was done in "AddHeader".
                ValidationState++;
            }

            HasFinishedAddingHeaders = true;
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
            int crlfIndex = GetIndexOf(Constants.Crlf, value) + 1;

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
        /// <returns></returns>
        public Verdict IsValidHeader(string value, ref string outField, ref string outValue)
        {
            int colonIndex = GetIndexOf(Constants.ColonSpace, value) + 1;
            int clrfIndex = GetIndexOf(Constants.Crlf, value) + 1;

            if (colonIndex < 0 || colonIndex < 0)
            {
                return Verdict.Indeterminate;
            }

            outField = value.Substring(0, colonIndex);
            outValue = value.Substring(colonIndex + 1).TrimEnd('\r', '\n');
            

            return Verdict.Valid;
        }
        #endregion

        #region Static Utility
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

        private static int GetIndexOf(string delim, string value, int numberToIgnore = 0)
        {
            int iDelim = 0;

            for (int i = 0; i <= value.Length - delim.Length; i++)
            {
                iDelim = numberToIgnore == 0 
                    ? delim[iDelim] == value[i] 
                        ? iDelim + 1
                        : 0 
                    : 0;

                if (iDelim == delim.Length - 1)
                {
                    return i - iDelim;
                }
            }

            return -1;
        }
        #endregion

        #region HTTP Header Class
        internal class HttpHeader
        {
            public string Field { get; set; }
            public string Value { get; set; }
        }
        #endregion
    }
}
