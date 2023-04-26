using System.Collections.ObjectModel;
using System.Windows;
using MySqlConnector;

namespace TolyMusic_for_PC.Local
{
    public class LocalFunc
    {
        private ViewModel vm;
        private Player player;
        private Queue queue;
        private Local.Main local;
        private Library.Main lib;
        //コンストラクタ
        public LocalFunc(ViewModel vm, Player player, Queue queue, Main local)
        {
            this.vm = vm;
            this.player = player;
            this.queue = queue;
            this.local = local;
            lib = new Library.Main(vm);
        }
        //共通メソッド
        private void MakeQueue()
        {
            switch (vm.Listtype)
            {
                case ViewModel.TypeEnum.Track:
                    vm.PlayQueue = local.GetTracks();
                    break;
                case ViewModel.TypeEnum.Album:
                    vm.PlayQueue = local.GetTracks(vm.Curt_Album.Id,Main.id_type.album);
                    break;
                case ViewModel.TypeEnum.Artist:
                    vm.PlayQueue = local.GetTracks(vm.Curt_Artist.Id,Main.id_type.artist);
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
        //ライブラリに追加
        public void AddLibAll(object sender = null, RoutedEventArgs e = null)
        {
            //tracks
            Collection<Track> tracks = new Collection<Track>();
            switch (vm.Listtype)
            {
                case ViewModel.TypeEnum.Track:
                    tracks = vm.Tracks;
                    break;
                case ViewModel.TypeEnum.Album:
                    tracks = local.GetTracks(vm.Curt_Album.Id, Main.id_type.album);
                    break;
                case ViewModel.TypeEnum.Artist:
                    tracks = local.GetTracks(vm.Curt_Artist.Id, Main.id_type.artist);
                    break;
                default:
                    break;
            }
            //パラメータ作成
            lib.AddListTracks(tracks);
        }
    }
}