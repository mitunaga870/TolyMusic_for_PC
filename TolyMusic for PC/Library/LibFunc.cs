using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TolyMusic_for_PC.Property;

namespace TolyMusic_for_PC.Library;

public class LibFunc
{
    ViewModel vm;
    Player player;
    Queue queue;
    Grid container;
    StackPanel func_container;
    Main main;
    Lib_PC libPc;
    //コンストラクタ
    public LibFunc(ViewModel vm, Player player, Queue queue, Grid container, StackPanel funcContainer, Main main, Lib_PC libPc)
    {
        this.vm = vm;
        this.player = player;
        this.queue = queue;
        this.container = container;
        this.func_container = funcContainer;
        this.main = main;
        this.libPc = libPc;
    }
private void MakeQueue()
        {
            switch (vm.Curttype)
            {
                case ViewModel.TypeEnum.Track:
                    vm.PlayQueue = vm.Tracks;
                    break;
                case ViewModel.TypeEnum.Album:
                    vm.PlayQueue = main.GetTracks(vm.Curt_Album.Id, Main.FilterEnum.Album);
                    break;
                case ViewModel.TypeEnum.Artist:
                    vm.PlayQueue = main.GetTracks(vm.Curt_Artist.Id,Main.FilterEnum.Artist);
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
    //リスト生成関数
    public void MakeTrackList()
    {
        //メインリスト作成
        ListView mainlist = new ListView();
        mainlist.ItemsSource = vm.Tracks;
        mainlist.SelectionMode = SelectionMode.Single;
        mainlist.HorizontalAlignment = HorizontalAlignment.Stretch;
        //メインリスト用スタイル
        Style mainlist_style = new Style(typeof(ListViewItem));
        //トラック再生イベント
        EventSetter play_event = new EventSetter();
        play_event.Event = ListViewItem.MouseDoubleClickEvent;
        play_event.Handler = new MouseButtonEventHandler((sender, args) =>
        {
            //キューの割当
            vm.Curt_track = (Track)mainlist.SelectedItem;
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
        mainlist_style.Setters.Add(play_event);
        mainlist.ItemContainerStyle = mainlist_style;
        //行テンプレート
        GridView row = new GridView();
        //タイトル
        GridViewColumn title = new GridViewColumn();
        title.Header = "タイトル";
        title.DisplayMemberBinding = new System.Windows.Data.Binding("Title");
        row.Columns.Add(title);
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
        property.Click += (sender, args) =>
        {
            TrackPreoperty propertyWindow = new TrackPreoperty(vm,ViewModel.TypeEnum.Track);
            propertyWindow.Owner = Application.Current.MainWindow;
            propertyWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            propertyWindow.ShowDialog();
            libPc.Refresh();
        };
        menu.Items.Add(property);
        other_button.SetValue(Button.ContextMenuProperty,menu);
        other_button.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, args) =>
        {
            vm.Preoperty_Id = ((Button)sender).Uid;
            ((Button)sender).ContextMenu.IsOpen = true;
        }));
        other_template.VisualTree = other_button;
        other.CellTemplate = other_template;
        row.Columns.Add(other);
        //最終処理
        mainlist.View = row;
        container.Children.Add(mainlist);
    }
    public void MakeAlbumList()
    {
        //メインリスト作成
        ListView mainlist = new ListView();
        mainlist.ItemsSource = vm.Albums;
        mainlist.SelectionMode = SelectionMode.Single;
        mainlist.HorizontalAlignment = HorizontalAlignment.Stretch;
        //メインリスト用スタイル
        Style mainlist_style = new Style(typeof(ListViewItem));
        //トラック再生イベント
        EventSetter play_event = new EventSetter();
        play_event.Event = ListViewItem.MouseDoubleClickEvent;
        play_event.Handler = new MouseButtonEventHandler((sender, args) =>
        {
            Album album = (Album)mainlist.SelectedItem;
            vm.Curt_Album = album;
            //タイトル変更
            vm.Prev_title = vm.Page;
            vm.Page = album.Title;
            //トラック取得
            vm.Tracks = main.GetTracks(album.Id, Main.FilterEnum.Album);
            vm.Listtypes.Add(ViewModel.TypeEnum.Track);
            MakeTrackList();
        });
        mainlist_style.Setters.Add(play_event);
        mainlist.ItemContainerStyle = mainlist_style;
        //行テンプレート
        GridView row = new GridView();
        //タイトル
        GridViewColumn title = new GridViewColumn();
        title.Header = "タイトル";
        title.DisplayMemberBinding = new System.Windows.Data.Binding("Title");
        row.Columns.Add(title);
        //その他ボタン
        GridViewColumn other = new GridViewColumn();
        other.Header = "other";
        var other_template = new DataTemplate();
        var other_button = new FrameworkElementFactory(typeof(Button));
        other_button.SetValue(Button.ContentProperty, "...");
        other_button.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, args) =>
        {
            MessageBox.Show("test");
        }));
        other_template.VisualTree = other_button;
        other.CellTemplate = other_template;
        row.Columns.Add(other);
        //最終処理
        mainlist.View = row;
        container.Children.Add(mainlist);
    }

    public void MakeArtistList()
    {
        //メインリスト作成
        ListView mainlist = new ListView();
        mainlist.ItemsSource = vm.Artists;
        mainlist.SelectionMode = SelectionMode.Single;
        mainlist.HorizontalAlignment = HorizontalAlignment.Stretch;
        //メインリスト用スタイル
        Style mainlist_style = new Style(typeof(ListViewItem));
        //トラック再生イベント
        EventSetter play_event = new EventSetter();
        play_event.Event = ListViewItem.MouseDoubleClickEvent;
        play_event.Handler = new MouseButtonEventHandler((sender, args) =>
        {
            Artist artist = (Artist)mainlist.SelectedItem;
            vm.Curt_Artist = artist;
            //タイトル変更
            vm.Prev_title = vm.Page;
            vm.Page = artist.Name;
            //アルバム取得
            vm.Albums = main.GetAlbums(artist.Id, Main.FilterEnum.Artist);
            vm.Listtypes.Add(ViewModel.TypeEnum.Album);
            MakeAlbumList();
        });
        mainlist_style.Setters.Add(play_event);
        mainlist.ItemContainerStyle = mainlist_style;
        //行テンプレート
        GridView row = new GridView();
        //タイトル
        GridViewColumn Name = new GridViewColumn();
        Name.Header = "名前";
        Name.DisplayMemberBinding = new System.Windows.Data.Binding("Name");
        row.Columns.Add(Name);
        //その他ボタン
        GridViewColumn other = new GridViewColumn();
        other.Header = "other";
        var other_template = new DataTemplate();
        var other_button = new FrameworkElementFactory(typeof(Button));
        other_button.SetValue(Button.ContentProperty, "...");
        other_button.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, args) =>
        {
            MessageBox.Show("test");
        }));
        other_template.VisualTree = other_button;
        other.CellTemplate = other_template;
        row.Columns.Add(other);
        //最終処理
        mainlist.View = row;
        container.Children.Add(mainlist);
    }
}