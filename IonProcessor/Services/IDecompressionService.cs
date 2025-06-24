using System.IO;

namespace IonProcessor.Services
{
    public interface IDecompressionService
    {
        Stream Decompress(Stream compressedStream);
    }
}
