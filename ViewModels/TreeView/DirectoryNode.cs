namespace ViewModels.TreeView
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using System.Collections.ObjectModel;

    public partial class DirectoryNode : ObservableObject
    {
        public DirectoryNode()
        {
            Children = new ObservableCollection<DirectoryNode>();
        }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string fullPath;

        [ObservableProperty]
        private ObservableCollection<DirectoryNode> children;

        [ObservableProperty]
        private bool isExpanded;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private int paddingLeft;

        [ObservableProperty]
        private int depthFromRootDirectory;

        public bool HasChildren => Children?.Count > 0;
    }
}
