using System.Windows;
using System.Windows.Forms; // NotifyIcon 트레이 아이콘을 사용하기 위해 필요
using System.Drawing;
using WOLClient.Services;

namespace WOLClient
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon _notifyIcon;
        private BackgroundService _backgroundService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize NotifyIcon
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = SystemIcons.Information;
            _notifyIcon.Text = "WOLClient is running";
            _notifyIcon.Visible = true;
            _notifyIcon.DoubleClick += NotifyIconDoubleClick;

            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, OnExitClicked);

            _backgroundService = new BackgroundService();
            _backgroundService.Start();

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        private void NotifyIconDoubleClick(object sender, EventArgs e)
        {
            System.Windows.MessageBox.Show("WOLClient is running in the background.", "WOLClient Status", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnExitClicked(object? sender, EventArgs e)
        {
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _backgroundService?.Stop();
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
