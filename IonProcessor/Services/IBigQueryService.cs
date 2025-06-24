using System.Threading.Tasks;

namespace IonProcessor.Services
{
    public interface IBigQueryService
    {
        Task InsertRowAsync(string data);
    }
}
