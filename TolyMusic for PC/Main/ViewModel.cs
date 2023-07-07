using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace TolyMusic_for_PC
{
    public class ViewModel : INotifyPropertyChanged
    {
        public enum TypeEnum
        {
            Track = 0,
            All = 0,
            Album = 1,
            Playlist = 1,
            Artist = 2,
        }
        public enum LocationEnum
        {
            Local = 0,
            Youtube = 1,
            ToIS = 2,
        }
        //変数宣言
        private bool excl;
        private object loop;
        private string type, page;
        private Driver share_driver;
        private Driver excl_driver;
        private Track curt_track;
        private long curt_time;
        private long next_time;
        private int curt_queue_num;
        private long curt_length;
        private int queue_bt_height;
        private int queue_list_height;
        private int volume;
        private bool enable_volume;
        private Collection<string> prev_page = new Collection<string>();
        private Collection<TypeEnum> listtypes = new Collection<TypeEnum>();
        private TypeEnum curttype;
        private Artist curt_artist;
        private Album curt_album;
        //public変数
        public bool skip;
        public string Preoperty_Id;
        public bool isOnline;
        public string Curt_YoutubeId;
        public event PropertyChangedEventHandler PropertyChanged;

        //コンストラクタ
        public ViewModel()
        {
            type = "ライブラリ";
            page = "曲";
            Excl = false;
            next_time = -1;
            Queue_bt_height = 0;
            Queue_list_height = 0;
            Volume = 100;
            Listtypes = new ObservableCollection<TypeEnum>();
            Load_Settings();
        }

        public void Load_Settings()
        {
            var setting = Properties.Settings.Default;
            if (setting.SDcustumized)
            {
                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                foreach (var wasMMD in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                {
                    if (wasMMD.DeviceFriendlyName == setting.ShareDriver)
                    {
                        Share_Driver = new Driver(wasMMD);
                        break;
                    }
                }
                enumerator.Dispose();
            }
            if (setting.EDcustumized)
            {
                if (setting.EDisASIO)
                    Excl_Driver = new Driver(setting.ExclutionDriver);
                else
                {
                    MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                    foreach (var wasMMD in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                    {
                        if (wasMMD.DeviceFriendlyName == setting.ExclutionDriver)
                        {
                            Excl_Driver = new Driver(wasMMD);
                            break;
                        }
                    }
                    enumerator.Dispose();
                }
            }
        }
        //プロパティ
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public Album Curt_Album
        {
            get { return curt_album; }
            set
            {
                curt_album = value;
                OnPropertyChanged();
            }
        }
        public Artist Curt_Artist
        {
            get { return curt_artist; }
            set
            {
                curt_artist = value;
                OnPropertyChanged();
            }
        }
        public bool Enable_volume
        {
            get { return enable_volume; }
            set
            {
                enable_volume = value;
                OnPropertyChanged();
            }
        }
        public int Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                OnPropertyChanged();
            }
        }
        public int Queue_bt_height
        {
            get { return queue_bt_height; }
            set
            {
                queue_bt_height = value;
                OnPropertyChanged();
            }
        }
        public int Queue_list_height
        {
            get { return queue_list_height; }
            set
            {
                queue_list_height = value;
                OnPropertyChanged();
            }
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
                Enable_volume = !value;
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
        
        public Driver Share_Driver
        {
            get { return share_driver; }
            set
            {
                share_driver = value;
                OnPropertyChanged();
            }
        }
        public Driver Excl_Driver
        {
            get { return excl_driver; }
            set
            {
                excl_driver = value;
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
        public string Prev_title
        {
            get
            {
                if(prev_page.Count == 0)
                    return "";
                string res = prev_page[prev_page.Count - 1];
                prev_page.RemoveAt(prev_page.Count - 1);
                return res;
            }
            set
            {
                prev_page.Add(value);
                OnPropertyChanged();
            }
        }

        public TypeEnum Curttype
        {
            get { return curttype; }
            set
            {
                curttype = value;
                OnPropertyChanged();
            }
        }
        //リストのアイテムのリスト
        public TypeEnum? Listtype
        {
            get
            {
                if (Listtypes.Count==0)
                {
                    return null;
                }
                return Listtypes[Listtypes.Count - 1];
            }
        }
        public ObservableCollection<TypeEnum> Listtypes { set; get; }
        
        //シャッフル・順再生のためのフィルターのリスト
        public string Filter
        {
            get
            {
                if (filters.Count == 0)
                {
                    return null;
                }

                return filters[filters.Count - 1];
            }
            set
            {
                if (value == null)
                {
                    filters.RemoveAt(filters.Count - 1);
                    return;
                }
                if (value == String.Empty)
                {
                    filters.Clear();
                    return;
                }
                filters.Add(value);
            }
        }
        public TypeEnum Filtertype
        {
            get
            {
                if (Listtypes.Count == 0)
                {
                    return TypeEnum.All;
                }

                return Listtypes[Listtypes.Count - 2];
            }
        }
        private Collection<string> filters = new Collection<string>();
        public ObservableCollection<Track> Tracks { get; set; }
        public ObservableCollection<Track> PlayQueue { get; set; }
        public ObservableCollection<Album> Albums { get; set; }
        public ObservableCollection<Artist> Artists { get; set; }
    }
}