using System.IO;

namespace IonProcessor.Services
{
    public class NoOpDecompressionService : IDecompressionService
    {
        public Stream Decompress(Stream compressedStream)
        {
            // This is a placeholder implementation that does nothing.
            // When the actual decompression algorithm is known, this
            // method will be updated to decompress the stream.
            return compressedStream;
        }
    }
}
