using CommunityToolkit.Mvvm.ComponentModel;

namespace ViewModels
{
    [ObservableObject]
    public partial class TrackViewModel
    {
        [ObservableProperty] 
        private TrackMetadata metadata;
    }
}
