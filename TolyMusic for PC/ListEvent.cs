using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace TolyMusic_for_PC;

public class ListEvent
{
    protected ViewModel vm;
    protected Player player;
    protected Queue.Main queue;
    //コンストラクタ
    public ListEvent(ViewModel vm, Player player, Queue.Main queue)
    {
        this.vm = vm;
        this.player = player;
        this.queue = queue;
    }
    //トラックリストでの再生イベント
    public void PlayTrack(object sender, MouseButtonEventArgs e)
    {
        //再生中の曲を停止
        player.Close();
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
    }
}