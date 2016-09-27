// Colin Phillips
// 11357836
// CS 422 - Fall 2016
// Assignment 1

using System;
using System.IO;
using System.Diagnostics;

namespace CS422
{
    public class IndexedNumsStream : Stream
    {
        private long _position;
        private long _length;

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; } 
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the specified length in bytes of the stream.
        /// </summary>
        public override long Length 
        {
            get { return _length; }
        }

        /// <summary>
        /// Gets and sets the current position of the stream.
        /// </summary>
        public override long Position
        {
            get { return _position; }
            set
            {
                if (0 > value)
                {
                    // The new position cannot be less than 0.
                    _position = 0;
                }
                else if (Length < value)
                {
                    // The new position cannot be greater than Length.
                    _position = Length;
                }
                else
                {
                    _position = value;
                }
            }
        }

        /// <param name="position">The starting position of the stream.</param>
        /// <param name="length">The length of the stream in bytes.</param>
        public IndexedNumsStream(long length)
        {
            _length = length < 0 ? 0 : length;

            _position = 0;
        }
            
        public override void Flush()
        {
        }

        /// <summary>
        /// Move the position of the current stream.
        /// </summary>
        /// <param name="offset">The number of bytes to offset the position by.</param>
        /// <param name="origin">The point of origin for the offset.</param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }

            return _position;
        }

        /// <summary>
        /// Modify the length of the stream in bytes.
        /// </summary>
        /// <param name="value">The new length of the stream in bytes.</param>
        public override void SetLength(long value)
        {
            // An argument less than zero defaults the stream's length to 0.
            _length = (0 > value) ? 0 : value;

            // After setting the length, the current position may also need to 
            // be altered
            if (_position > _length)
            {
                Position = _length;
            }
        }

        /// <summary>
        /// Read from the stream into the provided buffer at the provided offset for 
        /// a given number of bytes.
        /// </summary>
        /// <param name="buffer">The byte array to store the read values into.</param>
        /// <param name="offset">The position in the buffer to read the values into.</param>
        /// <param name="count">The number of values to read into the buffer.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;

            if (offset < 0)
            {
                // Offset cannot be negative.
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count < 0)
            {
                // Count cannot be negative.
                throw new ArgumentOutOfRangeException("count");
            }

            if (buffer == null)
            {
                // Buffer cannot be null.
                throw new ArgumentNullException("buffer");
            }

            if (offset + count > buffer.Length)
            {
                // Attempting to read into the buffer beyond its length.
                throw new ArgumentException("Sum of offset and count is greater than buffer length.");
            }
                
            //Seek(offset, SeekOrigin.Begin);

            while (bytesRead < count)
            {
                if (Position >= Length)
                {
                    // End of stream.
                    break;
                }

                buffer[Position] = (byte) (Position % 256);
                bytesRead++;
                Position++;
            }

            return bytesRead;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Count.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (CanWrite)
            {
                throw new NotSupportedException("The IndexedNumsStream is readonly.");
            }
        }
    }
}
