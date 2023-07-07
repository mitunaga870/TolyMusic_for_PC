using System.Windows;
using CefSharp.Wpf;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

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
        }
        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            string key = "AIzaSyBIIm4tnKmwsb3rbZO66GZUleqtAERBY5w";
            YouTubeService service = new YouTubeService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                ApiKey = key,
                ApplicationName = "TolyMusic"
            });
            var req = service.Videos.List("snippet");
            req.Id = "8BQqO_1nl24";
            VideoListResponse res = await req.ExecuteAsync();
            var item = res.Items[0];
            title.Content = item.Snippet.Title;
            artist.Content = item.Snippet.ChannelTitle;
            album.Content = item.Snippet.Description;
            composer.Content = item.Snippet.ChannelId;
        }
    }
}