﻿using System.Windows.Controls;

namespace TolyMusic_for_PC.Local;

public class Local_PC : Super.PageController
{
    private bool loaded = false;
    private Main main;
    private LocalFunc localFunc;
    //コンストラクタ
    public Local_PC(ViewModel vm, Player player, Queue.Main queue, Grid container, StackPanel funcContainer) 
        : base(vm,player,queue,container,funcContainer)
    {
        main = new Main(vm);
        localFunc = new LocalFunc(vm, player, queue, container,func_container ,main,this);
    }
    //ページ遷移
    public override void Go(string page)
    {
        if (!loaded)
        {
            main.Init();
            loaded = true;
        }
        vm.Listtypes.Add(ViewModel.TypeEnum.All);
        //データ取得
        switch (page)
        {
            case "tracks":
                vm.Tracks = main.GetTracks();
                vm.Curttype = ViewModel.TypeEnum.Track;
                localFunc.MakeTrackList();
                break;
            case "albums":
                vm.Albums = main.GetAlbums();
                vm.Curttype = ViewModel.TypeEnum.Album;
                localFunc.MakeAlbumList();
                break;
            case "artists":
                vm.Artists = main.GetArtists();
                vm.Curttype = ViewModel.TypeEnum.Artist;
                localFunc.MakeArtistList();
                break;
        }
        //その他ボタンの作成
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
        func_container.Children.Add(AddLib);
        //検索テキストボックスの作成
        TextBox search = new TextBox();
        search.Width = 200;
        search.TextChanged += localFunc.Search;
        func_container.Children.Add(search);
        //プレイリスト追加
        //その他
    }
}