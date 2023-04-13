using System.Windows.Controls;
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
        public void go_general_device(StackPanel sp)
        {
            //要素追加
            sp.Children.Add(new Label() { Content = "共有時設定デバイス" });
            ComboBox sd_cb = new ComboBox();
            sd_cb.Name = "Share_Device";
            sd_cb.ItemsSource = vm.Share_device_list;
            sd_cb.SelectedIndex = 0;
            sp.Children.Add(sd_cb);
            sp.Children.Add(new Label() { Content = "排他時設定デバイス" });
            ComboBox ex_cb = new ComboBox();
            ex_cb.Name = "Excl_Device";
            ex_cb.ItemsSource = vm.Excl_device_list;
            ex_cb.SelectedIndex = 0;
            sp.Children.Add(ex_cb);
            //接続デバイス読み込み
             MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
             foreach (var wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.All,DeviceState.Active))
             {
                 vm.Excl_device_list.Add(wasapi); 
                 vm.Share_device_list.Add(wasapi);
             }
             foreach (var asio in AsioOut.GetDriverNames())
                 vm.Excl_device_list.Add(asio);
             enumerator.Dispose();
        }
    }
}