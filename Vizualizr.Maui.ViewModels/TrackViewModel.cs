using CommunityToolkit.Mvvm.ComponentModel;
using Vizualizr.Backend.Audio;

namespace ViewModels
{
    [ObservableObject]
    public partial class TrackViewModel
    {
        [ObservableProperty] 
        private TrackMetadata metadata;
    }
}
