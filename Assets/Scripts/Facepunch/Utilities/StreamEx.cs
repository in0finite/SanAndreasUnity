namespace System.IO
{
    public static class StreamEx
    {
        public static void Clear(this Stream stream)
        {
            stream.Reset();
            stream.SetLength(0);
        }

        public static void Reset(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        public static Stream ClearWriteReset(this Stream stream, Action<Stream> write)
        {
            stream.Clear();
            write(stream);
            stream.Reset();

            return stream;
        }

        public static Stream ClearWriteReset(this Stream stream, byte[] contents)
        {
            stream.Clear();
            stream.Write(contents, 0, contents.Length);
            stream.Reset();

            return stream;
        }

        private const int DefaultCopyBufferSize = 2048;

        public static void CopyTo(this Stream from, Stream dest, byte[] buffer = null)
        {
            buffer = buffer ?? new byte[DefaultCopyBufferSize];
            var bufferSize = buffer.Length;

            int read;
            while ((read = from.Read(buffer, 0, bufferSize)) > 0) {
                dest.Write(buffer, 0, read);
            }
        }

        public static void CopyTo(this Stream from, Stream dest, int length, byte[] buffer = null)
        {
            buffer = buffer ?? new byte[DefaultCopyBufferSize];
            var bufferSize = buffer.Length;

            int toRead, read, total = 0;
            while ((toRead = Math.Min(length - total, bufferSize)) > 0
                && (read = from.Read(buffer, 0, toRead)) > 0) {
                dest.Write(buffer, 0, read);
                total += read;
            }
        }
    }
}
