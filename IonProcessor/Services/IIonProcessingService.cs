using System.IO;
using System.Threading.Tasks;

namespace IonProcessor.Services
{
    public interface IIonProcessingService
    {
        Task<string> ProcessIonFileAsync(string bucketName, string objectName);
    }
}
