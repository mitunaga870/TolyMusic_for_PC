using System;
using System.IO;
using System.Windows;
using NAudio.Wave;

namespace TolyMusic_for_PC
{
    public class Player
    {
        //変数宣言
        ViewModel vm;
        AsioOut asio;
        public bool isPlaying = false;
        public Player(ViewModel vm)
        {
            this.vm = vm;
        }
        //初期化処理
        public void Init()
        {
            //ドライバ指定
            if(asio != null)
                asio.Dispose();
            asio = new AsioOut(vm.Curt_Driver);
            asio.PlaybackStopped += new EventHandler<StoppedEventArgs>(Ended);
        }
        //キュー開始処理
        public void Start()
        {
            Init();
            if(vm.Curt_Driver == null)
            {
                MessageBox.Show("ドライバーが選択されていません");
                return;
            }
            AudioFileReader reader = new AudioFileReader(vm.Curt_track.Path);
            asio.Init(reader);
            Play();
        }
        //再生処理
        public void Play()
        {
            if (vm.Curt_Driver == null)
            {
                MessageBox.Show("ドライバーが選択されていません");
                return;
            }
            asio.Play();
            isPlaying = true;
        }
        //一時停止処理
        public void Pause()
        {
            if (vm.Curt_Driver == null)
            {
                MessageBox.Show("ドライバーが選択されていません");
                return;
            }
            asio.Pause();
            isPlaying = false;
        }
        //再生終了時の処理
        public void Ended(object obj, StoppedEventArgs e)
        {
            MessageBox.Show("再生終了");
        }
    }
}