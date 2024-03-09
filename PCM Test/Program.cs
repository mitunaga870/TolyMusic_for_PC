using System;
using System.IO;
using System.Linq;
using NAudio.Wave;

namespace PCM_Test
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            MemoryStream stream = new MemoryStream();
            FileStream fileStream = new FileStream("sin.raw", FileMode.Open);
            byte[] buffer = new byte[1024];
            int read;
            while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                stream.Write(buffer, 0, read);
            }
            fileStream.Close();

            
            stream.Position = 0;
            
            WaveFormat waveFormat = new WaveFormat(44100, 2);
            WaveStream waveStream = new RawSourceWaveStream(stream, waveFormat);
            WaveOut waveOut = new WaveOut();
            waveOut.Init(waveStream);
            waveOut.Play();
            
            Console.ReadKey();      
            
            stream.Close();
        }
    }
}