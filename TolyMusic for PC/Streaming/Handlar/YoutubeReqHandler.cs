using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Windows;
using CefSharp;
using CefSharp.Handler;
using NAudio.Wave;
using TolyMusic_for_PC.Super;

namespace TolyMusic_for_PC.Streaming.Handlar;

public class YoutubeReqHandler : StreamingReqHandler
{
    public YoutubeReqHandler(ViewModel vm) : base(vm)
    {
    }

    
    public override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
        IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
    {
        string url = request.Url; 
        if (Regex.Match(url, @".*docid=.*").Success)
        {
            MessageBox.Show("再生阻止");
            return null;
        }
        return null;
    }
}