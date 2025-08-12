using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace WOL.Models
{
    [Table("project")]
    public class Project : INotifyPropertyChanged
    {
        [Key]
        private int _id;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private ObservableCollection<Device> _devices;

        public Project()
        {
            _devices = [];
        }

        [Column("id")]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        [Column("_name")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [Column("_description")]
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public ObservableCollection<Device> Devices
        {
            get => _devices;
            set => SetProperty(ref _devices, value);
        }

        public int TotalDevices => Devices.Count;
        public int OnlineDevices => Devices.Count(d => d.Status == DeviceStatus.Online);
        public int TotalPrograms => Devices.Sum(d => d.Programs.Count);
        public int RunningPrograms => Devices.Sum(d => d.Programs.Count(p => p.Status == ProgramStatus.Running));

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
