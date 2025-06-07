namespace Vizualizr.Backend.Utility
{
    using System;
    using System.Timers;

    public class ResettableTimer
    {
        private readonly Timer _timer;
        private readonly double _interval;

        public event Action TimerElapsedWithoutReset;

        public ResettableTimer(double intervalMilliseconds)
        {
            _interval = intervalMilliseconds;
            _timer = new Timer(_interval);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = false;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            TimerElapsedWithoutReset?.Invoke();
        }

        public void Reset()
        {
            _timer.Stop();
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Start()
        {
            _timer.Start();
        }

        public bool IsRunning => _timer.Enabled;
    }

}
