﻿// Colin Phillips
// 11357836
// CS 422 - Fall 2016
// Assignment 1

using System.IO;
using System.Text;

namespace CS422
{
    /// <summary>
    /// Appends a line number for every line of text written.
    /// </summary>
    public class NumberedTextWriter : TextWriter
    {
        private readonly TextWriter _writer;
        private int _lineNumber;

        /// <summary>
        /// Gets the wrapped text writer object's encoding.
        /// </summary>
        public override Encoding Encoding
        {
            get { return _writer.Encoding; }
        }

        /// <param name="wrapThis">The text writer object to wrap.</param>
        public NumberedTextWriter(TextWriter wrapThis)
        {
             _writer = wrapThis;
            _lineNumber = 1;      // Default to starting the line number at 1.
        }

        /// <param name="wrapThis">The text writer object to wrap.</param>
        /// <param name="startingLineNumber">The starting line number.</param>
        public NumberedTextWriter(TextWriter wrapThis, int startingLineNumber)
        {
            _writer = wrapThis;
            _lineNumber = startingLineNumber;
        }

        public override void WriteLine()
        {
            _writer.WriteLine(_lineNumber + ": ");

            _lineNumber++;
        }

        /// <summary>
        /// Prints the provided string after appending the current line number.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public override void WriteLine(string value)
        {
            _writer.WriteLine(_lineNumber + ": " + value);

            _lineNumber++;
        }

        public override void Write(string value)
        {
            _writer.Write(value);
        }
    }
}
