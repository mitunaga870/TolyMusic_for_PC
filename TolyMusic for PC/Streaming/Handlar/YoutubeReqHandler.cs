using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Windows;
using CefSharp;
using NAudio.Wave;

namespace TolyMusic_for_PC.Streaming.Handlar;

public class YoutubeReqHandler : IRequestHandler
{
    AudioFileReader afreader;
    //コンストラクタ
    public YoutubeReqHandler(){}
    public YoutubeReqHandler(AudioFileReader afreader)
    {
        this.afreader = afreader;
    }

    public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture,
        bool isRedirect)
    {
            return false;
    }

    public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
        return;
    }

    public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl,
        WindowOpenDisposition targetDisposition, bool userGesture)
    {
        return true;
    }

    public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
        IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
    {
        if(Regex.Match(request.Url,".*playback.*").Success)
            MessageBox.Show(request.Url);
        return null;
    }

    public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host,
        int port, string realm, string scheme, IAuthCallback callback)
    {
        return true;
    }

    public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl,
        ISslInfo sslInfo, IRequestCallback callback)
    {
        return true;
    }

    public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port,
        X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
    {
        return false;
    }

    public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
        return;
    }

    public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status)
    {
        return;
    }
}