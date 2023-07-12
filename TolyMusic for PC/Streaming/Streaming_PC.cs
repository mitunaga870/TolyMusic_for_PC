using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Internals;
using CefSharp.Wpf;
using TolyMusic_for_PC.Streaming.Handlar;

namespace TolyMusic_for_PC.Streaming;

public class Streaming_PC
{
    private ViewModel vm;
    private Player player;
    private Queue.Main queue;
    private Grid container;
    private StackPanel func_container;
    private Yt_Func yt_func;
    public Streaming_PC(ViewModel vm, Player player, Queue.Main queue, Grid container, StackPanel funcContainer)
    {
        this.vm = vm;
        this.player = player;
        this.queue = queue;
        this.container = container;
        this.func_container = funcContainer;
        yt_func = new Yt_Func(vm, player, queue, container);
    }
    //ストリーミングページに移動
    public void Go(string type)
    {
        if (vm.isOnline)
        {
            ChromiumWebBrowser web = new ChromiumWebBrowser();
            web.BrowserSettings.LocalStorage = CefState.Enabled;
            switch (type)
            {
                case "youtube":
                    //ページ遷移
                    web.Address = "https://music.youtube.com/";
                    container.Children.Add(web);
                    //再生id取得
                    web.RequestHandler = new YoutubeReqHandler(vm,yt_func);
                    //再生アイテム追加
                    Button add_bt = new Button();
                    add_bt.Content = "再生中コンテンツを追加";
                    add_bt.Click += yt_func.Add_PlayingTrack;
                    func_container.Children.Add(add_bt);
                    //ライブラリ同期
                    Button add_lib_bt = new Button();
                    add_lib_bt.Content = "ライブラリを同期";
                    add_lib_bt.Click += yt_func.SyncLib;
                    func_container.Children.Add(add_lib_bt);
                    break;
                case "tois":
                    break;
            }
        }
        else
        {
            MessageBox.Show("オフラインです。");
        }
    }
}