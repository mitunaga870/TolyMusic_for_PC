using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace TolyMusic_for_PC
{
    public class Player
    {
        //変数宣言
        private enum Locaion
        {
            youtube,
            local,
        }

        private Locaion locaiton;
        ViewModel vm;
        private bool isASIO;
        public AsioOut asio;
        public WasapiOut wasapi;
        public bool isPlaying;
        private AudioFileReader afreader;
        private MediaFoundationReader mfreader;
        private Task timesetter;
        public bool started;
        private WebClient webClient;
        private Grid container;
        private bool webloaded;
        private ChromiumWebBrowser browser;
        private bool youtube_loaded;
        private bool anti_end;
        private Task checkbuf;
        private bool initializing;
        private Task Ended_youtube;

        public Player(ViewModel vm, Grid container)
        {
            this.vm = vm;
            this.container = container;
            webloaded = false;
            isASIO = false;
            webClient = new WebClient();
            timesetter = new Task(TimeControler);
            checkbuf = new Task(CheckBuf);
            Ended_youtube = new Task(Ended_Youtube);
        }

        //初期化処理
        public void Init()
        {
            initializing = true;
            anti_end = false;
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

            //Browser初期化
            if (browser != null)
                browser.LoadHtml("<html><head></head><body></body></html>", "http://example.com/");

            //タスクの破棄確認
            while (true)
            {
                var res = checkbuf.Status;
                if (res == TaskStatus.RanToCompletion)
                {
                    checkbuf.Dispose();
                    checkbuf = new Task(CheckBuf);
                }

                var res2 = timesetter.Status;
                if (res2 == TaskStatus.RanToCompletion)
                {
                    timesetter.Dispose();
                    timesetter = new Task(TimeControler);
                }

                var res3 = Ended_youtube.Status;
                if (res3 == TaskStatus.RanToCompletion)
                {
                    Ended_youtube.Dispose();
                    Ended_youtube = new Task(Ended_Youtube);
                }

                if (res != TaskStatus.Running && res2 != TaskStatus.Running && res3 != TaskStatus.Running)
                    break;
            }

            initializing = false;
        }

        //キュー開始処理
        public async void Start()
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
                    case 1: //youtubeトラックの
                        //読み込み開始
                        youtube_loaded = false;
                        
                        if (!webloaded)
                        {
                            browser = new ChromiumWebBrowser();
                            container.Children.Add(browser);
                            webloaded = true;
                        }

                        string html = "<html> <head> <script>";
                        html += "var script = document.createElement( 'script' );script.src = \"//www.youtube.com/iframe_api\";var firstScript = document.getElementsByTagName( 'script' )[ 0 ];firstScript.parentNode.insertBefore( script , firstScript );";
                        html += "var player;";
                        html += "var loaded = false;";
                        html += "function onYouTubeIframeAPIReady() {" +
                                "player = new YT.Player(" +
                                "'video'," +
                                "{videoId:\'"+vm.Curt_track.youtube_id+"\'," +
                                "playerVars:{" +
                                "controls:0," +
                                "}," +
                                "events:{'onReady':onPlayerReady}});}";
                        html += "function onPlayerReady(event) {loaded= true;}";
                        html += "function play() {player.playVideo();}";
                        html += "function pause() {player.pauseVideo();}";
                        html += "function checkload() {return loaded;}";
                        html += "function settime(time) {" +
                                "if(time!=-1)" +
                                "player.seekTo(time);}";
                        html += "function gettime() {return player.getCurrentTime();}";
                        html += "function setvol(vol) {player.setVolume(vol);}";
                        html += "function getvol() {return player.getVolume();}";
                        html += "function getduration() {return player.getDuration();}";
                        html += "function getstate() {return player.getPlayerState();}";
                        html += "</script> </head>";
                        html += "<body> <div id=\"video\" style=\"width: 100%;height: 100%\"></div> </body>";
                        html += "</html>";
                        browser.LoadHtml(html, "http://example.com/");
                        //ロード後クリック
                        await browser.WaitForRenderIdleAsync();
                        bool sw;
                        do
                        {
                            var res = await browser.EvaluateScriptAsync("checkload();");
                            sw = (bool)res.Result;
                        }while (!sw);
                        //バッファリング解除
                        checkbuf.Start();
                        //終了処理
                        Ended_youtube.Start();
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
            //ボリューム・シークバー最大処理
            switch (locaiton)
            {
                case Locaion.local:
                    vm.Curt_length = afreader.TotalTime.Ticks;
                    if (!vm.Excl)
                        SetVol();
                    if (isASIO)
                        asio.Init(afreader);
                    else
                        wasapi.Init(afreader);
                    break;
                case Locaion.youtube:
                    SetVol();
                    JavascriptResponse res = await browser.EvaluateScriptAsync("getduration();");
                    
                    vm.Curt_length = (long)Convert.ToInt64(res.Result)*1000;
                    break;                           
            }

            if(locaiton == Locaion.local)
                Play();
            timesetter.Start();
            isPlaying = true;
        }

        //再生処理
        public void Play()
                              {
                              switch (locaiton)
                              {
                              case Locaion.local:
                              if (isASIO)
                              asio.Play();
                              else
                              wasapi.Play();
                              break;
                              case Locaion.youtube:
                              browser.ExecuteScriptAsync("play();");
                              break;
                              }
                              isPlaying = true;
                              }

        //一時停止処理
        public void Pause()
                               {
                               switch (locaiton)
                               {
                               case Locaion.local:
                               if (isASIO)
                               asio.Pause();
                               else
                               wasapi.Pause();
                               break;
                               case Locaion.youtube:
                               browser.ExecuteScriptAsync("pause();");
                               break;
                               }
                               isPlaying = false;
                               }

        //再生終了時の処理
        public void Ended(object obj, StoppedEventArgs e)
                                                             {
                                                             if(anti_end)
                                                             return;
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
        //Youtubeの終了処理
        private async void Ended_Youtube()
        {
            while (true)
            {
                if(initializing)
                    return;
                var res = await browser.EvaluateScriptAsync("getstate();");
                int state = Convert.ToInt32(res.Result);
                if (state == 0)
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
                     return;
                }
                await Task.Delay(100);
            }
        }
        public void next()
                              {
                              anti_end = true;
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
        private async void TimeControler()
        {
            switch (locaiton)
            {
                case Locaion.local:
                    while (afreader != null)
                    {
                        try
                        {
                            if(isPlaying)
                                vm.Curt_time = afreader.CurrentTime.Ticks;
                            if (vm.Next_time != -1)
                            {
                                afreader.CurrentTime = TimeSpan.FromTicks(vm.Next_time);
                                vm.Next_time = -1;
                            }
                        }
                        catch (ObjectDisposedException e)
                        {
                            return;
                        }

                        if (initializing)
                            return;
                    }

                    break;
                case Locaion.youtube:
                    while (true)
                    {
                        if (isPlaying)
                        {
                            JavascriptResponse jsres = await browser.EvaluateScriptAsync("gettime();");
                            vm.Curt_time = Convert.ToInt64(jsres.Result) * 1000;
                        }

                        if (vm.Next_time != -1)
                        {
                            browser.ExecuteScriptAsync("settime(" + vm.Next_time/1000 + ");");
                            vm.Next_time = -1;
                            await Task.Delay(10);
                        }
                        if (initializing)
                            return;
                    }
                    break;
            }
        }
        //Volume
        public void SetVol()
        {
            switch (locaiton)
            {
                case Locaion.local:
                    if (started)
                        afreader.Volume = (float)vm.Volume / 100;
                    break;
                case Locaion.youtube:
                    if (browser != null)
                        browser.ExecuteScriptAsync("setvol(" + vm.Volume + ");");
                    break;
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
        //checkbuf(Youtube自動再生エラー処理)
        private async void CheckBuf()
        {
            for (int i = 0; i < 200; i++)
            {
                int state;
                try
                {
                    var res = await browser.EvaluateScriptAsync("getstate();");
                    state = (int)res.Result;
                }
                catch
                {
                    return;
                }

                if (state == 5 || state == -1)
                {
                    browser.GetBrowserHost()
                        .SendMouseClickEvent(100, 100, MouseButtonType.Left, false, 1, CefEventFlags.None);
                    await Task.Delay(10);
                    browser.GetBrowserHost()
                        .SendMouseClickEvent(100, 100, MouseButtonType.Left, true, 1, CefEventFlags.None);
                }

                await Task.Delay(50);
                if (initializing)
                    return;
            }
        }
    }
}