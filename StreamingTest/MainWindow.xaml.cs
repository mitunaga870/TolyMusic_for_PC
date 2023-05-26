using System;
using System.IO;
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
        public void Loaded(object sender, RoutedEventArgs routedEventArgs)
        {
            /*
            //browser
            var browser = new ChromiumWebBrowser();
            browser.RequestHandler = new YoutubeReqHandler(); 
            browser.Address = "https://www.youtube.com/watch?v=XjfpkGSy8Q4";
            Main.Children.Add(browser);
            */
            AudioFileReader reader = new AudioFileReader("test.weba");
            BufferedWaveProvider provider = new BufferedWaveProvider(reader.WaveFormat);
        }
    }
}