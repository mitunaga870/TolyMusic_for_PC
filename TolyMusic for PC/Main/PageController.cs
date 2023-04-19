using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TolyMusic_for_PC.Local;

namespace TolyMusic_for_PC
{
    public class PageController
    {
        private Local.Main local;
        private bool loadedlocal = false;
        private string type;
        private string page;
        private ViewModel vm;
        private Grid container;
        private Player player;
        private Queue queue;
        private StackPanel func_container;
        public PageController(ViewModel vm, Grid container, StackPanel funcContainer, Player player, Queue queue)
        {
            this.vm = vm;
            this.container = container;
            this.player = player;
            this.queue = queue;
            this.func_container = funcContainer;
            go("library", "tracks");
        }
        public void go(string type, string page)
        {
            this.type = type;
            this.page = page;
            //ページタイトル変更
            switch (this.type)
            {
                case "local":
                    vm.Type = "ローカル";
                    break;
                case "library":
                    vm.Type = "ライブラリ";
                    break;
                case "youtube":
                    vm.Type = "YouTube";
                    break;
                default:
                    vm.Type = "不明";
                    break;
            }
            switch (this.page)
            {
                case "tracks":
                    vm.Page = "曲";
                    break;
                case "albums":
                    vm.Page = "アルバム";
                    break;
                case "artists":
                    vm.Page = "アーティスト";
                    break;
                case "playlists":
                    vm.Page = "プレイリスト";
                    break;
            }
            //データを取得
            getdata(vm);
            //入力養素の作成
        }
        private void getdata(ViewModel vm)
        {
            switch (type)
            {
                case "local":
                    //初回DB更新
                    if(!loadedlocal)
                    {
                        local = new Local.Main();
                        loadedlocal = true;
                    }
                    //データ取得
                    switch (page)
                    {
                        case "tracks":
                            vm.Tracks = local.GetTracks();
                            vm.Curttype = ViewModel.TypeEnum.Track;
                            vm.Listtypes.Add(ViewModel.TypeEnum.Track);
                            MakeTrackList();
                            break;
                        case "albums":
                            vm.Albums = local.GetAlbums();
                            vm.Curttype = ViewModel.TypeEnum.Album;
                            vm.Listtypes.Add(ViewModel.TypeEnum.Album);
                            MakeAlbumList();
                            break;
                        case "artists":
                            vm.Artists = local.GetArtists();
                            vm.Curttype = ViewModel.TypeEnum.Artist;
                            vm.Listtypes.Add(ViewModel.TypeEnum.Artist);
                            MakeArtistList();
                            break;
                    }
                    //その他ボタンの作成
                    LocalFunc localFunc = new LocalFunc(vm,player,queue,local);
                    //順再生
                    Button playall = new Button();
                    playall.Content = "順再生";
                    playall.Click += localFunc.PlayAll;
                    func_container.Children.Add(playall);
                    //シャッフル再生
                    Button Shuffleall = new Button();
                    Shuffleall.Content = "シャッフル再生";
                    Shuffleall.Click += localFunc.ShuffleAll;
                    func_container.Children.Add(Shuffleall);
                    //ライブラリ追加
                    Button AddLib = new Button();
                    AddLib.Content = "ライブラリに追加";
                    AddLib.Click += localFunc.AddLibAll;
                    //プレイリスト追加
                    //その他
                    break;
            }
        }
        private void MakeTrackList()
        {
            ListView ContentList = new ListView();
            ContentList.SelectionMode = SelectionMode.Single;
            ContentList.HorizontalAlignment = HorizontalAlignment.Stretch;
            ContentList.ItemsSource = vm.Tracks;
            Style style = new Style();
            style.TargetType = typeof(ListViewItem);
            EventSetter setter = new EventSetter();
            setter.Event = ListViewItem.MouseDoubleClickEvent;
            setter.Handler = new MouseButtonEventHandler((sender, args) =>
            {
                //キューの割当
                vm.Curt_track = (Track)ContentList.SelectedItem;
                vm.PlayQueue = new ObservableCollection<Track>(vm.Tracks);
                queue.set();
                queue.showbutton();
                for (int i = 0; i < vm.Tracks.Count; i++)
                {
                    if(vm.Curt_track.id==vm.PlayQueue[i].id)
                        vm.Curt_queue_num = i;
                }
                //再生
                player.Start();
            });
            style.Setters.Add(setter);
            ContentList.ItemContainerStyle = style;
            //行の添付作成
            GridView row = new GridView();
            //タイトル
            GridViewColumn title = new GridViewColumn();
            title.Header = "タイトル";
            title.DisplayMemberBinding = new System.Windows.Data.Binding("Title");
            row.Columns.Add(title);
            //固定プレイリストに追加用ボタン
            GridViewColumn AddLib = new GridViewColumn();
            DataTemplate AddLibTemplate = new DataTemplate();
            FrameworkElementFactory AddLibButton = new FrameworkElementFactory(typeof(Button));
            AddLibButton.SetValue(Button.ContentProperty, "＋");
            AddLibButton.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, args) =>
            {
                MessageBox.Show("追加しました");
            }));
            AddLibTemplate.VisualTree = AddLibButton;
            AddLib.CellTemplate = AddLibTemplate;
            row.Columns.Add(AddLib);
            //最終処理
            ContentList.View = row;
            container.Children.Add(ContentList);
        }
        private void MakeAlbumList()
        {
            ListView AlbumList = new ListView();
            AlbumList.SelectionMode = SelectionMode.Single;
            AlbumList.HorizontalAlignment = HorizontalAlignment.Stretch;
            AlbumList.ItemsSource = vm.Albums;
            Style Albumstyle = new Style();
            Albumstyle.TargetType = typeof(ListViewItem);
            EventSetter Albumsetter = new EventSetter();
            Albumsetter.Event = ListViewItem.MouseDoubleClickEvent;
            Albumsetter.Handler = new MouseButtonEventHandler((sender, args) =>
            {
                Album album = (Album)AlbumList.SelectedItem;
                vm.Curt_Album = album;
                //ページタイトル変更
                vm.Prev_title = vm.Page;
                vm.Page = album.Title;
                //アルバムページに移動(トラックページとほぼ同様)
                local.GetTracks(album.Id, Main.id_type.album);
                vm.Tracks = local.GetTracks(album.Id, Main.id_type.album);
                vm.Listtypes.Add(ViewModel.TypeEnum.Track);
                MakeTrackList();
            });
            Albumstyle.Setters.Add(Albumsetter);
            AlbumList.ItemContainerStyle = Albumstyle;
            GridView Albumrow = new GridView();
            GridViewColumn Albumtitle = new GridViewColumn();
            Albumtitle.Header = "タイトル";
            Albumtitle.DisplayMemberBinding = new System.Windows.Data.Binding("Title");
            Albumrow.Columns.Add(Albumtitle);
            AlbumList.View = Albumrow;
            container.Children.Add(AlbumList);
        }
        private void MakeArtistList()
        {
            ListView ArtistList = new ListView();
            ArtistList.SelectionMode = SelectionMode.Single;
            ArtistList.HorizontalAlignment = HorizontalAlignment.Stretch;
            ArtistList.ItemsSource = vm.Artists;
            Style Artiststyle = new Style();
            Artiststyle.TargetType = typeof(ListViewItem);
            EventSetter Artistsetter = new EventSetter();
            Artistsetter.Event = ListViewItem.MouseDoubleClickEvent;
            Artistsetter.Handler = new MouseButtonEventHandler(((sender, args) =>
            {
                Artist artist = (Artist)ArtistList.SelectedItem;
                vm.Curt_Artist = artist;
                //ページタイトル変更
                vm.Prev_title = vm.Page;
                vm.Page = artist.Name;
                //アーティストページに移動(トラックページとほぼ同様)
                vm.Albums = local.GetAlbums(artist.Id, Main.id_type.artist);
                vm.Listtypes.Add(ViewModel.TypeEnum.Album);
                MakeAlbumList();
            }));
            Artiststyle.Setters.Add(Artistsetter);
            ArtistList.ItemContainerStyle = Artiststyle;
            GridView Artistrow = new GridView();
            GridViewColumn Artisttitle = new GridViewColumn();
            Artisttitle.Header = "名前";
            Artisttitle.DisplayMemberBinding = new System.Windows.Data.Binding("Name");
            Artistrow.Columns.Add(Artisttitle);
            ArtistList.View = Artistrow;
            container.Children.Add(ArtistList);
        }
    }
}