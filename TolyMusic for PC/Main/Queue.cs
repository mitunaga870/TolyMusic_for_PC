using System;
using System.Windows;
using System.Windows.Controls;

namespace TolyMusic_for_PC
{
    public class Queue
    {
        private ViewModel vm;
        private bool opened;
        private ListView list;
        public Queue(ViewModel vm, ListView list)
        {
            this.vm = vm;
            this.list = list;
        }
        //キュー設定イベント
        public void set()
        {
            list.ItemsSource = vm.PlayQueue;
        }
        //ボタン表示イベント
        public void showbutton()
        {
            vm.Queue_bt_height = 20;
        }
        //表示
        public void show()
        {
            vm.Queue_list_height = 500;
            opened = true;
        }
        //非表示
        public void hide()
        {
            vm.Queue_list_height = 0;
            opened = false;
        }
        //表示トグル
        public void toggle()
        {
            if (opened)
            {
                hide();
            }
            else
            {
                show();
            }
        }
        //シャフルイベント
        public void Shuffle()
        {
            var tmp = vm.PlayQueue[vm.Curt_queue_num];
            vm.PlayQueue[vm.Curt_queue_num] = vm.PlayQueue[0];
            vm.PlayQueue[0] = tmp;
            for (int i = vm.PlayQueue.Count-1; i > 1; i--)
            {
                var j = new Random().Next(1,i);
                var TMP = vm.PlayQueue[i];
                vm.PlayQueue[i] = vm.PlayQueue[j];
                vm.PlayQueue[j] = TMP;
            }
            vm.Curt_queue_num = 0;
        }
    }
}