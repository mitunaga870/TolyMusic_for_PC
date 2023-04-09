using System.ComponentModel;
using System.Windows;
using Label = System.Windows.Controls.Label;

namespace TolyMusic_for_PC
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        PageController pageController;
        private ViewModel vm;
        public static Label typelabel, pagelabel;
        //コンストラクタ
        public MainWindow()
        {
            InitializeComponent();
            vm = new ViewModel();
            DataContext = vm;
            pageController = new PageController(vm);
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
        
    }
}
