using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ViewModels.TreeView;

namespace ViewModels.FileBrowser
{
    [ObservableObject]
    public partial class DirectoryTreeViewModel
    {
        public DirectoryTreeViewModel()
        {
            DirectoryTree = new ObservableCollection<DirectoryNode>();
            FlattenedTree = new ObservableCollection<DirectoryNode>();

            //var root = LoadDirectoryTree("D:\\Music\\BigFolder\\");
            var root = LoadDirectoryTree("D:\\Music\\iTunes Library\\Music");
            root.IsExpanded = true;

            DirectoryTree.Add(root);
            RefreshFlattenedTree();
        }

        [ObservableProperty]
        private ObservableCollection<DirectoryNode> directoryTree;

        [ObservableProperty]
        private ObservableCollection<DirectoryNode> flattenedTree;

        [ObservableProperty]
        private DirectoryNode selectedDirectory;

        private DirectoryNode LoadDirectoryTree(string path, int depth = 0)
        {
            var node = new DirectoryNode
            {
                Name = Path.GetFileName(Path.GetDirectoryName(path)) ?? path,
                FullPath = path,
                DepthFromRootDirectory = depth
            };

            try
            {
                foreach (var dir in Directory.GetDirectories(path).Reverse())
                {
                    node.Children.Add(LoadDirectoryTree(dir + "/", depth + 1));
                }
            }
            catch { }

            return node;
        }

        private void RemoveChildrenRecursive(DirectoryNode node)
        {
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    RemoveChildrenRecursive(child);
                    FlattenedTree.Remove(node);
                }
            }
        }

        private void AddChildrenRecursive(DirectoryNode node, int depth)
        {
            node.PaddingLeft = depth * 20;

            if (node.IsExpanded)
            {
                foreach (var child in node.Children)
                {
                    FlattenedTree.Insert(FlattenedTree.IndexOf(node) + 1, child);
                    AddChildrenRecursive(child, depth + 1);
                }
            }
        }

        public void RefreshFlattenedTree()
        {
            flattenedTree.Clear();
            foreach (var node in DirectoryTree)
            {
                node.DepthFromRootDirectory = 0;
                flattenedTree.Add(node);
                AddChildrenRecursive(node, 1);
            }
        }

        [RelayCommand]
        private void ToggleNode(DirectoryNode node)
        {
            node.IsExpanded = !node.IsExpanded;

            if (!node.IsExpanded)
            {
                RemoveChildrenRecursive(node);
            } else
            {
                AddChildrenRecursive(node, node.DepthFromRootDirectory + 1);
            }
        }

        [RelayCommand]
        private void SelectNode(DirectoryNode node)
        {
            if (node == selectedDirectory)
            {
                return;
            }

            if (SelectedDirectory != null)
            {
                SelectedDirectory.IsSelected = false;
            }

            node.IsSelected = true;
            SelectedDirectory = node;
        }
    }
}