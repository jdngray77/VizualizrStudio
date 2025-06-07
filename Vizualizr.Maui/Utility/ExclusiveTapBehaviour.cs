namespace Vizualizr.Utility
{

    public class ExclusiveTapBehavior : Behavior<View>
    {
        public static readonly BindableProperty SingleTapCommandProperty =
            BindableProperty.Create(nameof(SingleTapCommand), typeof(Command), typeof(ExclusiveTapBehavior));

        public static readonly BindableProperty DoubleTapCommandProperty =
            BindableProperty.Create(nameof(DoubleTapCommand), typeof(Command), typeof(ExclusiveTapBehavior));

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ExclusiveTapBehavior));

        public Command SingleTapCommand
        {
            get => (Command)GetValue(SingleTapCommandProperty);
            set => SetValue(SingleTapCommandProperty, value);
        }

        public Command DoubleTapCommand
        {
            get => (Command)GetValue(DoubleTapCommandProperty);
            set => SetValue(DoubleTapCommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        private DateTime _lastTapTime = DateTime.MinValue;
        private bool _isDoubleTapPending = false;
        private const int TapThreshold = 300;

        protected override void OnAttachedTo(View bindable)
        {
            base.OnAttachedTo(bindable);
            var gesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
            gesture.Tapped += OnTapped;
            bindable.GestureRecognizers.Add(gesture);
        }

        private async void OnTapped(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var elapsed = (now - _lastTapTime).TotalMilliseconds;

            if (elapsed <= TapThreshold)
            {
                _isDoubleTapPending = false;
                _lastTapTime = DateTime.MinValue;
                DoubleTapCommand?.Execute(CommandParameter);
            }
            else
            {
                _isDoubleTapPending = true;
                _lastTapTime = now;

                await Task.Delay(TapThreshold);

                if (_isDoubleTapPending)
                {
                    _isDoubleTapPending = false;
                    SingleTapCommand?.Execute(CommandParameter);
                }
            }
        }

        protected override void OnDetachingFrom(View bindable)
        {
            base.OnDetachingFrom(bindable);
        }
    }
}
