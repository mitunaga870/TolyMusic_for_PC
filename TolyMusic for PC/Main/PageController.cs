using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace TolyMusic_for_PC
{
    public class PageController
    {
        private Local.Main local;
        private bool loadedlocal = false;
        private string type;
        private string page;
        public PageController(ViewModel vm)
        {
            go("library", "tracks", vm);
        }
        public void go(string type, string page ,ViewModel vm)
        {
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
                case "youtube":
                    vm.Type = "YouTube";
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
                case "local":
                    if(!loadedlocal)
                    {
                        local = new Local.Main();
                        loadedlocal = true;
                    }
                    switch (page)
                    {
                        case "tracks":
                            vm.Tracks = local.GetTracks();
                            break;
                        case "albums":
                            break;
                        case "artists":
                            break;
                    }
                    break;
            }
        }
    }
}