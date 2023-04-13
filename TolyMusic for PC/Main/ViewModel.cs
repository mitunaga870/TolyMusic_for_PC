using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NAudio.Wave;

namespace TolyMusic_for_PC
{
    public class ViewModel : INotifyPropertyChanged
    {
        //変数宣言
        private bool excl;
        private object loop;
        private string type, page;
        private string curt_driver;
        private Track curt_track;
        private long curt_time;
        private long next_time;
        private int curt_queue_num;
        private long curt_length;

        public event PropertyChangedEventHandler PropertyChanged;

        //コンストラクタ
        public ViewModel()
        {
            type = "ライブラリ";
            page = "曲";
            excl = false;
            next_time = -1;
        }

        //プロパティ
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public long Next_time
        {
            get { return next_time; }
            set
            {
                next_time = value;
                OnPropertyChanged();
            }
        }
        public long Curt_length
        {
            get { return curt_length; }
            set
            {
                curt_length = value;
                OnPropertyChanged();
            }
        }
        
        public long Curt_time
        {
            get { return curt_time; }
            set
            {
                curt_time = value;
                OnPropertyChanged();
            }
        }
        
        public object Loop
        {
            get { return loop; }
            set
            {
                loop = value;
                OnPropertyChanged();
            }
        }
        
        public bool Excl
        {
            get { return excl; }
            set
            {
                excl = value;
                OnPropertyChanged();
            }
        }
        
        public string Type
        {
            get { return type; }
            set
            {
                type = value;
                OnPropertyChanged();
            }
        }

        public string Page
        {
            get { return page; }
            set
            {
                page = value;
                OnPropertyChanged();
            }
        }

        public Track Curt_track
        {
            get { return curt_track; }
            set
            {
                curt_track = value;
                OnPropertyChanged();
            }
        }
        
        public string Curt_Driver
        {
            get { return curt_driver; }
            set
            {
                curt_driver = value;
                OnPropertyChanged();
            }
        }
        
        public int Curt_queue_num
        {
            get { return curt_queue_num; }
            set
            {
                curt_queue_num = value;
                OnPropertyChanged();
            }
        }
        
        public ObservableCollection<Track> Tracks { get; set; }

        public ObservableCollection<Track> PlayQueue { get; set; }
        
        public ObservableCollection<Album> Albums { get; set; }

        public ObservableCollection<Artist> Artists { get; set; }
        
        public ObservableCollection<string> Directories { get; set; }
    }
}