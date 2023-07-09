using System;
using System.Diagnostics;
using System.Text;

namespace TolyMusic_for_PC;

public class Python
{
    private static Process proc;
    
    private static void Init(string path)
    {
        string venv = "C:\\Users\\mitu\\RiderProjects\\TolyMusic_for_PC\\venv";
        proc = new Process
        {
            StartInfo = new ProcessStartInfo(venv + "\\Scripts\\python.exe", path)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8
            }
        };
    }
    public static void Run(string path)
    {
        Init(path);
        proc.Start();
    }
    public static void Run(string path,string[] args)
    {
        foreach (var arg in args)
            path += " " + arg;
        
        Init(path);
        proc.Start();
    }

    public static string[] Get(string path)
    {
        Run(path);
        var stream = proc.StandardOutput;
        var err = proc.StandardError;
        var outline = stream.ReadToEnd();
        var errline = err.ReadToEnd();
        proc.WaitForExit();
        if (errline != "")
            throw new Exception(errline);
        var result = outline.Split(Convert.ToChar("\n"));
        proc.Close();
        return result;
    }
    public static string[] Get(string path,string[] args)
    {
        Run(path,args);
        var stream = proc.StandardOutput;
        var err = proc.StandardError;
        var outline = stream.ReadToEnd();
        var errline = err.ReadToEnd();
        proc.WaitForExit();
        if (errline != "")
            throw new Exception(errline);
        var result = outline.Split(Convert.ToChar("\n"));
        proc.Close();
        return result;
    }
}