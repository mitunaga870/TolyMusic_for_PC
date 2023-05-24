using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        AsioOut asio;
        WasapiOut wasapi;
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
                        afreader = new AudioFileReader(vm.Curt_track.Path);
                        locaiton = Locaion.local;
                        break;
                    case 1: //youtubeトラックの時
                        //パケットのsendURLを取得し、同期的に更新
                        afreader = GetReqURLReader(String.Format("https://youtube.com/watch?id{0}&autoplay=1",
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
                container.Children.Add(browser);
                webloaded = true;
            }
            else
                browser.Address = url;
            browser.RequestHandler = new YoutubeReqHandler(afreader);
            return new AudioFileReader("https://rr4---sn-nvoxu-ioqk.googlevideo.com/videoplayback?expire=1684944254&ei=HuFtZI3JMfWTvcAP0da0kAs&ip=90.149.84.214&id=o-ABGe68XS-FJ_cXHUJdqhHrj3X8q-wVbFZtZyEoRzL4gF&itag=251&source=youtube&requiressl=yes&mh=GJ&mm=31%2C29&mn=sn-nvoxu-ioqk%2Csn-ogueln67&ms=au%2Crdu&mv=m&mvi=4&pl=18&ctier=A&pfa=5&initcwndbps=1728750&hightc=yes&spc=qEK7BwWlUCXoOqtI6KAFlH5MrY2I7ZMAdSp4ur4B04xL2ApgRtI2-0o&vprv=1&svpuc=1&mime=audio%2Fwebm&ns=kKA9GGB7xf7U9z1xiLcNWx8N&gir=yes&clen=4116968&dur=255.021&lmt=1684262764585279&mt=1684922008&fvip=3&keepalive=yes&fexp=24007246&beids=24350017&c=WEB_EMBEDDED_PLAYER&txp=5532434&n=2fkamfnaDBno0g&sparams=expire%2Cei%2Cip%2Cid%2Citag%2Csource%2Crequiressl%2Cctier%2Cpfa%2Chightc%2Cspc%2Cvprv%2Csvpuc%2Cmime%2Cns%2Cgir%2Cclen%2Cdur%2Clmt&lsparams=mh%2Cmm%2Cmn%2Cms%2Cmv%2Cmvi%2Cpl%2Cinitcwndbps&lsig=AG3C_xAwRQIhAID20SfZoSP6Duxk7Iq8NYC2-A40aofJXaN7PPG-ui1dAiAmk3uBJ7g3BUybfaLWFxhU_-sKNbZFR-blbog3nhHoKA%3D%3D&alr=yes&sig=AOq0QJ8wRQIhAI-blidrJKnEIeIAxqKUK_If3nre_QDhgDOlYG1s-gOmAiBX1jYG27A7qkPpTeZaD9ajx-GTe9POjRIm9YB8bR6icg%3D%3D&cpn=0QUkyy_UjC66cWf4&cver=1.20230521.00.00&range=0-4116967&rn=19&rbuf=119284&pot=MmQoyCyVYnUzHyjwnTTYA4p8z64mo9nBNLfsV1wXHT3ipJkNLNbnb_147zXfn9zPK0FIlkmfEEVrBG6o4ETiA-9XfZHN9giWcoDeVuyIY4VYoFAi4bdpP-OijJwAmd0bg0slHQd0");
        }
    }
}