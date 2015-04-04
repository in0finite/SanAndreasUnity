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

        private long _position;

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
                return _position;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
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
            if (_position > _length) {
                return 0;
            }

            var basePos = _offset + _position;

            if (_baseStream.Position != basePos) {
                _baseStream.Seek(basePos, SeekOrigin.Begin);
            }

            var read = _baseStream.Read(buffer, offset, (int) Math.Min(_length - _position, count));
            _position += read;

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin) {
                case SeekOrigin.Begin:
                    _position = offset;
                    break;
                case SeekOrigin.Current:
                    _position += offset;
                    break;
                case SeekOrigin.End:
                    _position = _length - offset;
                    break;
            }

            if (_position < 0 || _position > _length) {
                throw new ArgumentOutOfRangeException("offset");
            }

            return _baseStream.Seek(_position + _offset, SeekOrigin.Begin) - _offset;
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
