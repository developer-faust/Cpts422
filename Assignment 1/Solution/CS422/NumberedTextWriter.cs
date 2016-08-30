// Colin Phillips
// 11357836
// CS 422 - Fall 2016
// Assignment 1

using System;
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

        private bool _disposed;

        /// <summary>
        /// Gets the wrapped text writer object's encoding.
        /// </summary>
        public override Encoding Encoding
        {
            get { return _writer.Encoding; }
        }

        /// <param name="wrapThis">The text writer object to wrap.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public NumberedTextWriter(TextWriter wrapThis)   
        {
            if (wrapThis == null)
            {
                throw new ArgumentNullException("wrapThis");
            }

             _writer = wrapThis;
            _lineNumber = 1;      // Default to starting the line number at 1.
        }

        /// <param name="wrapThis">The text writer object to wrap.</param>
        /// <param name="startingLineNumber">The starting line number.</param>
        /// <exception cref="ArgumentNullException"/>
        public NumberedTextWriter(TextWriter wrapThis, int startingLineNumber) 
        {
            if (wrapThis == null)
            {
                throw new ArgumentNullException("wrapThis");
            }

            _writer = wrapThis;
            _lineNumber = startingLineNumber;
        }

        /// <exception cref="ObjectDisposedException"/>
        protected override void Dispose (bool disposing)
        {
            if (_disposed) 
            {
                throw new ObjectDisposedException ("NumberTextWriter object is already disposed.");  
            }

            _disposed = true;
            base.Dispose (disposing);
        }

        /// <summary>
        /// Simply writes a line with only the current line number.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        public override void WriteLine()
        {
            if (_disposed) 
            {
                throw new ObjectDisposedException ("NumberTextWriter object is already disposed.");  
            }
                
            _writer.WriteLine(_lineNumber + ": ");

            _lineNumber++;
        }

        /// <summary>
        /// Prints the provided string after appending the current line number.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <exception cref="ObjectDisposedException"/>
        public override void WriteLine(string value)
        {
            if (_disposed) 
            {
                throw new ObjectDisposedException ("NumberTextWriter object is already disposed.");  
            }

            _writer.WriteLine(_lineNumber + ": " + value);

            _lineNumber++;
        }

        /// <exception cref="ObjectDisposedException"/>
        public override void Write(string value)
        {
            if (_disposed) 
            {
                throw new ObjectDisposedException ("NumberTextWriter object is already disposed.");  
            }

            _writer.Write(value);
        }
    }
}
