using System.IO;
using CefSharp.Wpf;
using NAudio.Wave;
using NAudio.Wasapi.CoreAudioApi;

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
            InitializeComponent();
            
            
            browser = new ChromiumWebBrowser("https://www.youtube.com/embed/Ov94vYTgvlQ?autoplay=1");
            
            GetStreamingHandler handler = new GetStreamingHandler();
            browser.AudioHandler = handler;
            
            Content = browser;
        }
    }
}