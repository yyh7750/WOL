using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WOL.Models
{
    public enum ProgramStatus
    {
        Running,
        Stopped
    }

    [Table("program")]
    public class Program : INotifyPropertyChanged
    {
        [Key]
        private int _id;
        private string _name = string.Empty;
        private string _path = string.Empty;
        private ProgramStatus _status;

        public Program()
        {
            _status = ProgramStatus.Stopped;
        }

        [Column("id")]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        [Column("device_id")]
        public int DeviceId { get; set; }

        [Column("_name")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [Column("_path")]
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        [NotMapped]
        public ProgramStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string StatusText => Status == ProgramStatus.Running ? "실행 중" : "중지됨";
        public string StatusColor => Status == ProgramStatus.Running ? "#10B981" : "#6B7280";

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