using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TolyMusic_for_PC.Library;

namespace TolyMusic_for_PC.Streaming;

public class Yt_Func
{
    private ViewModel vm;
    private Player player;
    private Queue queue;
    private Grid container;
    private Library.AddLibFunc lib;
    public Yt_Func(ViewModel vm, Player player, Queue queue, Grid container)
    {
        this.vm = vm;
        this.player = player;
        this.queue = queue;
        this.container = container;
        lib = new AddLibFunc(vm);
    }
    //ライブラリ同期ボタン
    public void SyncLib(object sender, RoutedEventArgs e)
    {
        //ライブラリ取得スクリプト
        //スクリプト用変数取得
        string script = "script\\Youtube\\Get_Lib_Song.py";
        //スクリプトプロセス宣言
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo("python.exe")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                Arguments = script 
            }
        };
        //スクリプト実行・取得
        proc.Start();
        var stream = proc.StandardOutput;
        //スクリプト終了
        var sc_res = stream.ReadToEnd();
        proc.WaitForExit();
        proc.Close();
        //スクリプト結果をjson配列に変換
        string[] sc_res_ary = sc_res.Split(Convert.ToChar("\n"));
        //スクリプト結果をリストに格納
        System.Collections.ObjectModel.Collection<Track> yt_lib = new System.Collections.ObjectModel.Collection<Track>();
        System.Collections.ObjectModel.Collection<Album> yt_album = new System.Collections.ObjectModel.Collection<Album>();
        System.Collections.ObjectModel.Collection<Artist> yt_artist = new System.Collections.ObjectModel.Collection<Artist>();
        foreach (string sc_res_line in sc_res_ary)
        {
            if(sc_res_line == "")
                continue;
            //予約後置き換え・Jsonに変換
            string sc_res_line_after = sc_res_line.Replace("True", "\"true\"");
            sc_res_line_after = sc_res_line_after.Replace("False", "\"false\"");
            sc_res_line_after = sc_res_line_after.Replace("None", "\"none\"");
            var sc_res_json = JObject.Parse(sc_res_line_after);
            //tracks
            yt_lib.Add(new Track(sc_res_json));
            //albums
            if (yt_album.Count(a => a.Id == (string)sc_res_json["album"]["id"]) == 0)
                yt_album.Add(new Album((JObject)sc_res_json));
            foreach (var artist in sc_res_json["artists"])
                if(yt_artist.Count(a => a.Id == (string)artist["id"]) == 0)
                    yt_artist.Add(new Artist((JObject)artist));
        }
        //ライブラリに送信
        lib.AddYtmusic(yt_lib, yt_album, yt_artist);
    }
}