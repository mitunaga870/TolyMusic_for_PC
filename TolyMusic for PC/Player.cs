using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace TolyMusic_for_PC
{
    public class Player
    {
        //変数宣言
        ViewModel vm; 
        private bool isASIO;
        AsioOut asio;
        WasapiOut wasapi;
        public bool isPlaying = false;
        private AudioFileReader reader;
        private Task timesetter;
        public bool started = false;
        public Player(ViewModel vm)
        {
            this.vm = vm;
            isASIO = false;
        }
        //初期化処理
        public void Init()
        {
            if (vm.Excl&& Properties.Settings.Default.EDisASIO)//排他・ASIO
            {
                isASIO = true;
                //ドライバ破棄
                if (asio != null)
                {
                    asio.PlaybackStopped -= new EventHandler<StoppedEventArgs>(Ended);
                    asio.Dispose();
                } 
                if (wasapi != null)
                {
                    wasapi.PlaybackStopped -= new EventHandler<StoppedEventArgs>(Ended);
                    wasapi.Dispose();
                }
                //ドライバ指定
                asio = new AsioOut(vm.Excl_Driver.Name);
                asio.PlaybackStopped += new EventHandler<StoppedEventArgs>(Ended);
                asio.AutoStop = true;
            }else if (vm.Excl)//排他WASAPI
            {
                isASIO = false;
                //ドライバ破棄
                if (asio != null)
                {
                    asio.PlaybackStopped -= new EventHandler<StoppedEventArgs>(Ended);
                    asio.Dispose();
                }
                if (wasapi != null)
                {
                    wasapi.PlaybackStopped -= new EventHandler<StoppedEventArgs>(Ended);
                    wasapi.Dispose();
                }
                //ドライバ指定
                if (Properties.Settings.Default.SDcustumized)
                {
                    wasapi = new WasapiOut(vm.Excl_Driver.mmdevice, AudioClientShareMode.Exclusive, false, 100);
                }
                else
                {
                    wasapi = new WasapiOut(AudioClientShareMode.Exclusive,100);
                }
                wasapi.PlaybackStopped += new EventHandler<StoppedEventArgs>(Ended);
            }
            else//共有WASAPI
            {
                isASIO = false;
                //ドライバ破棄
                if (wasapi != null) 
                { 
                    wasapi.PlaybackStopped -= new EventHandler<StoppedEventArgs>(Ended); 
                    wasapi.Dispose(); 
                }
                if (asio != null)
                {
                    asio.PlaybackStopped -= new EventHandler<StoppedEventArgs>(Ended);
                    asio.Dispose();
                }
                //ドライバ指定
                if (Properties.Settings.Default.SDcustumized)
                {
                    wasapi = new WasapiOut(vm.Share_Driver.mmdevice, AudioClientShareMode.Shared, false, 100);
                }
                else//デフォルト
                {
                    wasapi = new WasapiOut();
                }
                wasapi.PlaybackStopped += new EventHandler<StoppedEventArgs>(Ended);
            }
        }
        //キュー開始処理
        public void Start()
        {
            Init();
            started = true;
            try
            {
                reader = new AudioFileReader(vm.Curt_track.Path);
            }
            catch (Exception e)
            {
                MessageBox.Show("ファイルが見つかりませんでした。");
                Dispose();
                return;
            }
            vm.Curt_length = reader.TotalTime.Ticks;
            if(vm.Excl)
                SetVol();
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
            Start();
        }
        //巻き戻し
        public void prev()
        {
            if(vm.Curt_time < 10000000)
            {
                if (vm.Curt_queue_num == 0)
                {
                    reader.CurrentTime = TimeSpan.FromTicks(0);
                }
                else
                {
                    vm.Curt_queue_num--;
                    vm.Curt_track = vm.PlayQueue[vm.Curt_queue_num];
                    reader.Dispose();
                    Start();
                }
            }
            else
            {
                reader.CurrentTime = TimeSpan.FromTicks(0);
            }
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
        //Volume
        public void SetVol()
        {
            if (started)
            {
                reader.Volume = (float)vm.Volume / 100;
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
            isPlaying = false;
        }
    }
}