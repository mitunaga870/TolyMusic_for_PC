using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public LibFunc(ViewModel vm, Player player, Queue queue, Grid container, StackPanel funcContainer, Main main,
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
        var Event = new MouseButtonEventHandler((sender, args) =>
        {
            ListViewItem item = (ListViewItem)sender;
            //キューの割当
            vm.Curt_track = (Track)item.Content;
            vm.PlayQueue = new ObservableCollection<Track>(vm.Tracks);
            queue.set();
            queue.showbutton();
            for (int i = 0; i < vm.Tracks.Count; i++)
            {
                if (vm.Curt_track.Id == vm.PlayQueue[i].Id)
                    vm.Curt_queue_num = i;
            }
            //再生
            player.Start();
        });
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
        }
        menu.Items.Add(property);
        //トラック削除
        if (type == ViewModel.TypeEnum.Track)
        {
            var del = new MenuItem();
            del.Header = "ライブラリから削除";
            del.Click += (sender, args) =>
            {
                if (MessageBox.Show("このトラックをライブラリから削除します。\nよろしいですか？", "確認", MessageBoxButton.OKCancel) ==
                    MessageBoxResult.Cancel)
                    return;
                main.Del_track(vm.Preoperty_Id);
                libPc.Refresh();
            };
            menu.Items.Add(del);
        }
        other_button.SetValue(Button.ContextMenuProperty, menu);
        other_button.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, args) =>
        {
            vm.Preoperty_Id = ((Button)sender).Uid;
            ((Button)sender).ContextMenu.IsOpen = true;
        }));
        other_template.VisualTree = other_button;
        other.CellTemplate = other_template;
        return other;
    }
}