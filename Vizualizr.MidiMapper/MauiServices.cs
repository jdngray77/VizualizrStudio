using ServiceInterfaces;
using System.Diagnostics;
using Vizualizr.MidiMapper.ViewModels;
using Vizualizr.MidiMapper.Views;

namespace Vizualizr.MidiMapper;

public class MauiServices : INonCommonServices
{
    public async Task<InputMappingViewModel?> DisplayAddInputMappingDialog(string deviceName)
    {
        var vm = Shell.Current.Handler.MauiContext.Services.GetRequiredService<AddInputMappingDialogViewModel>();
        vm.DeviceName = deviceName;
        var dialog = new AddInputMappingDialog(vm);
        await Shell.Current.Navigation.PushModalAsync(dialog);
        return await dialog.ShowAsync();
    }

    public async Task<bool> DisplayAlert(string title, string body, string yes, string no)
    {
        var alert = new ModalAlert(title, body, yes, no);
        await Shell.Current.Navigation.PushModalAsync(alert);
        bool result = await alert.ShowAsync();
        return result;
    }

    public double GetBatteryPercentage()
    {
        return Battery.ChargeLevel;
    }

    public bool OnBatteryPower()
    {
        return Battery.PowerSource == BatteryPowerSource.Battery;
    }

    public float GetCpuUsage()
    {
        return 0;
    }

    public float GetGpuUsage()
    {
        return 0;
    }

    private List<PerformanceCounter> GetGPUCounters(bool force = false)
    {
        return null;
    }

    public static float GetGPUUsage(List<PerformanceCounter> gpuCounters)
    {
        return 0;
    }
}
