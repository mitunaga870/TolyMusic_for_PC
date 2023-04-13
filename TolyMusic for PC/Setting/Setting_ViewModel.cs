using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TolyMusic_for_PC.Local;

namespace TolyMusic_for_PC
{
    public class Setting_ViewModel : INotifyPropertyChanged
    {
        //private変数
        string tmp_path;
        //コンストラクタ
        public Setting_ViewModel()
        {
            //初期化
            Init();
        }

        public void Init()
        {
            //settingfileを読み込み
            path_list = new ObservableCollection<string>();
            foreach (string path in Properties.Settings.Default.LocalDirectryPath.Split(','))
            {
                if(path == "")
                    continue;
                path_list.Add(path);
            }
            Share_device_list = new ObservableCollection<object>();
            Share_device_list.Add("デフォルトデバイス");
            Excl_device_list = new ObservableCollection<object>();
            Excl_device_list.Add("デフォルトデバイス");
        }


        //変更時処理
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        //プロパティ
        public ObservableCollection<string> path_list { set; get; }
        public ObservableCollection<object> Share_device_list { set; get; }
        public ObservableCollection<object> Excl_device_list { set; get; }
        public string Tmp_Path
        {
            get { return tmp_path; }
            set
            {
                tmp_path = value;
                OnPropertyChanged();
            }
        }
    }
}