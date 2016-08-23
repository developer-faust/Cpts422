// Colin Phillips
// 11357836
// CS 422 - Fall 2016
// Assignment 1 (Part 2)

using System;
using System.IO;

namespace CS422
{
  public class IndexedNumsStream : Stream
  {
    private long _position;
    private long _length;


    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;

    /// <summary>
    /// Gets the specified length in bytes of the stream.
    /// </summary>
    public override long Length => _length;

    /// <summary>
    /// Gets and sets the current position of the stream.
    /// </summary>
    public override long Position
    {
      get
      {
        return _position;
      }
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
      _position = position;
      _length = length;
    }

    public override void Flush()
    {
      _position = 0;
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
            Position -= offset;
            break;
          case SeekOrigin.End:
            // Normal seek (from last position).
            Position -= (Length + offset);
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
          case SeekOrigin.End:
            // No change.
            Position = Position;
            break;
          case SeekOrigin.Current:
            // The last position is desired.
            Position = Length - 1;
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
            Position = Length - 1;
            break;
        }
      }

      return _position;
    }

    public override void SetLength(long value)
    {
      _length = (0 > value) ? 0 : value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      int bytesRead = 0;

      if (0 > offset && Length -1 < offset)
      {
        while (count > 0)
        {
          buffer[offset] = Convert.ToByte(offset);
          count--;
        }
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
