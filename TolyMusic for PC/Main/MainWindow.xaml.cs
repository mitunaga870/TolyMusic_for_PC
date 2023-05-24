using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;
using CefSharp.OffScreen;
using TolyMusic_for_PC.Library;
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
        private Queue queue;
        private bool seek_playing;
        private bool queue_opened;
        private AddLibFunc lib;
        //コンストラクタ
        public MainWindow()
        {
            InitializeComponent();
            vm = new ViewModel();
            DataContext = vm;
            Player = new Player(vm,VPlayer);
            queue = new Queue(vm,queue_list);
            lib = new AddLibFunc(vm);
            pageController = new PageController(vm,MainGrid,PageFuncContainer,Player,queue);
            //Go_library_tracks( null, null);
            //CefSharp設定
            CefSharp.Wpf.CefSettings settings = new CefSharp.Wpf.CefSettings();
            settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 /CefSharp Browser" + Cef.CefSharpVersion;
            settings.Locale = "ja";
            settings.AcceptLanguageList = "ja,en-US;q=0.9,en;q=0.8";
            settings.CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache");
            settings.PersistSessionCookies = true;
            Cef.Initialize(settings);
        }
        //終了処理
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                Player.Dispose();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        //ページ切り替え事前処理
        private void closingPage(){
            PageFuncContainer.Children.Clear();
            MainGrid.Children.Clear();
            queue.hide();
        }
        //ページ遷移イベント
        private void Go_library_tracks(object sender, RoutedEventArgs e)
        {
            closingPage();
            pageController.go("library", "tracks");
        }
        private void Go_library_albums(object sender, RoutedEventArgs e)
        {
            closingPage();
            pageController.go("library", "albums");
        }
        private void Go_library_artists(object sender, RoutedEventArgs e)
        {
            closingPage();
            pageController.go("library", "artists");
        }
        private void Go_library_playlists(object sender, RoutedEventArgs e)
        {
            closingPage();
            pageController.go("library", "playlists");
        }
        private void Go_local_tracks(object sender, RoutedEventArgs e)
        {
            closingPage();
            pageController.go("local", "tracks");
            
        }
        private void Go_local_albums(object sender, RoutedEventArgs e)
        {
            closingPage();
            pageController.go("local", "albums");
        }
        private void Go_local_artists(object sender, RoutedEventArgs e)
        {
            closingPage();
            pageController.go("local", "artists");
        }
        private void go_youtube(object sender, RoutedEventArgs e)
        {
            closingPage();
            pageController.go("streaming", "youtube");
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
            if (Player.started)
            {
                Player.Start();
            }
        }

        private void Skip(object sender, RoutedEventArgs e)
        {
            vm.skip = true;
            if (Player.started)
                Player.next();
        }

        private void Shuffle(object sender, RoutedEventArgs e)
        {
            if (vm.PlayQueue != null)
            {
                queue.Shuffle();
            }
        }
        
        private void Open_queuelist(object sender, RoutedEventArgs e)
        {
            queue.toggle();
        }

        private void ChangeVol(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.SetVol();
        }

        private void Prev(object sender, RoutedEventArgs e)
        {
            if (Player.started)
                Player.prev();
        }

        private void PrevList(object sender, RoutedEventArgs e)
        {
            if (MainGrid.Children.Count > 1)
            {
                MainGrid.Children.Remove(MainGrid.Children[MainGrid.Children.Count - 1]);
                vm.Page = vm.Prev_title;
                vm.Listtypes.RemoveAt(vm.Listtypes.Count - 1);
            }
        }

    }
}   