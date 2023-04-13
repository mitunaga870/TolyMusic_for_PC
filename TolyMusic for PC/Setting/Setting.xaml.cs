using System;
using System.ComponentModel;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using TolyMusic_for_PC.Local;
using Label = System.Windows.Controls.Label;

namespace TolyMusic_for_PC
{
    public partial class Setting : Window
    {
        //変数宣言
        private Setting_ViewModel vm;
        private Setting_Page_Local spl;
        private Setting_Page_general spg;
        public Setting()
        {
            InitializeComponent();
            vm = new Setting_ViewModel();
            DataContext = vm;
            spl = new Setting_Page_Local(vm);
            spg = new Setting_Page_general(vm);
        }
        //ページ初期化イベント
        private void Page_init()
        {
            main.Children.Clear();
        }
        //ページ繊維イベント
        private void Open_Local_Directory(object sender, RoutedEventArgs e)
        {
            Page_init();
            spl.go_local_directory(main);
        }
        private void Send(object sender, RoutedEventArgs e)
        {
            //LocalDirectoryPath
            string send = "";
            foreach (var path in vm.path_list)
            {
                send += path + ",";
            }
            send = send.Substring(0, send.Length - 1);
            Properties.Settings.Default.LocalDirectryPath = send;
            Properties.Settings.Default.Save();
            vm.Init();
            this.Close();
        }

        private void Open_general_Device(object sender, RoutedEventArgs e)
        {
            Page_init();
            spg.go_general_device(main);
        }
    }
}