using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WOL.Helpers.Interface;
using System.Windows.Input;
using WOL.Commands;

namespace WOL.ViewModels
{
    public class RemoteExplorerViewModel : INotifyPropertyChanged
    {
        private readonly ITcpJsonClient _tcp;
        private bool _connected;

        public string ClientHost { get; set; } = "127.0.0.1";
        public int ClientPort { get; set; } = 6060;

        private string _currentPath = "C:\\";
        public string CurrentPath { get => _currentPath; set { _currentPath = value; OnPropertyChanged(); } }

        public ObservableCollection<TreeNode> TreeRoots { get; } = [];
        public ObservableCollection<EntryItem> Entries { get; } = [];

        private string _selectionInfo = string.Empty;
        public string SelectionInfo { get => _selectionInfo; set { _selectionInfo = value; OnPropertyChanged(); } }

        public RelayCommand ConnectCommand { get; }
        public RelayCommand NavigateCommand { get; }
        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand GoUpCommand { get; }
        public RelayCommand<EntryItem> DoubleClickItemCommand { get; }
        public RelayCommand<TreeNode> RootItemDoubleClickCommand { get; }

        public RemoteExplorerViewModel(ITcpJsonClient tcpJsonClient)
        {
            _tcp = tcpJsonClient;

            ConnectCommand = new RelayCommand(async () => await ConnectAsync());
            NavigateCommand = new RelayCommand(async () => await NavigateAsync(CurrentPath), () => _connected);
            OkCommand = new RelayCommand(() => ConfirmSelection(), () => CanConfirmSelection());
            CancelCommand = new RelayCommand(() => { foreach (EntryItem e in Entries) e.IsSelected = false; UpdateSelectionInfo(); });
            GoUpCommand = new RelayCommand(async () => await GoUpAsync(), () => _connected && CanGoUp());
            DoubleClickItemCommand = new RelayCommand<EntryItem>(async (item) => await DoubleClickItemAsync(item), (item) => item != null);
            RootItemDoubleClickCommand = new RelayCommand<TreeNode>(async (node) => await ExpandTreeNodeAsync(node), (node) => node != null);
        }

        private async Task DoubleClickItemAsync(EntryItem item)
        {
            if (item.IsDirectory || item.IsParentFolder)
            {
                await NavigateAsync(item.FullPath);
            }
        }

        private bool CanConfirmSelection()
        {
            List<EntryItem> selected = Entries.Where(x => x.IsSelected).ToList();
            return selected.Count != 0 && selected.All(x => !x.IsDirectory);
        }

        private bool CanGoUp()
        {
            if (string.IsNullOrEmpty(CurrentPath)) return false;
            string? parent = Path.GetDirectoryName(CurrentPath);
            return !string.IsNullOrEmpty(parent) && parent != CurrentPath;
        }

        public async Task ConnectAsync()
        {
            await _tcp.ConnectAsync(ClientHost, ClientPort);
            _connected = true;
            await LoadRootsAsync();
        }

        private async Task LoadRootsAsync()
        {
            List<EntryDto> resp = await _tcp.SendAsync<List<EntryDto>>(new { Type = "Roots" }) ?? [];
            TreeRoots.Clear();
            foreach (EntryDto r in resp.Where(x => x.IsDirectory))
                TreeRoots.Add(new TreeNode(r.FullPath, r.Name, this));
            if (TreeRoots.Count > 0)
            {
                CurrentPath = TreeRoots[0].FullPath;
                await NavigateAsync(CurrentPath);
            }
        }

        public async Task NavigateAsync(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            CurrentPath = path;

            List<EntryDto> list = await _tcp.SendAsync<List<EntryDto>>(new { Type = "List", Path = path, Skip = 0, Take = 500 }) ?? new();
            Entries.Clear();

            string? parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent))
            {
                EntryDto up = new(parent, "..", true, null, DateTime.UtcNow);
                Entries.Add(new EntryItem(up, this));
            }

            foreach (EntryDto e in list)
                Entries.Add(new EntryItem(e, this));

