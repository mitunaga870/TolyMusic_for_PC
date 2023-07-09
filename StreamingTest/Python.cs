using System;
using System.Diagnostics;
using System.Text;

namespace TolyMusic_for_PC;

public class Python
{
    private static Process proc;
    
    private static void Init(string venv,string path)
    {
        proc = new Process
        {
            StartInfo = new ProcessStartInfo(venv + "\\Scripts\\python.exe", path)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                StandardOutputEncoding = Encoding.UTF8,
                RedirectStandardOutput = true,
            }
        };
    }
    public static void Run(string path)
    {
        string venv = "C:\\Users\\mitu\\RiderProjects\\TolyMusic_for_PC\\venv";
        Init(venv,path);
        proc.Start();
    }

    public static string[] Get(string path)
    {
        Run(path);
        var stream = proc.StandardOutput;
        var outline = stream.ReadToEnd();
        proc.WaitForExit();
        var result = outline.Split(Convert.ToChar("\n"));
        proc.Close();
        return result;
    }
}