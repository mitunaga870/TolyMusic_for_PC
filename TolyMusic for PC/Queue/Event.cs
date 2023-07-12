using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CefSharp;

namespace TolyMusic_for_PC.Queue;

public class Event : ListEvent
{
    private ListView Qlist;
    private ListViewItem catched_item;
    private int? catched_num;
    private Point? prev_point;
    private Track drag_data;
    public Event(ViewModel vm, Player player, Main queue ,ListView Qlist) : base(vm, player, queue)
    {
        this.Qlist = Qlist;
        //値の初期化
        Init();
        //イベントセット
        var item_style = new Style(typeof(ListViewItem));
        var play_track = new EventSetter();
        play_track.Event = ListViewItem.MouseDoubleClickEvent;
        play_track.Handler = new MouseButtonEventHandler(PlayTrack);
        item_style.Setters.Add(play_track);
        //ドラック＆ドロップ並び替え
        //クリックで取得
        var catch_track = new EventSetter();
        catch_track.Event = ListViewItem.PreviewMouseLeftButtonDownEvent;
        catch_track.Handler = new MouseButtonEventHandler(CatchTrack);
        item_style.Setters.Add(catch_track);
        //移動ドロップ処理
        var moving_track = new EventSetter();
        moving_track.Event = ListViewItem.PreviewMouseMoveEvent;
        moving_track.Handler = new MouseEventHandler(MovingTrack);
        item_style.Setters.Add(moving_track);
        //ドロップ時の処理
        var drop_track = new EventSetter();
        drop_track.Event = ListViewItem.DropEvent;
        drop_track.Handler = new DragEventHandler(DropTrack);
        item_style.Setters.Add(drop_track);
        //無効ドロップじにデータをリセット
        var reset_data = new EventSetter();
        reset_data.Event = ListViewItem.PreviewMouseLeftButtonUpEvent;
        reset_data.Handler = new MouseButtonEventHandler((sender, e) => Init());
        item_style.Setters.Add(reset_data);
        Qlist.ItemContainerStyle = item_style;
        
        //リストのスタイル
        //ドラックされているアイテムが正しいか
        var list_style = new Style(typeof(ListView));
        var check_drag = new EventSetter();
        check_drag.Event = ListView.DragOverEvent;
        check_drag.Handler = new DragEventHandler((sender, e) =>
        {
            if (catched_item== null || catched_num == null || catched_item.Content != drag_data)
                e.Effects = DragDropEffects.None;
            else
                e.Effects = DragDropEffects.Move;
            e.Handled = true;
        });
        list_style.Setters.Add(check_drag);
        Qlist.Style = list_style;
    }
    //値の初期化用
    private void Init()
    {
        catched_num = null;
        catched_item = null;
        prev_point = null;
        drag_data = null;
    }
    private void DropTrack(object sender, DragEventArgs e)
    {
        var drop_item = (ListViewItem)sender;
        var drop_num = Qlist.Items.IndexOf(drop_item.Content);
        
        if (drop_num == catched_num || drop_num < 0 || catched_num == null) return;
        //再生トラックに絡む場合
        //再生トラックを移動させる
        if (catched_num == vm.Curt_queue_num)
        {
            vm.Curt_queue_num = drop_num;
        }
        else if (catched_num < vm.Curt_queue_num && drop_num >= vm.Curt_queue_num)
        {
            vm.Curt_queue_num--;
        }
        else if (catched_num > vm.Curt_queue_num && drop_num <= vm.Curt_queue_num)
        {
            vm.Curt_queue_num++;
        }
        
        vm.PlayQueue.Move((int)catched_num, drop_num);
    }
    private void MovingTrack(object sender, MouseEventArgs e)
    {
        if (catched_item == null) return;
        
        var curt_point = Mouse.GetPosition(Qlist);
        var diff = curt_point - prev_point;
        //移動距離が一定以上の時のみ処理
        if (SystemParameters.MinimumHorizontalDragDistance + 2 > Math.Abs(diff.Value.X) &&
             SystemParameters.MinimumVerticalDragDistance + 2 > Math.Abs(diff.Value.Y))return;
        
        DragDrop.DoDragDrop(catched_item, drag_data, DragDropEffects.Move);
        Init();
    }
    private void CatchTrack(object sender, MouseButtonEventArgs e)
    {
        catched_item = (ListViewItem)sender;
        catched_num = Qlist.Items.IndexOf(catched_item.Content);
        prev_point = e.GetPosition(Qlist);

        drag_data = (Track)catched_item.Content;
    }
}