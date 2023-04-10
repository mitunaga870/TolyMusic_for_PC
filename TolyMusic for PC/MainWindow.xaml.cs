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
        public static Label typelabel, pagelabel;
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
            vm.Curt_Driver = AsioOut.GetDriverNames()[0];
            vm.Curt_track = (Track)ContentList.SelectedItem;
            vm.PlayQueue = (ObservableCollection<Track>)ContentList.ItemsSource;
            for (int i = 0; i < vm.Tracks.Count; i++)
            {
                if(vm.Curt_track.id==vm.PlayQueue[i].id)
                    vm.Curt_queue_num = i;
            }
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
    }
}
