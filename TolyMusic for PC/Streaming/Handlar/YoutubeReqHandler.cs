using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Windows;
using CefSharp;
using CefSharp.Handler;
using NAudio.Wave;
using TolyMusic_for_PC.Library;
using TolyMusic_for_PC.Super;

namespace TolyMusic_for_PC.Streaming.Handlar;

public class YoutubeReqHandler : StreamingReqHandler
{
    private AddLibFunc lib;
    public YoutubeReqHandler(ViewModel vm) : base(vm)
    {
        lib = new AddLibFunc(vm);
    }

    //再生id取得
    public override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
        IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
    {
        string url = request.Url; 
        if (Regex.Match(url, @".*docid=.*").Success)
        {
            string id = Regex.Match(url, @"docid=.{11}").Value.Substring(6);
            vm.Curt_YoutubeId = id;
            return null;
        }
        return null;
    }
    //楽曲ページに移動時
    public override bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl,
        WindowOpenDisposition targetDisposition, bool userGesture)
    {
        if(Regex.Match(targetUrl,".*youtube\\.com/watch.*").Success)
        {
            string id = Regex.Match(targetUrl, @"v=.{11}").Value.Substring(2);
            lib.AddYtmusic(id);
            return true;
        }
        return base.OnOpenUrlFromTab(chromiumWebBrowser, browser, frame, targetUrl, targetDisposition, userGesture);
    }
}