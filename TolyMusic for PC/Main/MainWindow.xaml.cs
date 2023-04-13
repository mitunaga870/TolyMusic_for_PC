using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using NAudio.Wave;
using Label = System.Windows.Controls.Label;

namespace TolyMusic_for_PC
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private PageController pageController;
        private Player Player;
        private ViewModel vm;
        private bool seek_playing;
        //コンストラクタ
        public MainWindow()
        {
            InitializeComponent();
            vm = new ViewModel();
            DataContext = vm;
            pageController = new PageController(vm);
            Player = new Player(vm);
            Go_library_tracks( null, null);
        }
        //終了処理
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Player.Dispose();
        }
        //ページ遷移イベント
        private void Go_library_tracks(object sender, RoutedEventArgs e)
        {
            pageController.go("library", "tracks", vm);
        }
        private void Go_library_albums(object sender, RoutedEventArgs e)
        {
            pageController.go("library", "albums", vm);
        }
        private void Go_library_artists(object sender, RoutedEventArgs e)
        {
            pageController.go("library", "artists", vm);
        }
        private void Go_library_playlists(object sender, RoutedEventArgs e)
        {
            pageController.go("library", "playlists", vm);
        }
        private void Go_local_tracks(object sender, RoutedEventArgs e)
        {
            pageController.go("local", "tracks", vm);
            ContentList.ItemsSource = vm.Tracks;
        }
        private void Go_local_albums(object sender, RoutedEventArgs e)
        {
            pageController.go("local", "albums", vm);
        }
        private void Go_local_artists(object sender, RoutedEventArgs e)
        {
            pageController.go("local", "artists", vm);
        }
        private void Go_local_playlists(object sender, RoutedEventArgs e)
        {
            pageController.go("local", "playlists", vm);
        }
        //トラック再生
        private void PlayTrack(object sender, RoutedEventArgs e)
        {
            //キューの割当
            vm.Curt_track = (Track)ContentList.SelectedItem;
            vm.PlayQueue = (ObservableCollection<Track>)ContentList.ItemsSource;
            for (int i = 0; i < vm.Tracks.Count; i++)
            {
                if(vm.Curt_track.id==vm.PlayQueue[i].id)
                    vm.Curt_queue_num = i;
            }
            //再生
            Player.Start();
        }
        //再生ボタン
        private void Toggle_Player(object sender, RoutedEventArgs e)
        {
            if (Player.isPlaying)
            {
                Player.Pause();
            }
            else
            {
                Player.Play();
            }
        }
        
        private void Open_Settings(object sender, RoutedEventArgs e)
        {
            Setting setting = new Setting();
            setting.Owner = this;
            setting.Show();
        }

        private void Seeked(object sender, RoutedEventArgs e)
        {
            vm.Next_time = (long)Seekbar.Value;
            if (seek_playing)
                Player.Play();
        }
        
        private void Seeking(object sender, RoutedEventArgs e)
        {
            seek_playing = Player.isPlaying;
            if (seek_playing)
                Player.Pause();
        }

        private void SetExcl(object sender, RoutedEventArgs e)
        {
            long tmp = vm.Curt_time;
            Player.Start();
            vm.Next_time = tmp;
        }
    }
}