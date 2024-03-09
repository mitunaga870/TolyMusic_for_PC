using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using CefSharp;
using CefSharp.Wpf;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using MessageBox = System.Windows.MessageBox;
using MouseEventHandler = System.Windows.Input.MouseEventHandler;


namespace TolyMusic_for_PC
{
    public class Player
    {
        //変数宣言
        private enum Locaion
        {
            Youtube,
            Local,
        }

        private Locaion location;
        private readonly ViewModel vm;
        private bool isAsio;
        private AsioOut asio;
        private WasapiOut wasapi;
        public bool IsPlaying;
        private AudioFileReader afReader;
        private MediaFoundationReader mfReader;
        private Task timesetter;
        public bool Started;
        private readonly Grid container;
        private bool webLoaded;
        private ChromiumWebBrowser browser;
        private bool youtubeLoaded;
        private bool anti_end;
        private Task checkBuffering;
        private bool closing;
        private Task Ended_youtube;
        private Hotkey playpausehHotkey;
        private Hotkey nextHotkey;
        private Hotkey prevHotkey;
        private Dispatcher dispatcher;
        private readonly MemoryStream browserStream;

        public Player(Dispatcher dispatcher,ViewModel vm, Grid container)
        {
            this.dispatcher = dispatcher;
            this.vm = vm;
            this.container = container;
            webLoaded = false;
            isAsio = false;
        }

