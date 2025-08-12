using WOL.Commands;
using WOL.Models;
using System.Windows;
using System.Windows.Input;

namespace WOL.ViewModels
{
    public class NewDeviceViewModel
    {
        public Device Device { get; private set; }

        public NewDeviceViewModel()
        {
            Device = new Device(); // Default to a new device
            SetDeviceInfoCommand = new RelayCommand<Window>(SetDeviceInfo);
        }

        public void Initialize(Device? device = null)
        {
            Device = device ?? new Device();
        }

        public ICommand SetDeviceInfoCommand { get; }

        private void SetDeviceInfo(Window? window)
        {
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }
    }
}