            UpdateSelectionInfo();
            CommandManager.InvalidateRequerySuggested();
        }

        private async Task GoUpAsync()
        {
            string? parent = Path.GetDirectoryName(CurrentPath);
            if (!string.IsNullOrEmpty(parent))
            {
                await NavigateAsync(parent);
            }
        }

        private async Task ExpandTreeNodeAsync(TreeNode node)
        {
            if (node == null) return;

            await NavigateAsync(node.FullPath);

            if (node.Children.Any())
            {
                node.IsExpanded = true;
                return;
            }

            List<EntryDto> list = await _tcp.SendAsync<List<EntryDto>>(new { Type = "List", Path = node.FullPath, Skip = 0, Take = 500 }) ?? [];

            foreach (EntryDto e in list.Where(x => x.IsDirectory))
            {
                node.Children.Add(new TreeNode(e.FullPath, e.Name, this));
            }

            node.IsExpanded = true;
        }

        private void ConfirmSelection()
        {
            List<EntryItem> selected = Entries.Where(x => x.IsSelected && !x.IsDirectory).ToList();
            if (selected.Count == 0) return;
            SelectedFiles = selected.Select(x => new EntryDto(x.FullPath, x.Name, x.IsDirectory, x.Size, x.ModifiedUtc)).ToList();
            OnFilesSelected(SelectedFiles);
        }

        public List<EntryDto> SelectedFiles { get; private set; } = [];
        public event Action<List<EntryDto>>? FilesSelected;
        internal bool FilesSelectedHasSubscriber { get; set; }
        protected virtual void OnFilesSelected(List<EntryDto> files) => FilesSelected?.Invoke(files);

        internal void UpdateSelectionInfo()
        {
            int count = Entries.Count(e => e.IsSelected);
            int fileCount = Entries.Count(e => e.IsSelected && !e.IsDirectory);
            int folderCount = count - fileCount;
            if (count == 0) SelectionInfo = "No items selected";
            else if (fileCount > 0 && folderCount == 0) SelectionInfo = $"{fileCount} file(s) selected - OK to confirm";
            else if (folderCount > 0 && fileCount == 0) SelectionInfo = $"{folderCount} folder(s) selected - Double-click to navigate";
            else SelectionInfo = $"{count} items selected ({fileCount} files, {folderCount} folders)";
            CommandManager.InvalidateRequerySuggested();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public record EntryDto(string FullPath, string Name, bool IsDirectory, long? Size, DateTime ModifiedUtc);

    public class EntryItem : INotifyPropertyChanged
    {
        public string Name { get; }
        public string Type { get; }
        public string SizeDisplay { get; }
        public string ModifiedLocal { get; }
        public string FullPath { get; }
        public bool IsDirectory { get; }
        public bool IsParentFolder { get; }
        public long? Size { get; }
        public DateTime ModifiedUtc { get; }
        private readonly RemoteExplorerViewModel _vm;

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); _vm.UpdateSelectionInfo(); } }

        public EntryItem(EntryDto e, RemoteExplorerViewModel vm)
        {
            _vm = vm;
            FullPath = e.FullPath;
            Name = e.Name;
            IsDirectory = e.IsDirectory;
            IsParentFolder = e.Name == "..";
            Size = e.Size;
            ModifiedUtc = e.ModifiedUtc;
            if (IsParentFolder)
            {
                Type = "Parent";
                SizeDisplay = string.Empty;
                ModifiedLocal = string.Empty;
            }
            else
            {
                Type = e.IsDirectory ? "Folder" : Path.GetExtension(e.FullPath).Trim('.').ToUpperInvariant();
                SizeDisplay = e.IsDirectory ? string.Empty : FormatSize(e.Size ?? 0);
                ModifiedLocal = e.ModifiedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        static string FormatSize(long b)
        {
            string[] u = { "B", "KB", "MB", "GB", "TB" };
            double v = b; int i = 0; while (v >= 1024 && i < u.Length - 1) { v /= 1024; i++; }
            return $"{v:0.##} {u[i]}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class TreeNode : INotifyPropertyChanged
    {
        public string Name { get; }
        public string FullPath { get; }
        public ObservableCollection<TreeNode> Children { get; } = [];

        private bool _isExpanded;
        public bool IsExpanded { get => _isExpanded; set { _isExpanded = value; OnPropertyChanged(); } }

        public TreeNode(string fullPath, string name, RemoteExplorerViewModel vm) { FullPath = fullPath; Name = name; }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
