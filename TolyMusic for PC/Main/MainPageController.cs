using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TolyMusic_for_PC.Library;
using TolyMusic_for_PC.Local;
using TolyMusic_for_PC.Streaming;

namespace TolyMusic_for_PC
{
    public class MainPageController
    {
        private bool loadedlocal = false;
        private string type;
        private string page;
        private ViewModel vm;
        private Grid container;
        private Player player;
        private Queue.Main queue;
        private StackPanel func_container;
        private Streaming_PC streaming;
        private Lib_PC library;
        private Local_PC local;
        LocalFunc localFunc;
        public MainPageController(ViewModel vm, Grid container, StackPanel funcContainer, Player player, Queue.Main queue)
        {
            this.vm = vm;
            this.container = container;
            this.player = player;
            this.queue = queue;
            this.func_container = funcContainer;
            local = new Local_PC(vm,player,queue,this.container,this.func_container);
            streaming = new Streaming_PC(vm,player,queue,this.container,this.func_container);
            library = new Lib_PC(vm,player,queue,this.container,this.func_container);
            if(vm.isOnline)
                go("library", "tracks");
            else
                go("local", "tracks");
        }
        public void go(string type, string page)
        {
            //オフライン時はローカル以外に飛ばない
            if (!vm.isOnline&&type!="local")
            {
                MessageBox.Show("オフライン時はローカルのみ利用可能です。");
                go(this.type, this.page);
                return;
            }
            this.type = type;
            this.page = page;
            //ページタイトル変更
            switch (this.type)
            {
                case "local":
                    vm.Type = "ローカル";
                    break;
                case "library":
                    vm.Type = "ライブラリ";
                    break;
                case "streaming":
                    vm.Type = "ストリーミング";
                    break;
                default:
                    vm.Type = "不明";
                    break;
            }
            switch (this.page)
            {
                case "tracks":
                    vm.Page = "曲";
                    break;
                case "albums":
                    vm.Page = "アルバム";
                    break;
                case "artists":
                    vm.Page = "アーティスト";
                    break;
                case "youtube":
                    vm.Page = "Youtube";
                    break;
                case "playlists":
                    vm.Page = "プレイリスト";
                    break;
            }
            //データを取得
            getdata(vm);
            //入力養素の作成
        }
        private void getdata(ViewModel vm)
        {
            switch (type)
            {
                case "library":
                    library.Go(page);
                    break;
                case "local":
                    local.Go(page);
                    break;
                case "streaming":
                    streaming.Go(page);
                    break;
            }
        }
        
    }
}