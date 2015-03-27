using System;
using System.IO;

namespace SanAndreasUnity.Utilities
{
    /// <summary>
    /// Represents a subsection of another stream, starting from a certain offset and
    /// with a given length.
    /// </summary>
    public class FrameStream : Stream
    {
        private readonly Stream _baseStream;

        private readonly long _offset;
        private readonly long _length;

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

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get
            {
                return Math.Min(Math.Max(_baseStream.Position - _offset, 0), _length);
            }
            set
            {
                _baseStream.Seek(value + _offset, SeekOrigin.Begin);
            }
        }

        public FrameStream(Stream baseStream, long offset, long length)
        {
            _baseStream = baseStream;

            _offset = offset;
            _length = length;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_baseStream.Position < _offset) {
                _baseStream.Seek(_offset, SeekOrigin.Begin);
            }

            if (_baseStream.Position > _offset + _length) {
                return 0;
            }

            return _baseStream.Read(buffer, offset, (int) Math.Min(_length - Position, count));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin) {
                case SeekOrigin.Begin:
                    offset += _offset; break;
                case SeekOrigin.Current:
                    offset += _baseStream.Position; break;
                case SeekOrigin.End:
                    offset += _offset + _length; break;
            }

            if (offset < _offset || offset > _offset + _length) {
                throw new ArgumentOutOfRangeException("offset");
            }

            return _baseStream.Seek(offset, SeekOrigin.Begin) - _offset;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
