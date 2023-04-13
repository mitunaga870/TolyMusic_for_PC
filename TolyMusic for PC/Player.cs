using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using NAudio.Wave;

namespace TolyMusic_for_PC
{
    public class Player
    {
        //変数宣言
        ViewModel vm; 
        bool isASIO;
        AsioOut asio;
        WasapiOut wasapi;
        public bool isPlaying = false;
        private AudioFileReader reader;
        private Task timesetter;
        public Player(ViewModel vm)
        {
            this.vm = vm;
            isASIO = Properties.Settings.Default.EDisASIO;
        }
        //初期化処理
        public void Init()
        {

            if (vm.Excl&&isASIO)//排他・ASIO
            {
                //ドライバ指定
                if (asio != null)
                {
                    asio.PlaybackStopped -= new EventHandler<StoppedEventArgs>(Ended);
                    asio.Dispose();
                }
                asio = new AsioOut(vm.Curt_Driver);
                asio.PlaybackStopped += new EventHandler<StoppedEventArgs>(Ended);
            }else if (vm.Excl)//排他WASAPI
            {
                //ドライバ指定
            }
            else//共有WASAPI
            {
                //ドライバ指定
                if (Properties.Settings.Default.NDcustumized)
                {
                    wasapi = new WasapiOut();
                }
                else//デフォルト
                {
                    if (wasapi != null)
                    {
                        wasapi.PlaybackStopped -= new EventHandler<StoppedEventArgs>(Ended);
                        wasapi.Dispose();
                    }
                    wasapi = new WasapiOut();
                    wasapi.PlaybackStopped += new EventHandler<StoppedEventArgs>(Ended);
                }
            }
        }
        //キュー開始処理
        public void Start()
        {
            Init();
            reader = new AudioFileReader(vm.Curt_track.Path);
            vm.Curt_length = reader.TotalTime.Ticks;
            if(isASIO)
                asio.Init(reader);
            else
                wasapi.Init(reader);
            Play();
        }
        //再生処理
        public void Play()
        {
            if (isASIO)
                asio.Play();
            else
                wasapi.Play();
            isPlaying = true;
            timesetter = new Task(TimeControler);
            timesetter.Start();
        }
        //一時停止処理
        public void Pause()
        {
            if(isASIO)
                asio.Pause();
            else
                wasapi.Pause();
            isPlaying = false;
        }
        //再生終了時の処理
        public void Ended(object obj, StoppedEventArgs e)
        {
            //停止処理 
            isPlaying = false;
            if (vm.Loop == null)//一曲ループ処理
            {
                reader.CurrentTime = TimeSpan.FromTicks(0);
                Play();
            }else if(vm.PlayQueue.Count - 1 == vm.Curt_queue_num && !(bool)vm.Loop)//キューの最後の終了処理
            {
                Dispose();
            }
            else
            {
                next();
            }
        }

        public void next()
        {
            if (vm.PlayQueue.Count - 1 == vm.Curt_queue_num && (bool)vm.Loop) //キューのループ処理
            {
                vm.Curt_queue_num = 0;
                vm.Curt_track = vm.PlayQueue[0];
            }
            else
            {
                vm.Curt_queue_num++;
                vm.Curt_track = vm.PlayQueue[vm.Curt_queue_num];
            }
            reader.Dispose();
            reader = new AudioFileReader(vm.Curt_track.Path);
            vm.Curt_length = reader.CurrentTime.Ticks;
            Start();
        }

        //再生位置の変更
        private void TimeControler()
        {
            while (isPlaying)
            {
                vm.Curt_time = reader.CurrentTime.Ticks;
                if (vm.Next_time != -1)
                {
                    reader.CurrentTime = TimeSpan.FromTicks(vm.Next_time);
                    vm.Next_time = -1;
                }
            }
        }
        //終了処理
        public void Dispose()
        {
            if (isASIO)
            {
                asio.Dispose();
            }
            else
            {
                wasapi.Dispose();
            }
            reader.Dispose();
        }
    }
}