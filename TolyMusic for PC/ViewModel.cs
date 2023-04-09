using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TolyMusic_for_PC
{
    public class ViewModel : INotifyPropertyChanged
    {
        //変数宣言
        private string type, page;
        private string curt_driver;
        private Track curt_track;

        public event PropertyChangedEventHandler PropertyChanged;

        //コンストラクタ
        public ViewModel()
        {
            type = "ライブラリ";
            page = "曲";
        }

        //プロパティ
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
        
        public ObservableCollection<Track> Tracks { get; set; }

        public ObservableCollection<Album> Albums { get; set; }

        public ObservableCollection<Artist> Artists { get; set; }
    }
}