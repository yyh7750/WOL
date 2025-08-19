using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WOL.ViewModels;

namespace WOL.View
{
    public partial class RemoteExplorerView : Window
    {
        public System.Collections.Generic.List<EntryDto> SelectedFiles { get; private set; } = new();
        public RemoteExplorerView()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (DataContext is RemoteExplorerViewModel vm && !vm.FilesSelectedHasSubscriber)
            {
                vm.FilesSelected += files => { SelectedFiles = files; DialogResult = true; Close(); };
                vm.FilesSelectedHasSubscriber = true;
            }
        }

        private async void ListViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem lvi && lvi.DataContext is EntryItem item && DataContext is RemoteExplorerViewModel vm)
            {
                if (item.IsDirectory || item.IsParentFolder)
                {
                    await vm.NavigateAsync(item.FullPath);
                }
            }
        }
    }
}


