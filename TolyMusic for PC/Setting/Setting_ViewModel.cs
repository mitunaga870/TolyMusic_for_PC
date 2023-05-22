using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using TolyMusic_for_PC.Local;

namespace TolyMusic_for_PC
{
    public class Setting_ViewModel : INotifyPropertyChanged
    {
        //private変数
        private string tmp_path;
        private int selected_share;
        private int selectid_excl;
        private string database_sever_adress;
        private string database_sever_port;
        private string database_sever_user;
        private PasswordBox database_sever_password;
        private string youtubeplaylist;
        //コンストラクタ
        public Setting_ViewModel()
        {
            //初期化
            Init();
        }

        public void Init()
        {
            var settingfile = Properties.Settings.Default;
            //settingfileを読み込み
            path_list = new ObservableCollection<string>();
            foreach (string path in Properties.Settings.Default.LocalDirectryPath.Split(','))
            {
                if(path == "")
                    continue;
                path_list.Add(path);
            }
            //driver
            Share_driver_list = new ObservableCollection<Driver>();
            Share_driver_list.Add(new Driver());
            Excl_driver_list = new ObservableCollection<Driver>();
            Excl_driver_list.Add(new Driver());
            //接続デバイス読み込み
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            foreach (var wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Render,DeviceState.Active))
            {
                Excl_driver_list.Add(new Driver(wasapi));
                Share_driver_list.Add(new Driver(wasapi));
            }
            foreach (var asio in AsioOut.GetDriverNames())
                Excl_driver_list.Add(new Driver(asio));
             enumerator.Dispose();
             //setting繁栄
             for (int i = 1; i < Share_driver_list.Count; i++)
             {
                 if (settingfile.ShareDriver == Share_driver_list[i].Name)
                 {
                     Selected_share = i;
                     break;
                 }
                 if (i + 1 == Share_driver_list.Count)
                     Selected_share = 0;
             }
             for (int i = 1; i < Excl_driver_list.Count; i++)
             {
                 if (settingfile.ExclutionDriver == Excl_driver_list[i].Name)
                 {
                     Selected_excl = i;
                     break;
                 }

                 if (i + 1 == Excl_driver_list.Count)
                     Selected_excl = 0;
             }
             //DB
             DatabaseSeverAdress = settingfile.LibraryServerAdress;
             DatabaseSeverPort = settingfile.LibraryServerPort;
             DatabaseSeverUser = settingfile.LibraryServerUser;
             database_sever_password = new PasswordBox();
             //streaming
             //youtube
             YoutubePlaylist = settingfile.YoutubePlaylist;
        }
        //変更時処理
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        //プロパティ
        public ObservableCollection<string> path_list { set; get; }
        public ObservableCollection<Driver> Share_driver_list { set; get; }
        public ObservableCollection<Driver> Excl_driver_list { set; get; }

        public string YoutubePlaylist
        {
            get { return youtubeplaylist;}
            set
            {
                youtubeplaylist = value;
                OnPropertyChanged();
            }
        }
        public int Selected_excl
        {
            get { return selectid_excl;}
            set
            {
                selectid_excl = value;
                OnPropertyChanged();
            }
        }
        public int Selected_share
        {
            get { return selected_share;}
            set
            {
                selected_share = value;
                OnPropertyChanged();
            }
        }
        public string Tmp_Path
        {
            get { return tmp_path; }
            set
            {
                tmp_path = value;
                OnPropertyChanged();
            }
        }

        public string DatabaseSeverAdress
        {
            get { return database_sever_adress; }
            set
            {
                database_sever_adress = value;
                OnPropertyChanged();
            }
        }

        public string DatabaseSeverPort
        {
            get { return database_sever_port;}
            set
            {
                database_sever_port = value;
                OnPropertyChanged();
            }
        }

        public string DatabaseSeverUser
        {
            get { return database_sever_user;}
            set
            {
                database_sever_user = value;
                OnPropertyChanged();
            }
        }

        public PasswordBox DatabaseSeverPassword
        {
            get { return database_sever_password;}
            set
            {
                database_sever_password = value;
                OnPropertyChanged();
            }
        }
    }
}