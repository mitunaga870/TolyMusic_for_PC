using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using CefSharp;
using CefSharp.Handler;
using CefSharp.Structs;
using NAudio.Wave;

namespace StreamingTest;

public class GetStreamingHandler : AudioHandler
{
    private int position;
    BufferedWaveProvider bufferedWaveProvider;
    private int channels;
    private MemoryStream stream;

    protected override bool GetAudioParameters(IWebBrowser chromiumWebBrowser, IBrowser browser, ref AudioParameters parameters)
    {
        return true;
    }

    protected override void OnAudioStreamStarted(IWebBrowser chromiumWebBrowser, IBrowser browser, AudioParameters parameters, int channels)
    {
        position = 0;
        this.channels = channels;
        
        stream = new MemoryStream();
        
        WaveFormat waveFormat = new WaveFormat(parameters.SampleRate, 32, channels);
        bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
        bufferedWaveProvider.DiscardOnBufferOverflow = true;

        WaveOut waveOut = new WaveOut();
        waveOut.Init(bufferedWaveProvider);
        //waveOut.Play();

        base.OnAudioStreamStarted(chromiumWebBrowser, browser, parameters, channels);
    }

    protected override void OnAudioStreamPacket(IWebBrowser chromiumWebBrowser, IBrowser browser, IntPtr data, int noOfFrames, long pts)
    {
        var sizeOfData = noOfFrames * channels * 4;
        
        byte[] buffer = new byte[sizeOfData];
        
        for (int i = 0; i < sizeOfData-4; i+= 4)
        {
            buffer[i] = Marshal.ReadByte(data, i + 3);
            buffer[i + 1] = Marshal.ReadByte(data, i + 2);
            buffer[i + 2] = Marshal.ReadByte(data, i + 1);
            buffer[i + 3] = Marshal.ReadByte(data, i);
        }
        
        Marshal.Copy(data, buffer, 0, sizeOfData);
        MessageBox.Show(buffer.ToString());
        
        bufferedWaveProvider.AddSamples(buffer, 0, sizeOfData);
        stream.Write(buffer, 0, sizeOfData);
        
        base.OnAudioStreamPacket(chromiumWebBrowser, browser, data, noOfFrames, pts);
    }

    protected override void OnAudioStreamError(IWebBrowser chromiumWebBrowser, IBrowser browser, string errorMessage)
    {
        MessageBox.Show("Audio stream error!");
        base.OnAudioStreamError(chromiumWebBrowser, browser, errorMessage);
    }

    protected override void OnAudioStreamStopped(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
        stream.Position = 0;
        FileStream fileStream = new FileStream("stream.raw", FileMode.Create);
        stream.CopyTo(fileStream);
        fileStream.Close();
        stream.Close();
        base.OnAudioStreamStopped(chromiumWebBrowser, browser);
    }
}