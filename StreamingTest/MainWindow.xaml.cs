using System.Windows;
using CefSharp.Wpf;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using TolyMusic_for_PC;

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
            Python.Get("sc/test.py");
        }
    }
}