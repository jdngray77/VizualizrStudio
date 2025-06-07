using ServiceInterfaces;
using Vizualizr.Views;

#if WINDOWS
using System.Diagnostics;
#endif

namespace Vizualizr.MauiServices
{
    internal class MauiServices : INonCommonServices
    {
#if WINDOWS
        private PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
#endif

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
#if WINDOWS
            return cpuCounter.NextValue();
#else
            // assume unix.
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"top -l 1 | grep 'CPU usage'\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Parse output like: "CPU usage: 10.35% user, 5.00% sys, 84.64% idle"
            var idlePart = output.Split(',').FirstOrDefault(s => s.Contains("idle"));
            var idlePercent = float.Parse(Regex.Match(idlePart ?? "0", @"\d+(\.\d+)?").Value);
            return 100 - idlePercent;
#endif

        }

        public float GetGpuUsage()
        {
            try
            {
                var gpuCounters = GetGPUCounters();
                return GetGPUUsage(gpuCounters);
            } catch (Exception ex)
            {
                var gpuCounters = GetGPUCounters(true);
                return GetGPUUsage(gpuCounters);
            }
        }

        private PerformanceCounterCategory category = new PerformanceCounterCategory("GPU Engine");
        private List<PerformanceCounter>? gpuCounters = null;
        private List<PerformanceCounter> GetGPUCounters(bool force = false)
        {
            if (this.gpuCounters != null && !force)
            {
                return this.gpuCounters;
            }

            var counterNames = category.GetInstanceNames();

            var gpuCounters = counterNames
                                .Where(counterName => counterName.EndsWith("engtype_3D"))
                                .SelectMany(counterName => category.GetCounters(counterName))
                                .Where(counter => counter.CounterName.Equals("Utilization Percentage"))
                                .ToList();

            this.gpuCounters = gpuCounters;
            return gpuCounters;
        }

        public static float GetGPUUsage(List<PerformanceCounter> gpuCounters)
        {
            return gpuCounters.Select(x => x.NextValue()).Max();
        }
    }
}
