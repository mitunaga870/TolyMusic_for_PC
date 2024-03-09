using System;
using CefSharp;
using CefSharp.Structs;

namespace TolyMusic_for_PC.Streaming.Handlar;

public class StreamingAudioHandler : IAudioHandler
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public bool GetAudioParameters(IWebBrowser chromiumWebBrowser, IBrowser browser, ref AudioParameters parameters)
    {
        throw new NotImplementedException();
    }

    public void OnAudioStreamStarted(IWebBrowser chromiumWebBrowser, IBrowser browser, AudioParameters parameters, int channels)
    {
        throw new NotImplementedException();
    }

    public void OnAudioStreamPacket(IWebBrowser chromiumWebBrowser, IBrowser browser, IntPtr data, int noOfFrames, long pts)
    {
        throw new NotImplementedException();
    }

    public void OnAudioStreamStopped(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
        throw new NotImplementedException();
    }

    public void OnAudioStreamError(IWebBrowser chromiumWebBrowser, IBrowser browser, string errorMessage)
    {
        throw new NotImplementedException();
    }
}