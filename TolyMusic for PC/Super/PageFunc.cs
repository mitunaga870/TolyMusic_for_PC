using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TolyMusic_for_PC.Library;

namespace TolyMusic_for_PC.Super
{
    public abstract class PageFunc
    {
        protected ViewModel vm;
        protected Player player;
        protected Queue queue;
        protected Grid container;
        protected StackPanel func_container;

        //コンストラクタ
        public PageFunc(ViewModel vm, Player player, Queue queue, Grid container, StackPanel funcContainer, object main, object PageControler)
        {
            this.vm = vm;
            this.player = player;
            this.queue = queue;
            this.container = container;
            this.func_container = funcContainer;
        }

        protected abstract void MakeQueue();
        //純再生
        public virtual void PlayAll(object sender = null, RoutedEventArgs e = null)
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
        public virtual void ShuffleAll(object sender = null, RoutedEventArgs e = null)
        {
            //キュー作成作業
            MakeQueue();
            vm.Curt_queue_num = 0;
            for (int i = vm.PlayQueue.Count - 1; i > 0; i--)
            {
                int r = new System.Random().Next(i + 1);
                Track tmp = vm.PlayQueue[i];
                vm.PlayQueue[i] = vm.PlayQueue[r];
                vm.PlayQueue[r] = tmp;
            }
            vm.Curt_track = vm.PlayQueue[vm.Curt_queue_num];
            //キュー生成
            queue.set();
            queue.showbutton();
            //再生
            player.Start();
        }

        protected ListView MakeList(Dictionary<string,string> head_path,ViewModel.TypeEnum type,MouseButtonEventHandler DoubleClickEvent,ref GridView row)
        {
            //メインリスト作成
            ListView mainlist = new ListView();
            mainlist.SelectionMode = SelectionMode.Single;
            switch (type)
            {
                case ViewModel.TypeEnum.Track:
                    mainlist.ItemsSource = vm.Tracks;
                    break;
                case ViewModel.TypeEnum.Album:
                    mainlist.ItemsSource = vm.Albums;
                    break;
                case ViewModel.TypeEnum.Artist:
                    mainlist.ItemsSource = vm.Artists;
                    break;
            }
            mainlist.HorizontalAlignment = HorizontalAlignment.Stretch;
            ////メインリスト用スタイル
            Style mainlist_style = new Style(typeof(ListViewItem));
            //ダブルクリックイベント
            EventSetter play_event = new EventSetter();
            play_event.Event = ListViewItem.MouseDoubleClickEvent;
            play_event.Handler = DoubleClickEvent;
            mainlist_style.Setters.Add(play_event);
            mainlist.ItemContainerStyle = mainlist_style;
            //行テンプレート
            foreach (var hp in head_path)
            {
                GridViewColumn Name = new GridViewColumn();
                Name.Header = hp.Key;
                Name.DisplayMemberBinding = new System.Windows.Data.Binding(hp.Value);
                row.Columns.Add(Name);
            }
            mainlist.View = row;
            return mainlist;
        }

        public void Search(object sender, RoutedEventArgs e)
        {
            ListView mainlist = (ListView)container.Children[container.Children.Count - 1];
            string keyword = ((TextBox)sender).Text;
            var list = mainlist.ItemsSource;
            switch (vm.Listtype)
            {
                case ViewModel.TypeEnum.Track:
                    if(keyword == "")
                    {
                        mainlist.ItemsSource = vm.Tracks;
                        break;
                    }
                    var flist = vm.Tracks.Where(t => t.Title.Contains(keyword));
                    mainlist.ItemsSource = flist;
                    break;
                case ViewModel.TypeEnum.Album:
                    if (keyword == "")
                    {
                        mainlist.ItemsSource = vm.Albums;
                        break;
                    }
                    var flist2 = vm.Albums.Where(t => t.Title.Contains(keyword));
                    mainlist.ItemsSource = flist2;
                    break;
                case ViewModel.TypeEnum.Artist:
                    if (keyword == "")
                    {
                        mainlist.ItemsSource = vm.Artists;
                        break;
                    }
                    var flist3 = vm.Artists.Where(t => t.Name.Contains(keyword));
                    mainlist.ItemsSource = flist3;
                    break;
            }
        }
    }
}