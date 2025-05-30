using System.Threading.Tasks;

namespace ServiceInterfaces
{
    public interface IAlertService
    {
        Task<bool> DisplayAlert(string title, string body, string yes, string no);
    }
}
