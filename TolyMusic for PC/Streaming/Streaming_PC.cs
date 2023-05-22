using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;

namespace TolyMusic_for_PC.Streaming;

public class Streaming_PC
{
    private ViewModel vm;
    private Player player;
    private Queue queue;
    private Grid container;
    private StackPanel func_container;
    private Yt_Func yt_func;
    public Streaming_PC(ViewModel vm, Player player, Queue queue, Grid container, StackPanel funcContainer)
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
        ChromiumWebBrowser web = new ChromiumWebBrowser();
        web.BrowserSettings.LocalStorage = CefState.Enabled;
        switch (type)
        {
            case "youtube":
                //ページ遷移
                web.Address = "https://music.youtube.com/";
                container.Children.Add(web);
                //追加イベント
                //一括追加イベント実装
                Button add_bt = new Button();
                add_bt.Content = "一括追加";
                func_container.Children.Add(add_bt);
                Button add_lib_bt = new Button();
                add_lib_bt.Content = "ライブラリを同期";
                add_lib_bt.Click += yt_func.SyncLib;
                func_container.Children.Add(add_lib_bt);
                break;
        }
    }
}