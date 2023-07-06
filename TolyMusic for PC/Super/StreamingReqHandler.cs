using System.Security.Cryptography.X509Certificates;
using System.Windows;
using CefSharp;

namespace TolyMusic_for_PC.Super
{
    public class StreamingReqHandler : IRequestHandler
    {
        protected ViewModel vm;
        //コンストラクタ
        public StreamingReqHandler(ViewModel vm)
        {
            this.vm = vm;
        }
        //義務実装部
        //繊維ページのリクエストをハンドル
        public virtual bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture,
            bool isRedirect)
        {
            return false;
        }
        public virtual void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public virtual bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl,
            WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return true;
        }
        //リソース読み込み時
        public virtual IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
            IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return null;
        }

        public virtual bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host,
            int port, string realm, string scheme, IAuthCallback callback)
        {
            return true;
        }

        public virtual bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl,
            ISslInfo sslInfo, IRequestCallback callback)
        {
            return true;
        }

        public virtual bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port,
            X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            return false;
        }

        public virtual void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public virtual void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status)
        {
        }
    }
}