using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySqlX.XDevAPI.Relational;

namespace TolyMusic_for_PC.Library;

public class Lib_PC : Super.PageController
{
    //private変数
    private Main main;
    private LibFunc func;
    private bool loaded = false;
    private string curt_page;
    //コンストラクタ
    public Lib_PC(ViewModel vm, Player player, Queue queue, Grid container, StackPanel funcContainer) 
        : base (vm,player,queue,container,funcContainer)
    {
        //初期化
        main = new Main(vm, player, queue, container, funcContainer);
        func = new LibFunc(vm, player, queue, container, funcContainer, main, this);
    }
    //ページ遷移
    public override void Go(string page)
    {
        curt_page = page;
        if (!loaded)
        {
            main.Init();
            loaded = true;
        }
        //その他ボタンの作成
        //順再生
        Button playall = new Button();
        playall.Content = "順再生";
        playall.Click += func.PlayAll;
        func_container.Children.Add(playall);
        //シャッフル再生
        Button Shuffleall = new Button();
        Shuffleall.Content = "シャッフル再生";
        Shuffleall.Click += func.ShuffleAll;
        func_container.Children.Add(Shuffleall);
        //検索テキストボックスの作成
        TextBox search = new TextBox();
        search.Width = 200;
        search.TextChanged += func.Search;
        func_container.Children.Add(search);
        switch (page)
        {
            case "tracks":
                vm.Tracks = main.GetTracks("",Main.FilterEnum.All);
                vm.Curttype = ViewModel.TypeEnum.Track;
                func.MakeTrackList();
                break;
            case "albums":
                vm.Albums = main.GetAlbums("",Main.FilterEnum.All);
                vm.Curttype = ViewModel.TypeEnum.Album;
                func.MakeAlbumList();
                break;
            case "artists":
                vm.Artists = main.GetArtists("",Main.FilterEnum.All);
                vm.Curttype = ViewModel.TypeEnum.Artist;
                func.MakeArtistList();
                break;
        }
    }
    //リフレッシュ
    public void Refresh()
    {
        func_container.Children.Clear();
        Go(curt_page);
    }
}