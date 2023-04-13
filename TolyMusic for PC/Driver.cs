using NAudio.CoreAudioApi;

namespace TolyMusic_for_PC
{
    public class Driver
    {
        //メンバ偏す宣言
        public string DisplayName { set; get; }
        public bool isAsio { set; get; }
        public string Name { set; get; }
        public  MMDevice mmdevice { set; get; }
        //コンストラクタ
        public Driver()
        {
            isAsio = false;
            DisplayName = "デフォルトデバイス";
        }

        public Driver(MMDevice mmd)
        {
            isAsio = false;
            mmdevice = mmd;
            DisplayName = "WASAPI:"+mmd.FriendlyName;
            Name = mmd.DeviceFriendlyName;
        }

        public Driver(string asioName)
        {
            Name = asioName;
            DisplayName = "ASIO:" + asioName;
            isAsio = true;
        }
    }
}