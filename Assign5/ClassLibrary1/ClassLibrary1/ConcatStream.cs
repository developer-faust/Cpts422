using System;
using System.IO;

namespace CS422
{
    public class ConcatStream : Stream
    {
        private readonly Stream _first, _second;
        private readonly bool _givenFixedLength;
        private long _length, _position;

        public override bool CanRead { get { return _first.CanRead && _second.CanRead; } }
        public override bool CanSeek { get { return _first.CanSeek && _second.CanSeek; } }
        public override bool CanWrite { get { return _first.CanWrite && _second.CanWrite; } }

        public override long Length
        {
            get
            {
                if (!CanSeek && !_first.CanSeek && !_givenFixedLength)
                {
                    throw new NotSupportedException("Only forward reading is provided.");
                }

                return _length;
            }
        }

        public override long Position
        {
            get { return _position; }
            set
            {
                if (!CanSeek)
                {
                    throw new NotSupportedException("Only forward reading is provided.");
                }

                if (0 > value)
                {
                    _position = 0;
                }
                else if (_length < value)
                {
                    _position = _length;
                }
                else
                {
                    _position = value;
                }
            }
        }


        public ConcatStream(Stream first, Stream second)
        {
            if (!first.CanSeek)
            {
                // If the first stream cannot seek then its length cannot be access.
                // The length of the first stream is needed to concat the second
                // stream onto it, so an exception must be thrown.
                throw new InvalidDataException("First stream must support seeking.");
            }

            _first = first;
            _second = second;
        }

        public ConcatStream(Stream first, Stream second, long fixedLength) : this(first, second)
        {
            if (fixedLength < 0)
            {
                throw new ArgumentException("The fixed length cannot be less than zero.");
            }

            _length = fixedLength;
            _givenFixedLength = true;
        }

        public override void Flush()
        {
            // TODO: Ask about flush implementation.
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek)
            {
                throw new NotSupportedException("Only forward reading is provided.");
            }

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

        public override void SetLength(long value)
        {
            if (!CanSeek)
            {
                throw new NotSupportedException("Only forward reading is provided.");
            }

            // An argument less than zero defaults the stream's length to 0.
            _length = 0 > value ? 0 : value;

            // After setting the length, the current position may also need to 
            // be altered
            if (_position > _length)
            {
                Position = _length;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
            {
                throw new NotSupportedException("Reading not supported by one or both streams.");
            }

            int bytesRead = _first.Read(buffer, offset, count);

            return _second.Read(buffer, bytesRead + offset, count - bytesRead);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Cannot read.");

            if (!CanWrite)
            {
                throw new NotSupportedException("Writing not supported by one or both streams.");
            }
        }
    }
}
