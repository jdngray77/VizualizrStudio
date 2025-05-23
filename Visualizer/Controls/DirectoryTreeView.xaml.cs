using ViewModels.FileBrowser;

namespace Visualizer.Controls
{
    public partial class DirectoryTreeView : CollectionView
    {
        public DirectoryTreeView()
        {
            InitializeComponent();
        }

        private const int DoubleTapTimeout = 300; // ms between taps to consider double tap
        private DateTime _lastTapTime = DateTime.MinValue;
        private object _lastTappedItem = null;
        private bool _isDoubleTapPending = false;

        private async void OnGridTapped(object sender, EventArgs e)
        {
            if (sender is VisualElement grid)
            {
                var tappedItem = grid.BindingContext;  // The item bound to this DataTemplate

                var now = DateTime.Now;
                var timeSinceLastTap = (now - _lastTapTime).TotalMilliseconds;

                if (timeSinceLastTap < DoubleTapTimeout && tappedItem == _lastTappedItem)
                {
                    // Double tap detected on the same item
                    _isDoubleTapPending = false;
                    _lastTapTime = DateTime.MinValue;
                    _lastTappedItem = null;

                    // Execute your double tap logic
                    var vm = this.BindingContext as DirectoryTreeViewModel;
                    vm?.ToggleNodeCommand.Execute(tappedItem);
                }
                else
                {
                    _isDoubleTapPending = true;
                    _lastTapTime = now;
                    _lastTappedItem = tappedItem;

                    // Wait for possible double tap
                    await Task.Delay(DoubleTapTimeout);

                    if (_isDoubleTapPending)
                    {
                        _isDoubleTapPending = false;
                        _lastTappedItem = null;

                        // Execute your single tap logic
                        var vm = this.BindingContext as DirectoryTreeViewModel;
                        vm?.SelectNodeCommand.Execute(tappedItem);
                    }
                }
            }
        }

    }
}