using System.Threading.Tasks;

namespace MvvmLight1.Model
{
    public interface IDataService
    {
        Task<DataItem> GetData();
    }
}