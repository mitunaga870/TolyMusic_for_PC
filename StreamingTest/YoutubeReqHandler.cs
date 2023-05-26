using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Windows;
using CefSharp;
using NAudio.Wave;

namespace StreamingTest;

public class YoutubeReqHandler : IRequestHandler
{
    //コンストラクタ
    public YoutubeReqHandler(){}

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
        if (Regex.Match(request.Url, ".*audio.*").Success && Regex.Match(request.Url, ".*playback.*").Success)
        {
            Regex replace = new Regex("(.+range=)[0-9]+-([0-9]+)(.+)");
            string tmp1 = replace.Replace(request.Url, "$1");
            string length = replace.Replace(request.Url, "$2");
            string tmp2 =replace.Replace(request.Url, "$3");
            string url = tmp1 + "0-" + length + tmp2;
            url = "https://rr1---sn-xgmnpoxuopp-ioqe.googlevideo.com/videoplayback?expire=1685003770&ei=msluZPmAIbOW1d8P1tWdgAM&ip=210.137.32.50&id=o-ADqOj3DD7TVmeeNL8QpzNvcv5G5AE28681qlCIfj36q1&itag=251&source=youtube&requiressl=yes&mh=GJ&mm=31%2C29&mn=sn-xgmnpoxuopp-ioqe%2Csn-ogueln67&ms=au%2Crdu&mv=m&mvi=1&pl=19&ctier=A&pfa=5&initcwndbps=1735000&hightc=yes&spc=qEK7B_MCNFvNO2ENnA4mmABiFDyOrqAu7p6PRH0WMhHKMlHCu_of39I&vprv=1&svpuc=1&mime=audio%2Fwebm&ns=g4GMoVZdDi3pUZDsfnvLT8kN&gir=yes&clen=4116968&dur=255.021&lmt=1684262764585279&mt=1684981984&fvip=3&keepalive=yes&fexp=24007246&beids=24350017&c=WEB_EMBEDDED_PLAYER&txp=5532434&n=FmCXX3DJ5CWHjA&sparams=expire%2Cei%2Cip%2Cid%2Citag%2Csource%2Crequiressl%2Cctier%2Cpfa%2Chightc%2Cspc%2Cvprv%2Csvpuc%2Cmime%2Cns%2Cgir%2Cclen%2Cdur%2Clmt&lsparams=mh%2Cmm%2Cmn%2Cms%2Cmv%2Cmvi%2Cpl%2Cinitcwndbps&lsig=AG3C_xAwRQIgGc1l-y2v-frHGW01zEUZmFh63dxwiNg4b6lsblcfcQYCIQDXsXuC5_w6TKwhRmsJYKbFAOIoOTj_vWdmH9MbvLBI8A%3D%3D&alr=yes&sig=AOq0QJ8wRQIhALF7D9aIEaEuZCkQRERUve6HU5ynwjOWqQYfdsKKsy0TAiBd0NmBHBUu7oq25dfsmqCZjWgy7DRMcAqL2ofW17AIEQ%3D%3D&cpn=U0Hu22GrUNaIE9zd&cver=1.20230521.00.00&range=0-2797451&rn=14&rbuf=77440&pot=MmTmJBwaQiAv5eYci0ImRqC3X_4OdxwiGLhc_lZ2X_GN8uSPZ61sVL8g5UFsElVMAHiXFL7rrM6y6TQ6RPi2Qf0FT1FTRmbKuL6Qqkrqil6m84f2L4dbjpwd7aMOK3Vil8rguNCp";
            AudioFileReader tmpreader;
            try
            {
                tmpreader = new AudioFileReader(url);
            }
            catch (Exception e)
            {
                return null;
            }
            WaveFormat format = tmpreader.WaveFormat;
            float[] buffer = new float[tmpreader.Length];
            
            tmpreader.Read(buffer, 10, buffer.Length);
            MessageBox.Show(buffer[10].ToString());
        }
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