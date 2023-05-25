using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using TolyMusic_for_PC.Streaming.Handlar;

namespace TolyMusic_for_PC
{
    public class Player
    {
        //変数宣言
        private enum Locaion
        {
            youtube,
            local,
            tois
        }

        private Locaion locaiton;
        ViewModel vm;
        private bool isASIO;
        public AsioOut asio;
        public WasapiOut wasapi;
        public bool isPlaying = false;
        private AudioFileReader afreader;
        private MediaFoundationReader mfreader;
        private Task timesetter;
        public bool started = false;
        private WebClient webClient;
        private Grid container;
        private bool webloaded;
        private ChromiumWebBrowser browser;

        public Player(ViewModel vm, Grid container)
        {
            this.vm = vm;
            this.container = container;
            webloaded = false;
            isASIO = false;
            webClient = new WebClient();
        }

        //初期化処理
        public void Init()
        {
            if (vm.Excl && Properties.Settings.Default.EDisASIO) //排他・ASIO
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
            }
            else if (vm.Excl) //排他WASAPI
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
                    wasapi = new WasapiOut(AudioClientShareMode.Exclusive, 100);
                }

                wasapi.PlaybackStopped += new EventHandler<StoppedEventArgs>(Ended);
            }
            else //共有WASAPI
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
                else //デフォルト
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
                switch (vm.Curt_track.location)
                {
                    case 0: //localトラックの時
                        string path = Properties.Settings.Default.LocalDirectryPath.Replace('\\', '/') + "/" + vm.Curt_track.Path;
                        afreader = new AudioFileReader(path);
                        locaiton = Locaion.local;
                        break;
                    case 1: //youtubeトラックの時
                        //パケットのsendURLを取得し、同期的に更新
                        afreader = GetReqURLReader(String.Format("https://youtube.com/watch?v={0}&autoplay=1",
                            vm.Curt_track.youtube_id));
                        locaiton = Locaion.youtube;
                        break;
                    default:
                        throw new Exception();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("ファイルが見つかりませんでした。");
                Dispose();
                return;
            }

            if (vm.Excl)
                SetVol();
            switch (locaiton)
            {
                case Locaion.local:
                case Locaion.youtube:
                    vm.Curt_length = afreader.TotalTime.Ticks;
                    if (vm.Excl)
                        SetVol();
                    if (isASIO)
                        asio.Init(afreader);
                    else
                        wasapi.Init(afreader);
                    break;
                
            }

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
            if (isASIO)
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
            if (vm.Loop == null) //一曲ループ処理
            {
                vm.Next_time = 0;
                Play();
            }
            else if (vm.PlayQueue.Count - 1 == vm.Curt_queue_num && !(bool)vm.Loop) //キューの最後の終了処理
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
            if (vm.PlayQueue.Count -1 == vm.Curt_queue_num) //キューのループ処理
            {
                vm.Curt_queue_num = 0;
                vm.Curt_track = vm.PlayQueue[0];
            }
            else
            {
                vm.Curt_queue_num++;
                vm.Curt_track = vm.PlayQueue[vm.Curt_queue_num];
            }

            if (afreader != null)
                afreader.Dispose();
            if (mfreader != null)
                mfreader.Dispose();
            Start();
        }

        //巻き戻し
        public void prev()
        {
            if (vm.Curt_time < 10000000)
            {
                if (vm.Curt_queue_num == 0)
                {
                    vm.Next_time = 0;
                }
                else
                {
                    vm.Curt_queue_num--;
                    vm.Curt_track = vm.PlayQueue[vm.Curt_queue_num];
                    if (afreader != null)
                        afreader.Dispose();
                    if (mfreader != null)
                        mfreader.Dispose();
                    Start();
                }
            }
            else
            {
                vm.Next_time = 0;
            }
        }

        //再生位置の変更
        private void TimeControler()
        {
            switch (locaiton)
            {
                case Locaion.local:
                case Locaion.youtube:
                    while (isPlaying)
                    {
                        vm.Curt_time = afreader.CurrentTime.Ticks;
                        if (vm.Next_time != -1)
                        {
                            afreader.CurrentTime = TimeSpan.FromTicks(vm.Next_time);
                            vm.Next_time = -1;
                        }
                    }
                    break;
            }
        }

        //Volume
        public void SetVol()
        {
            if (started)
            {
                afreader.Volume = (float)vm.Volume / 100;
            }
        }

        //終了処理
        public void Dispose()
        {
            vm.Curt_track = null;
            if (vm.PlayQueue.Count != 0)
                vm.PlayQueue.Clear();
            vm.Curt_queue_num = -1;
            if (isASIO)
            {
                asio.PlaybackStopped -= new EventHandler<StoppedEventArgs>(Ended);
                asio.Dispose();
            }
            else
            {
                wasapi.PlaybackStopped -= new EventHandler<StoppedEventArgs>(Ended);
                wasapi.Dispose();
            }

            if (afreader != null)
                afreader.Dispose();
            if (mfreader != null)
                mfreader.Dispose();
            isPlaying = false;
        }

        //webstremingのとき
         private AudioFileReader GetReqURLReader(string url)
        {
            if (!webloaded)
            {
                browser = new ChromiumWebBrowser(url);
                browser.RequestHandler = new YoutubeReqHandler(ref wasapi,ref asio);
                container.Children.Add(browser);
                webloaded = true;
            }
            else
                browser.Address = url;
            //ロード完了を待つ
            return new AudioFileReader("https://rr1---sn-nvoxu-ioqel.googlevideo.com/videoplayback?expire=1684952699&ei=GwJuZNO1ELCXvcAPm-mJgAM&ip=240d%3A1a%3Ab2e%3Ad300%3A8873%3A2ab2%3Ac81%3A8d74&id=o-AA0-jS15WoIPtHId0XsVYA3a4Gfrpkm7Y-OxEN3YFcHA&itag=141&source=youtube&requiressl=yes&mh=mG&mm=31%2C29&mn=sn-nvoxu-ioqel%2Csn-oguesnds&ms=au%2Crdu&mv=m&mvi=1&pl=39&ctier=A&pfa=5&gcr=jp&initcwndbps=1446250&hightc=yes&spc=qEK7BwvFx7SHMufMGRAQahYz77d83diKcDzXEEF_Wi_K&vprv=1&svpuc=1&mime=audio%2Fmp4&ns=bD-pfrkr36Z27COlyO-empEN&gir=yes&clen=5121366&dur=158.999&lmt=1682668068591618&mt=1684930625&fvip=2&keepalive=yes&fexp=24007246&beids=24350017&c=WEB_REMIX&txp=2318224&n=o8TPjuvZRJmxPA&sparams=expire%2Cei%2Cip%2Cid%2Citag%2Csource%2Crequiressl%2Cctier%2Cpfa%2Cgcr%2Chightc%2Cspc%2Cvprv%2Csvpuc%2Cmime%2Cns%2Cgir%2Cclen%2Cdur%2Clmt&lsparams=mh%2Cmm%2Cmn%2Cms%2Cmv%2Cmvi%2Cpl%2Cinitcwndbps&lsig=AG3C_xAwRAIgS_iMKY7o-ZJ3sP0d-W93xaUBT6RsvXsjw9A7b5_1D7oCIEDCTjUbhaxrOqtr67prD8cBW871LoMLhV3cOmF4jsrx&alr=yes&sig=AOq0QJ8wRAIgMBmUEP3m3QjD6RWv20ytSTVDFgwi2eWM-Pd9SpFDpVgCICfIfHQ-yvx6-hGue9AY8p33fh-1wrkl3XnFkIA8psur&cpn=z7_ExFfnjTRcqKAu&cver=1.20230517.01.00&range=0-5121365&rn=11&rbuf=76815&pot=MlunjWxGoTXaR6e1-JEZoxdqxZv8798XIutQ_6BSmptII-cJpSYjw-iRQ08RKIhqq7SSAlM_78TrDE-oZ9cU8-GolwgMqnnNJDZOEOtQZH4JiEFXSLFJTJ36YMFh");
        }
    }
}