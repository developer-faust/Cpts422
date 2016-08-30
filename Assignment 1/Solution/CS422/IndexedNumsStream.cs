// Colin Phillips
// 11357836
// CS 422 - Fall 2016
// Assignment 1

using System;
using System.IO;

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
        public IndexedNumsStream(long position, long length)
        {
            _length = length < 0 ? 0 : length;

            if (position < 0)
            {
                _position = 0;
            }
            else if (position > length)
            {
                _position = length;
            }
            else
            {
                _position = position;
            }
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
            if (0 > offset)
            {
                // The offset is moving the position backward from origin.
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        // Anything less than zero will be zero. So this will set the 
                        // new position to be the first position (0).
                        Position = offset;
                        break;
                    case SeekOrigin.Current:
                        // Normal seek (from current position).
                        Position += offset;
                        break;
                    case SeekOrigin.End:
                        // Normal seek (from last position).
                        Position = Length + offset;
                        break;
                }
            }
            else if (0 == offset)
            {
                // The offset is not effecting the position. The new position is determined by origin.
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        // First position is desired.
                        Position = offset;
                        break;
                    case SeekOrigin.Current:
                        // No change.
                        Position = Position;
                        break;
                    case SeekOrigin.End:
                        // The last position is desired.
                        Position = Length;
                        break;
                }
            }
            else
            {
                // The offset is moving the position forward from origin.
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        // First position is desired.
                        Position = offset;
                        break;
                    case SeekOrigin.Current:
                        // Normal seek (from current position).
                        Position += offset;
                        break;
                    case SeekOrigin.End:
                        // Last position is desired.
                        Position = Length;
                        break;
                }
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
        }

        /// <summary>
        /// Read from the stream into the provided buffer at the provided offset for 
        /// a given number of bytes.
        /// </summary>
        /// <param name="buffer">The byte array to store the read values into.</param>
        /// <param name="offset">The position in the buffer to read the values into.</param>
        /// <param name="count">The number of values to read into the buffer.</param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("Sum of offset and count is greater than buffer length.");
            }

            Seek(offset, SeekOrigin.Begin);

            while (Position < count + offset)
            {
                if (Position >= Length)
                {
                    break;
                }

                buffer[Position] = (byte) ((byte) Position % 256);
                bytesRead++;
                Position++;
            }

            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (CanWrite)
            {
                throw new NotSupportedException("The IndexedNumsStream is readonly.");
            }
        }
    }
}
