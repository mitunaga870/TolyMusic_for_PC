using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TolyMusic_for_PC.Local
{
    public class LocalFunc
    {
        private ViewModel vm;
        private Player player;
        private Queue queue;
        private Local.Main local;
        private Library.AddLibFunc lib;
        Grid container;
        //コンストラクタ
        public LocalFunc(ViewModel vm, Player player, Queue queue, Main local, Grid container)
        {
            this.vm = vm;
            this.player = player;
            this.queue = queue;
            this.local = local;
            this.container = container;
            lib = new Library.AddLibFunc(vm);
        }
        //共通メソッド
        private void MakeQueue()
        {
            switch (vm.Listtype)
            {
                case ViewModel.TypeEnum.Track:
                    vm.PlayQueue = local.GetTracks();
                    break;
                case ViewModel.TypeEnum.Album:
                    vm.PlayQueue = local.GetTracks(vm.Curt_Album.Id,Main.id_type.album);
                    break;
                case ViewModel.TypeEnum.Artist:
                    vm.PlayQueue = local.GetTracks(vm.Curt_Artist.Id,Main.id_type.artist);
                    break;
            }
        }
        //純再生
        public void PlayAll(object sender = null, RoutedEventArgs e = null)
        {
            //キュー作成作業
            MakeQueue();
            vm.Curt_queue_num = 0;
            vm.Curt_track = vm.PlayQueue[vm.Curt_queue_num];
            //キュー生成
            queue.set();
            queue.showbutton();
            //再生
            player.Start();
        }
        //シャッフル再生
        public void ShuffleAll(object sender = null, RoutedEventArgs e = null)
        {
            //キュー作成作業
            MakeQueue();
            vm.Curt_queue_num = 0;
            for(int i = vm.PlayQueue.Count - 1; i > 0; i--)
            {
                int r = new System.Random().Next(i + 1);
                Track tmp = vm.PlayQueue[i];
                vm.PlayQueue[i] = vm.PlayQueue[r];
                vm.PlayQueue[r] = tmp;
            }
            //キュー生成
            queue.set();
            queue.showbutton();
            //再生
            player.Start();
        }
        //ライブラリに追加
        public void AddLibAll(object sender = null, RoutedEventArgs e = null)
        {
            //tracks
            Collection<Track> tracks = new Collection<Track>();
            switch (vm.Listtype)
            {
                case ViewModel.TypeEnum.Track:
                    tracks = vm.Tracks;
                    break;
                case ViewModel.TypeEnum.Album:
                    tracks = local.GetTracks(vm.Curt_Album.Id, Main.id_type.album);
                    break;
                case ViewModel.TypeEnum.Artist:
                    tracks = local.GetTracks(vm.Curt_Artist.Id, Main.id_type.artist);
                    break;
                default:
                    return;
            }
            //パラメータ作成
            if (tracks == null||tracks.Count == 0)
            {
                MessageBox.Show("リスト取得失敗");
                return;
            }
            lib.AddLocalListTracks(tracks);
        }
        public void MakeTrackList()
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
                    if(vm.Curt_track.Id==vm.PlayQueue[i].Id)
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
        public void MakeAlbumList()
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
                vm.Tracks = local.GetTracks(album.Id, Main.id_type.album);
                vm.Listtypes.Add(ViewModel.TypeEnum.Album);
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
        public void MakeArtistList()
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
                vm.Listtypes.Add(ViewModel.TypeEnum.Artist);
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