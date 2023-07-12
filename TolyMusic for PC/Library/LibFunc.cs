using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CefSharp.DevTools.Debugger;
using TolyMusic_for_PC.Property;
using TolyMusic_for_PC.Super;

namespace TolyMusic_for_PC.Library;

public class LibFunc : PageFunc
{
    private Lib_PC libPc;
    private Main main;

    public LibFunc(ViewModel vm, Player player, Queue.Main queue, Grid container, StackPanel funcContainer, Main main,
        Lib_PC PegeController) : base(vm, player, queue, container, funcContainer, main, PegeController)
    {
        this.main = (Main)main;
        libPc = (Lib_PC)PegeController;
    }

    protected override void MakeQueue()
    {
        if (vm.Filter == null)
        {//フィルターが未設定の時
            vm.PlayQueue = main.GetTracks(null, ViewModel.TypeEnum.All);
            return;
        }
        vm.PlayQueue = main.GetTracks(vm.Filter, vm.Filtertype);
    }

    //リスト生成関数
    public void MakeTrackList()
    {
        vm.Listtypes.Add(ViewModel.TypeEnum.Track);
        vm.Curttype = ViewModel.TypeEnum.Track;
        GridView row = new GridView();
        var head_path = new Dictionary<string, string>();
        head_path.Add("タイトル", "Title");
        head_path.Add("アーティスト", "Artist");
        head_path.Add("アルバム", "Album");
        //ダブルクリックイベント
        var Event = new MouseButtonEventHandler(listEvent.PlayTrack);
        var mainlist = MakeList(head_path, ViewModel.TypeEnum.Track, Event, ref row);
        var other = MakeOtheBT(ViewModel.TypeEnum.Track);
        row.Columns.Add(other);
        //最終処理
        mainlist.View = row;
        container.Children.Add(mainlist);
    }

    public void MakeAlbumList()
    {
        vm.Listtypes.Add(ViewModel.TypeEnum.Album);
        GridView row = new GridView();
        var head_path = new Dictionary<string, string>();
        head_path.Add("タイトル", "Title");
        head_path.Add("アーティスト", "Group");
        //ダブルクリックイベント
        var Event = new MouseButtonEventHandler((sender, args) =>
        {
            Album album = (Album)((ListViewItem)sender).Content;
            vm.Curt_Album = album;
            vm.Filter = album.Id;
            //タイトル変更
            vm.Prev_title = vm.Page;
            vm.Page = album.Title;
            //トラック取得
            vm.Tracks = main.GetTracks(album.Id, ViewModel.TypeEnum.Album);
            MakeTrackList();
        });
        var mainlist = MakeList(head_path, ViewModel.TypeEnum.Album,Event, ref row);
        //その他ボタン
        var other = MakeOtheBT(ViewModel.TypeEnum.Album);
        row.Columns.Add(other);
        //最終処理
        mainlist.View = row;
        container.Children.Add(mainlist);
    }

    public void MakeArtistList()
    {
        vm.Listtypes.Add(ViewModel.TypeEnum.Artist);
        //リスト作成
        var row = new GridView();
        var head_path = new Dictionary<string, string>();
        var Event = new MouseButtonEventHandler((sender, args) =>
        {
            Artist artist = (Artist)((ListViewItem)sender).Content;
            vm.Curt_Artist = artist;
            vm.Filter = artist.Id;
            //タイトル変更
            vm.Prev_title = vm.Page;
            vm.Page = artist.Name;
            //アルバム取得
            vm.Albums = main.GetAlbums(artist.Id, ViewModel.TypeEnum.Artist);
            MakeAlbumList();
        });
        head_path.Add("名前", "Name");
        var mainlist = MakeList(head_path, ViewModel.TypeEnum.Artist, Event, ref row);
        //その他ボタン
        var other = MakeOtheBT(ViewModel.TypeEnum.Artist);
        row.Columns.Add(other);
        //最終処理
        container.Children.Add(mainlist);
    }

    public void MakePlaylistList()
    {
        vm.Listtypes.Add(ViewModel.TypeEnum.Playlist);
        //リスト作成
        var row = new GridView();
        var head_path = new Dictionary<string, string>();
        var Event = new MouseButtonEventHandler((sender, args) =>
        {
            Playlist playlist = (Playlist)((ListViewItem)sender).Content;
            vm.Curt_Playlist = playlist;
            vm.Filter = playlist.Id;
            //タイトル変更
            vm.Prev_title = vm.Page;
            vm.Page = playlist.Title;
            //トラック取得
            vm.Tracks = main.GetTracks(playlist.Id, ViewModel.TypeEnum.Playlist);
            MakeTrackList();
        });
        head_path.Add("タイトル", "Title");
        var mainlist = MakeList(head_path, ViewModel.TypeEnum.Playlist, Event, ref row);
        //その他ボタン
        var other = MakeOtheBT(ViewModel.TypeEnum.Playlist);
        row.Columns.Add(other);
        //最終処理
        container.Children.Add(mainlist);
    }
    
