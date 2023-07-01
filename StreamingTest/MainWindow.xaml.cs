using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using NAudio.Wave;

namespace StreamingTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ChromiumWebBrowser browser;
        public MainWindow()
        {
            //CefSharp設定
            CefSharp.Wpf.CefSettings settings = new CefSharp.Wpf.CefSettings();
            settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 /CefSharp Browser" + Cef.CefSharpVersion;
            settings.Locale = "ja";
            settings.AcceptLanguageList = "ja,en-US;q=0.9,en;q=0.8";
            settings.CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache");
            settings.PersistSessionCookies = true;
            Cef.Initialize(settings);
        }
        public async void Loaded(object sender, RoutedEventArgs routedEventArgs)
        {
            browser = new ChromiumWebBrowser();

            Main.Children.Add(browser);
            //browser.LoadUrl("https://kakeru.app/7caa2d8ca65c9a77e66fb8031b441116");
            string html = "<html> <head> <script>";
            html += "var script = document.createElement( 'script' );script.src = \"//www.youtube.com/iframe_api\";var firstScript = document.getElementsByTagName( 'script' )[ 0 ];firstScript.parentNode.insertBefore( script , firstScript );";
            html += "var player;";
            html += "var loaded = false;";
            html += "function onYouTubeIframeAPIReady() {" +
                    "player = new YT.Player(" +
                    "'video'," +
                    "{videoId:\'poujQfl73Ok\'," +
                    "playerVars:{" +
                    "'controls': 0," +
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
            await browser.WaitForRenderIdleAsync();
            bool sw;
            do
            {
                 JavascriptResponse res = await browser.EvaluateScriptAsync("checkload();");
                 sw = (bool)res.Result; 
            }while (!sw);
            browser.GetBrowserHost().SendMouseClickEvent(100, 100, MouseButtonType.Left, false, 1, CefEventFlags.None);
            await Task.Delay(10);
            browser.GetBrowserHost().SendMouseClickEvent(100, 100, MouseButtonType.Left, true, 1, CefEventFlags.None);
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            browser.GetBrowserHost().SendMouseClickEvent(100, 100, MouseButtonType.Left, false, 1, CefEventFlags.None);
            await Task.Delay(10);
            browser.GetBrowserHost().SendMouseClickEvent(100, 100, MouseButtonType.Left, true, 1, CefEventFlags.None);
        }
    }
}