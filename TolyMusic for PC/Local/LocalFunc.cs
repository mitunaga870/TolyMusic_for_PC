using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TolyMusic_for_PC.Library;
using TolyMusic_for_PC.Super;

namespace TolyMusic_for_PC.Local
{
    public class LocalFunc : PageFunc
    {
        private Local.Main local;
        private Library.AddLibFunc lib;
        //コンストラクタ
        public LocalFunc(ViewModel vm, Player player, Queue queue, Grid container, StackPanel funcContainer, object main, object PageControler) : base(vm, player, queue, container, funcContainer, main, PageControler)
        {
            local = (Main)main;
            lib = new AddLibFunc(vm);
        }
        //共通メソッド
        protected override void MakeQueue()
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
            var head_path = new Dictionary<string, string>();
            head_path.Add("タイトル", "Title");
            var Event = new MouseButtonEventHandler((sender, args) =>
            {
                //キューの割当
                vm.Curt_track = (Track)((ListViewItem)sender).Content;
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
            //行の添付作成
            GridView row = new GridView();
            var ContentList = MakeList(head_path, ViewModel.TypeEnum.Track, Event, ref row);
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
            container.Children.Add(ContentList);
        }
        public void MakeAlbumList()
        {
            var head_path = new Dictionary<string, string>();
            head_path.Add("タイトル","Title");
            var Event = new MouseButtonEventHandler((sender, args) =>
            {
                Album album = (Album)((ListViewItem)sender).Content;
                vm.Curt_Album = album;
                //ページタイトル変更
                vm.Prev_title = vm.Page;
                vm.Page = album.Title;
                //アルバムページに移動(トラックページとほぼ同様)
                vm.Tracks = local.GetTracks(album.Id, Main.id_type.album);
                vm.Listtypes.Add(ViewModel.TypeEnum.Album);
                MakeTrackList();
            });
            GridView Albumrow = new GridView();
            var AlbumList = MakeList(head_path, ViewModel.TypeEnum.Album,Event,ref Albumrow);
            container.Children.Add(AlbumList);
        }
        public void MakeArtistList()
        {
            GridView Artistrow = new GridView();
            ListView ArtistList = new ListView();
            var head_path = new Dictionary<string, string>();
            head_path.Add("名前","Name");
            var Event = new MouseButtonEventHandler(((sender, args) =>
            {
                Artist artist = (Artist)((ListViewItem)sender).Content;
                vm.Curt_Artist = artist;
                //ページタイトル変更
                vm.Prev_title = vm.Page;
                vm.Page = artist.Name;
                //アーティストページに移動(トラックページとほぼ同様)
                vm.Albums = local.GetAlbums(artist.Id, Main.id_type.artist);
                vm.Listtypes.Add(ViewModel.TypeEnum.Artist);
                MakeAlbumList();
            }));
            ArtistList = MakeList(head_path, ViewModel.TypeEnum.Artist, Event, ref Artistrow);
            container.Children.Add(ArtistList);
        }

    }
}