    private GridViewColumn MakeOtheBT(ViewModel.TypeEnum type)
    {
        //その他ボタン
        GridViewColumn other = new GridViewColumn();
        other.Header = "other";
        var other_template = new DataTemplate();
        var other_button = new FrameworkElementFactory(typeof(Button));
        other_button.SetValue(Button.ContentProperty, "...");
        other_button.SetBinding(Button.UidProperty, new System.Windows.Data.Binding("Id"));
        //ポップアップメニュー
        ContextMenu menu = new ContextMenu();
        //プロパティ
        MenuItem property = new MenuItem();
        property.Header = "プロパティ";
        switch (type)
        {
            case ViewModel.TypeEnum.Track:
                property.Click += (sender, args) =>
                {
                    TrackPreoperty propertyWindow = new TrackPreoperty(vm);
                    propertyWindow.Owner = Application.Current.MainWindow;
                    propertyWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    propertyWindow.ShowDialog();
                    libPc.Refresh();
                };
                break;
            case ViewModel.TypeEnum.Album:
                property.Click += (sender, args) =>
                {
                    AlbumPreoperty window = new AlbumPreoperty(vm);
                    window.Owner = Application.Current.MainWindow;
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.ShowDialog();
                    libPc.Refresh();
                };
                break;
            case ViewModel.TypeEnum.Artist:
                property.Click += (sender, args) =>
                {
                    ArtistProperty window = new ArtistProperty(vm);
                    window.Owner = Application.Current.MainWindow;
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.ShowDialog();
                    libPc.Refresh();
                };
                break;
            case ViewModel.TypeEnum.Playlist:
                StackPanel container = new StackPanel();
                container.Orientation = Orientation.Horizontal;
                TextBox title = new TextBox();
                title.Width = 200;
                container.Children.Add(title);
                Button update = new Button();
                update.Content = "更新";
                update.Click += (sender, args) =>
                {
                    if (MessageBox.Show("このプレイリストのタイトルを変更します。\nよろしいですか？", "確認", MessageBoxButton.OKCancel) ==
                        MessageBoxResult.Cancel)
                        return;
                    main.Update_playlist(vm.Curt_Playlist.Id, title.Text);
                    libPc.Refresh();
                };
                container.Children.Add(update);
                property.Items.Add(container);
                break;
        }
        menu.Items.Add(property);
        //トラック削除
        switch (type)
        {
            case ViewModel.TypeEnum.Track:
                var del = new MenuItem();
                del.Header = "ライブラリから削除";
                del.Click += (sender, args) =>
                {
                    if (MessageBox.Show("このトラックをライブラリから削除します。\nよろしいですか？", "確認", MessageBoxButton.OKCancel) ==
                        MessageBoxResult.Cancel)
                        return;
                    main.Del_track(vm.Othermenu_Id);
                    libPc.Refresh();
                };
                menu.Items.Add(del);
                break;
            case ViewModel.TypeEnum.Playlist:
                var del_playlist = new MenuItem();
                del_playlist.Header = "プレイリストから削除";
                del_playlist.Click += (sender, args) =>
                {
                    if (MessageBox.Show("このプレイリストを削除します。\nよろしいですか？", "確認", MessageBoxButton.OKCancel) ==
                        MessageBoxResult.Cancel)
                        return;
                    main.Del_playlist(vm.Othermenu_Id);
                    libPc.Refresh();
                };
                menu.Items.Add(del_playlist);
                break;
        }
        //プレイリストに追加
        var add_playlist = new MenuItem();
        add_playlist.Header = "プレイリストに追加";
        menu.Items.Add(add_playlist);
        //ボタンにメニューを割り当て
        other_button.SetValue(Button.ContextMenuProperty, menu);
        other_button.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, args) =>
        {
            //ボタンのUidにIdを入れておく
            vm.Othermenu_Id = ((Button)sender).Uid;
            AddPlaylistMenu(ref add_playlist, type);
            //コンテキストメニューを開く
            ((Button)sender).ContextMenu.IsOpen = true;
        }));
        other_template.VisualTree = other_button;
        other.CellTemplate = other_template;
        return other;
    }

    private void AddPlaylistMenu(ref MenuItem add_playlist , ViewModel.TypeEnum type)
    {
        //プレイリストメニューリセット
        add_playlist.Items.Clear();
        //新規プレイリスト
        var new_playlist = new StackPanel();
        new_playlist.Orientation = Orientation.Horizontal;
        var new_plalist_label = new Label();
        new_plalist_label.Content = "新規プレイリスト";
        new_playlist.Children.Add(new_plalist_label);
        var new_playlist_title = new TextBox();
        new_playlist_title.Width = 100;
        new_playlist.Children.Add(new_playlist_title);
        var new_playlist_add = new Button();
        new_playlist_add.Content = "追加";
        new_playlist_add.Click += (sender, args) =>
        {
            main.Make_playlist(new_playlist_title.Text, vm.Othermenu_Id ,type);
            libPc.Refresh();
        };
        new_playlist.Children.Add(new_playlist_add);
        add_playlist.Items.Add(new_playlist);
        //プレイリスト取得
        var playlists = main.GetPlaylists();
        foreach (var playlist in playlists)
        {
            var item = new MenuItem();
            item.IsCheckable = true;
            item.Header = playlist.Title;
            //チェック状態
            if (playlist.Tracks.Count(t => t.Id == vm.Othermenu_Id) > 0)
                item.IsChecked = true;
            else
                item.IsChecked = false;
            //追加イベント
            item.Checked += (sender2, args2) => { main.Add_playlist(playlist.Id, vm.Othermenu_Id, type); };
            //削除イベント
            item.Unchecked += (sender2, args2) => { main.Del_playlist(playlist.Id, vm.Othermenu_Id ,type); };
            add_playlist.Items.Add(item);
        }
}

}