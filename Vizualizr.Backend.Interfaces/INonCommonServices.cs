using System.Threading.Tasks;

namespace ServiceInterfaces
{
    /**
     * Contains utility methods for stuff that is specific to the final application framework.
     */
    public interface INonCommonServices
    {
        Task<bool> DisplayAlert(string title, string body, string yes, string no);

        double GetBatteryPercentage();
        bool OnBatteryPower();
        float GetCpuUsage();
        float GetGpuUsage();
    }
}
