using System.Windows.Controls;
using System.Windows.Data;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.Asio;

namespace TolyMusic_for_PC.Local
{
    public class Setting_Page_general
    {
        //変数宣言
        Setting_ViewModel vm;
        //コンストラクタ
        public Setting_Page_general(Setting_ViewModel vm)
        {
            this.vm = vm;
        }
        //ページ遷移
        public void go_general_driver(StackPanel sp)
        {
            //要素追加
            sp.Children.Add(new Label() { Content = "共有時設定デバイス" });
            ComboBox sd_cb = new ComboBox();
            sd_cb.Name = "Share_Device";
            sd_cb.ItemsSource = vm.Share_driver_list;
            sd_cb.DisplayMemberPath = "DisplayName"; 
            sd_cb.SelectedIndex = vm.Selected_share;
            sd_cb.SelectionChanged += new SelectionChangedEventHandler(((sender, args) =>
            {
                vm.Selected_share = sd_cb.SelectedIndex;
            }));
            sp.Children.Add(sd_cb);
            sp.Children.Add(new Label() { Content = "排他時設定デバイス" });
            ComboBox ex_cb = new ComboBox();
            ex_cb.Name = "Excl_Device";
            ex_cb.ItemsSource = vm.Excl_driver_list;
            ex_cb.DisplayMemberPath = "DisplayName"; 
            ex_cb.SelectedIndex = vm.Selected_excl;
            ex_cb.SelectionChanged += new SelectionChangedEventHandler(((sender, args) =>
            {
                vm.Selected_excl = ex_cb.SelectedIndex;
            }));
            vm.Selected_excl = ex_cb.SelectedIndex;
            sp.Children.Add(ex_cb);
        }
        public void go_library_database(StackPanel main)
        {
            //Adress
            main.Children.Add(new Label() { Content = "アドレス" });
            TextBox Adress = new TextBox();
            Adress.Name = "Adress";
            Adress.SetBinding(TextBox.TextProperty, new Binding("DatabaseSeverAdress") { Source = vm });
            main.Children.Add(Adress);
            //Port
            main.Children.Add(new Label() { Content = "ポート" });
            TextBox Port = new TextBox();
            Port.Name = "Port";
            Port.SetBinding(TextBox.TextProperty, new Binding("DatabaseSeverPort") { Source = vm });
            main.Children.Add(Port);
            //User
            main.Children.Add(new Label() { Content = "ユーザー名" });
            TextBox User = new TextBox();
            User.Name = "User";
            User.SetBinding(TextBox.TextProperty, new Binding("DatabaseSeverUser") { Source = vm });
            main.Children.Add(User);
            //Password
            main.Children.Add(new Label() { Content = "パスワード" });
            PasswordBox Password = new PasswordBox();
            Password.Name = "Password";
            vm.DatabaseSeverPassword = Password;
            main.Children.Add(Password);
        }

        public void go_streaming_youtube(StackPanel main)
        {
            //Playlist
            main.Children.Add(new Label() { Content = "同期プレイリスト" });
            TextBox Playlist = new TextBox();
            Playlist.Name = "Playlist";
            Playlist.SetBinding(TextBox.TextProperty, new Binding("YoutubePlaylist") { Source = vm });
            main.Children.Add(Playlist);
        }
    }
}