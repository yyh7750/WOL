using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace WOL.Models
{
    public enum DeviceStatus
    {
        Online,
        Offline,
        Checking
    }

    [Table("device")]
    public class Device : INotifyPropertyChanged
    {
        [Key]
        private int _id;
        private string _name;
        private string _ip;
        private string _mac;
        private DeviceStatus _status;
        private DateTime _lastHeartbeat;
        private ObservableCollection<Program> _programs;

        public Device()
        {
            _programs = [];
            _status = DeviceStatus.Offline;
        }

        [Column("id")]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        [Column("project_id")]
        public int ProjectId { get; set; }

        [Column("_name")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [Column("ip")]
        public string IP
        {
            get => _ip;
            set => SetProperty(ref _ip, value);
        }

        [Column("mac")]
        public string MAC
        {
            get => _mac;
            set => SetProperty(ref _mac, value);
        }

        [NotMapped]
        public DeviceStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        [NotMapped]
        public DateTime LastHeartbeat
        {
            get => _lastHeartbeat;
            set => SetProperty(ref _lastHeartbeat, value);
        }

        public ObservableCollection<Program> Programs
        {
            get => _programs;
            set => SetProperty(ref _programs, value);
        }

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