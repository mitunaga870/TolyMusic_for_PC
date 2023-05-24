using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySqlX.XDevAPI.Relational;

namespace TolyMusic_for_PC.Library;

public class Lib_PC
{
    //private変数
    private ViewModel vm;
    private Player player;
    private Queue queue;
    private Grid container;
    private StackPanel func_container;
    private Main main;
    private LibFunc func;
    private bool loaded = false;
    //コンストラクタ
    public Lib_PC(ViewModel vm, Player player, Queue queue, Grid container, StackPanel funcContainer)
    {
        this.vm = vm;
        this.player = player;
        this.queue = queue;
        this.container = container;
        this.func_container = funcContainer;
        //初期化
        main = new Main(vm, player, queue, container, func_container);
    }
    //ページ遷移
    public void Go(string page)
    {
        if (!loaded)
        {
            main.Init();
            loaded = true;
        }
        switch (page)
        {
            case "tracks":
                vm.Tracks = main.GetTracks();
                vm.Curttype = ViewModel.TypeEnum.Track;
                MakeTrackList();
                break;
            case "albums":
                break;
            case "artists":
                break;
        }
    }
    //要素作成関数
    private void MakeTrackList()
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
                    if(vm.Curt_track.Id==vm.PlayQueue[i].Id)
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
        //最終処理
        mainlist.View = row;
        container.Children.Add(mainlist);
    }
}