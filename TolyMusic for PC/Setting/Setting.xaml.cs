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
        private enum SettingPage 
        {
            Home,
            General_Deiver,
            Local_Directory,
            Library_DataBaseServer
        }

        private SettingPage title;
        public Setting()
        {
            InitializeComponent();
            vm = new Setting_ViewModel();
            DataContext = vm;
            spl = new Setting_Page_Local(vm);
            spg = new Setting_Page_general(vm);
            title = SettingPage.Home;
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
            title = SettingPage.Local_Directory;
            spl.go_local_directory(main);
        }
        private void Send(object sender, RoutedEventArgs e)
        {
            var send_obj = Properties.Settings.Default;
            //LocalDirectoryPath
            string send = "";
            foreach (var path in vm.path_list)
            {
                send += path + ",";
            }
            send = send.Substring(0, send.Length - 1);
            send_obj.LocalDirectryPath = send;
            //Driver
            bool custumized_share = vm.Selected_share != 0;
            if (custumized_share)
                send_obj.ShareDriver = vm.Share_driver_list[vm.Selected_share].Name;
            else
                send_obj.ShareDriver = "";
            send_obj.SDcustumized = custumized_share;
            bool custumized_excl = vm.Selected_excl != 0;
            if (custumized_excl)
            {
                send_obj.ExclutionDriver = vm.Excl_driver_list[vm.Selected_excl].Name;
                send_obj.EDisASIO = vm.Excl_driver_list[vm.Selected_excl].isAsio;
            }
            else
                send_obj.ExclutionDriver = "";
            send_obj.EDcustumized = custumized_excl;
            //Library
            //DataBaseServer
            send_obj.LibraryServerAdress = vm.DatabaseSeverAdress;
            send_obj.LibraryServerPort = vm.DatabaseSeverPort;
            send_obj.LibraryServerUser = vm.DatabaseSeverUser;
            if(vm.DatabaseSeverPassword.Password != "")
                send_obj.LibraryServerPass = vm.DatabaseSeverPassword.Password;
            //Streaming
            //youtube
            send_obj.YoutubePlaylist = vm.YoutubePlaylist;
            //send
            send_obj.Save();
            vm.Init();
            this.Close();
        }

        private void Open_general_Device(object sender, RoutedEventArgs e)
        {
            Page_init();
            title = SettingPage.General_Deiver;
            spg.go_general_driver(main);
        }

        private void Open_Library_DataBaseServer(object sender, RoutedEventArgs e)
        {
            Page_init();
            title = SettingPage.Library_DataBaseServer;
            spg.go_library_database(main);
        }
        
        private void Open_Streaming_Youtube(object sender, RoutedEventArgs e)
        {
            Page_init();
            title = SettingPage.Home;
            spg.go_streaming_youtube(main);
        }
    }
}