        //初期化処理
        public bool Init()
        {
            //ホットキー登録
            playpausehHotkey = new Hotkey(Keys.MediaPlayPause);
            playpausehHotkey.HotkeyPressed += (s, e) =>
            {
                if (IsPlaying)
                    Pause();
                else
                    Play();
            };
            nextHotkey = new Hotkey(Keys.MediaNextTrack);
            nextHotkey.HotkeyPressed += (s, e) =>
            {
                next();
            };
            prevHotkey = new Hotkey(Keys.MediaPreviousTrack);
            prevHotkey.HotkeyPressed += (s, e) =>
            {
                prev();
            };
            
            //ドライバ初期化
            try
            {
                anti_end = false;
                if (vm.Curt_track.location == 1)
                {
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
                }
                else if (vm.Excl && Properties.Settings.Default.EDisASIO) //排他・ASIO
                {
                    isAsio = true;
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
                    isAsio = false;
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
                    if (Properties.Settings.Default.EDcustumized)
                    {
                        wasapi = new WasapiOut(vm.Excl_Driver.mmdevice, AudioClientShareMode.Exclusive, false, 500);
                    }
                    else
                    {
                        wasapi = new WasapiOut(AudioClientShareMode.Exclusive, 100);
                    }

                    wasapi.PlaybackStopped += new EventHandler<StoppedEventArgs>(Ended);
                }
                else //共有WASAPI
                {
                    isAsio = false;
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
            catch (Exception e)
            {
                MessageBox.Show("初期化処理エラー：" + e.Message);
                return false;
            }
            
            //タスク初期化
            checkBuffering = new Task(()=>
            {
                CheckBuf();
            });
            timesetter = new Task(() =>
            {
                TimeControler();
            });
            Ended_youtube = new Task(() =>
            {
                Ended_Youtube();
                
            });
            
            return true;
        }

        //キュー開始処理
        public async Task Start()
        {
            if (!Init())
            {
                return;
            }

            //ファイル読み込み
            try
            {
                switch (vm.Curt_track.location)
                {
                    case 0: //localトラックの時
                        string path = Properties.Settings.Default.LocalDirectryPath.Replace('\\', '/') + "/" + vm.Curt_track.Path;
                        afReader = new AudioFileReader(path);
                        location = Locaion.Local;
                        break;
                    case 1: //youtubeトラックの
                        //読み込み開始
                        youtubeLoaded = false;

                        if (!webLoaded)
                        {
                            browser = new ChromiumWebBrowser();
                            
                            browser.PreviewMouseMove += new MouseEventHandler(((sender, args) =>
                            {
                                args.Handled = true;
                            }));
                            browser.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(((sender, args) =>
                            {
                                args.Handled = true;
                            }));
                            browser.PreviewMouseRightButtonDown += new MouseButtonEventHandler(((sender, args) =>
                            {
                                args.Handled = true;
                            }));
                            
                            

                            container.Children.Add(browser);
                            webLoaded = true;

                            string html = "<html> <head> <script>";
                            html +=
                                "var script = document.createElement( 'script' );script.src = \"//www.youtube.com/iframe_api\";var firstScript = document.getElementsByTagName( 'script' )[ 0 ];firstScript.parentNode.insertBefore( script , firstScript );";
                            html += "var player;";
                            html += "var loaded = false;";
                            html += "function onYouTubeIframeAPIReady() {" +
                                    "player = new YT.Player(" +
                                    "'video'," +
                                    "{videoId:\'" + vm.Curt_track.youtube_id + "\'," +
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
                            html += "function start(id) {" +
                                    "player.cueVideoById(id);}";
                            html += "function stop() {" +
                                    "player.stopVideo();" +
                                    "player.clearVideo();}";
                            html += "</script> </head>";
                            html += "<body> <div id=\"video\" style=\"width: 100%;height: 100%\"></div> </body>";
                            html += "</html>";
                            browser.LoadHtml(html, "http://example.com/");
                            await browser.WaitForInitialLoadAsync();
                            
                            //ロード後クリック
                            bool sw = false;
                            int count= 0;
                            
                            //ロード待ち
                            do
                            {
                                JavascriptResponse res;
                                try
                                {
                                    res = await browser.EvaluateScriptAsync("checkload();", TimeSpan.FromSeconds(1));
                                } catch (Exception e)
                                {
                                    continue;
                                }

                                if (res.Success)
                                    sw = (bool)res.Result;
                                
                                count++;
                                if (count > 10)
                                    throw new Exception("タイムアウトです。");

                                await Task.Delay(100);
                            }while (!sw);
                        }
                        else
                        {
                            await browser.EvaluateScriptAsync("start(\"" + vm.Curt_track.youtube_id + "\");");
                        }
                        //バッファリング解除
                        checkBuffering.Start();
                        //終了処理
                        Ended_youtube.Start();
                        location = Locaion.Youtube;
                        break;
                    default:
                        throw new Exception();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("読み込みに失敗しました。" + e.Message);
                Dispose();
                return;
            }
            //ボリューム・シークバー最大処理
            switch (location)
            {
                case Locaion.Local:
                    vm.Curt_length = afReader.TotalTime.Ticks;
                    try
                    {
                        if (!vm.Excl)
                            SetVol();
                        if (isAsio)
                        {
                            asio.Init(afReader);
                            asio.Play();
                        }
                        else
                        {
                            wasapi.Init(afReader);
                            wasapi.Play();
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        Dispose();
                        return;
                    }

                    break;
                case Locaion.Youtube:
                    SetVol();
                    break;                           
            }

            timesetter.Start();
            
            IsPlaying = true;
            Started = true;
        }

        //再生処理
        public void Play()
        {
            if (!Started)
            {
                return;
            }
            try
            {
                switch (location)
                {
                    case Locaion.Local:
                        if (isAsio)
                            asio.Play();
                        else
                            wasapi.Play();
                        break;
                    case Locaion.Youtube:
                        browser.ExecuteScriptAsync("play();");
                        break;
                }

                IsPlaying = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Dispose();
            }
        }

        //一時停止処理
        public void Pause()
        {
            try
            {
                switch (location)
                {
                    case Locaion.Local:
                        if (isAsio)
                            asio.Pause();
                        else
                            wasapi.Pause();
                        break;
                    case Locaion.Youtube:
                        browser.ExecuteScriptAsync("pause();");
                        break;
                }

                IsPlaying = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Dispose();
            }
        }

        //再生終了時の処理
        public void Ended(object obj, StoppedEventArgs e)
        {
            if (anti_end)
                return;
            //停止処理 
            IsPlaying = false;
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
            //連続再生の重複skipを防ぐ
            await Task.Delay(1000);

            //無限回くり
            while (true)
            {
                if (closing)
                    return;
                JavascriptResponse res;
                try {
                    res = await browser.EvaluateScriptAsync("getstate();");
                } catch (Exception e)
                {
                    if (closing)
                        return;
                    continue;
                }
                int state = Convert.ToInt32(res.Result);
                if (state == 0 && vm.Curt_time >= vm.Curt_length)
                {
                    //停止処理 
                    IsPlaying = false;
                    if (vm.Loop == null) //一曲ループ処理
                    {
                        vm.Next_time = 0;
                        Play();
                        continue;
                    }
                    else if (vm.PlayQueue.Count - 1 == vm.Curt_queue_num && !(bool)vm.Loop) //キューの最後の終了処理
                    {
                        Dispose();
                    }
                    else
                    {
                        dispatcher.Invoke(() =>
                        {
                            next();
                        });
                    }
                    return;
                }

                await Task.Delay(1000);
            }
        }

        public async Task next()
        {

            if(!Started)
                return;
            
            anti_end = true;
            if (vm.PlayQueue.Count - 1 == vm.Curt_queue_num) //キューのループ処理
            {
                vm.Curt_queue_num = 0;
                vm.Curt_track = vm.PlayQueue[0];
            }
            else
            {
                vm.Curt_queue_num++;
                vm.Curt_track = vm.PlayQueue[vm.Curt_queue_num];
            }

            Close();
            await Start();
        }

        //巻き戻し
        public void prev()
        {
            try
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
                        Close();
                        Start();
                    }
                }
                else
                {
                    vm.Next_time = 0;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Dispose();
            }
        }
        //再生位置の変更
        private async Task TimeControler()
        {
            try{
                switch (location)
                {
                    case Locaion.Local:
                        while (afReader != null)
                        {
                            try
                            {
                                if(IsPlaying)
                                    vm.Curt_time = afReader.CurrentTime.Ticks;
                                long time = vm.Next_time;
                                if (time >= 0)
                                {
                                    afReader.CurrentTime = TimeSpan.FromTicks(time);
                                    vm.Next_time = -1;
                                    await Task.Delay(10);
                                }
                            }
                            catch (ObjectDisposedException e)
                            {
                                return;
                            }
                            catch (NullReferenceException e)
                            {
                                return;
                            }

                            if (closing)
                                return;
                            
                        }

                        break;
                    case Locaion.Youtube:
                        JavascriptResponse res;
                        do
                        {
                            res = await browser.EvaluateScriptAsync("getduration();");
                        }while(res.Result == null || Convert.ToInt64(res.Result) == 0);
                        
                        vm.Curt_length = Convert.ToInt64(res.Result) * 1000;
                        while (true)
                        {
                            if (IsPlaying)
                            {
                                JavascriptResponse jsres = await browser.EvaluateScriptAsync("gettime();");
                                vm.Curt_time = (long)Math.Ceiling(Convert.ToDouble(jsres.Result)) * 1000;

                            }

                            if (vm.Next_time != -1)
                            {
                                browser.ExecuteScriptAsync("settime(" + vm.Next_time / 1000 + ");");
                                vm.Next_time = -1;
                                await Task.Delay(10);
                            }

                            if (closing)
                                    return;
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                if (closing)
                    return;
                MessageBox.Show("再生位置の変更に失敗しました。"+e.Message);
                Dispose();
            }
        }
        //Volume
        public void SetVol()
        {
            try {
                switch (location)
                {
                    case Locaion.Local:
                        if (Started)
                            afReader.Volume = (float)vm.Volume / 100;
                        break;
                    case Locaion.Youtube:
                        if (browser != null)
                            browser.ExecuteScriptAsync("setvol(" + vm.Volume + ");");
                        break;
                }
            } catch (Exception e)
            {
                MessageBox.Show("音量の変更に失敗しました。"+e.Message);
                Dispose();
            }
        }
        //ファイルを閉じる
        public void Close()
        {
            //再生停止処理
            IsPlaying = false;
            asio?.Stop();
            wasapi?.Stop();

            //Task 終了処理
            closing = true;
            if (timesetter != null && timesetter.Status != TaskStatus.Created)
            {
                timesetter.Wait();
                timesetter.Dispose();
            }
            if (checkBuffering != null && checkBuffering.Status != TaskStatus.Created)
            {
                checkBuffering.Wait();
                checkBuffering.Dispose();
            }
            if (Ended_youtube != null && Ended_youtube.Status != TaskStatus.Created)
            {
                Ended_youtube.Wait();
                Ended_youtube.Dispose();
            }
            closing = false;
            //ファイルを閉じる
            if (afReader != null)
                afReader.Close();
            if (mfReader != null)
                mfReader.Close();
            
            //Youtubeを閉じる
            if (browser != null)
                browser.ExecuteScriptAsync("stop();");
        }
        //終了処理
        public void Dispose()
        {
            Close();
            
            vm.DeleteQueue();
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

            if (afReader != null)
                afReader.Dispose();
            if (mfReader != null)
                mfReader.Dispose();
            
            browser?.ExecuteScriptAsync("stop();");
            
            vm.Curt_track = null;
            IsPlaying = false;
            Started = false;
            
            //ホットキーの削除
            playpausehHotkey?.Dispose();
            playpausehHotkey = null;
            nextHotkey?.Dispose();
            nextHotkey = null;
            prevHotkey?.Dispose();
            prevHotkey = null;
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
                    continue;
                }

                //クリック前だったら・   
                if (state == 5 || state == -1 || (state == 1 && !IsPlaying) || (state == 2 && IsPlaying))
                {
                    browser.GetBrowserHost()
                        .SendMouseClickEvent(100, 100, MouseButtonType.Left, false, 1, CefEventFlags.None);
                    await Task.Delay(10);
                    browser.GetBrowserHost()
                        .SendMouseClickEvent(100, 100, MouseButtonType.Left, true, 1, CefEventFlags.None);
                    await Task.Delay(900);
                }

                await Task.Delay(10);
                if (closing)
                    return;
            }
        }
    }
}