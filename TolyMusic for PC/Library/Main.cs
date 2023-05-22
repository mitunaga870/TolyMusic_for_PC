using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TolyMusic_for_PC.Library;

public class Main
{
    //private変数
    private ViewModel vm;
    private Player player;
    private Queue queue;
    private Grid container;
    private StackPanel func_container;
    private AddLibFunc lib;
    //コンストラクタ
    public Main(ViewModel vm, Player player, Queue queue, Grid container, StackPanel funcContainer)
    {
        this.vm = vm;
        this.player = player;
        this.queue = queue;
        this.container = container;
        this.func_container = funcContainer;
        lib = new AddLibFunc(vm);
    }
    //初期化
    public void Init()
    {
        //YoutubePlaylistの同期
        string script = "scripts\\Youtube\\Get_Playlist_Track.py";
        string youtube_playlist_id = Properties.Settings.Default.YoutubePlaylist;
        //スクリプトプロセス宣言
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo("py")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                Arguments = script + " " + youtube_playlist_id
            }
        };
        //スクリプト実行・取得
        proc.Start();
        var stream = proc.StandardOutput;
        //スクリプト終了
        string sc_res = stream.ReadToEnd();
        proc.WaitForExit();
        proc.Close();
        //スクリプト結果を配列に変換
        List<string> sc_res_ary = sc_res.Split(Convert.ToChar("\n")).ToList();
        //スクリプト結果をリストに格納
        string yt_playlist_name = sc_res_ary[0];
        sc_res_ary.RemoveAt(0);
        System.Collections.ObjectModel.Collection<Track> yt_lib = new System.Collections.ObjectModel.Collection<Track>();
        System.Collections.ObjectModel.Collection<Album> yt_album = new System.Collections.ObjectModel.Collection<Album>();
        System.Collections.ObjectModel.Collection<Artist> yt_artist = new System.Collections.ObjectModel.Collection<Artist>();
        foreach (string sc_res_line in sc_res_ary)
        {
            if (sc_res_line == "")
                continue;
            //予約後置き換え・Jsonに変換
            string sc_res_line_after = sc_res_line.Replace("True", "\"true\"");
            sc_res_line_after = sc_res_line_after.Replace("False", "\"false\"");
            sc_res_line_after = sc_res_line_after.Replace("None", "\"none\"");
            var sc_res_json = JObject.Parse(sc_res_line_after);
            //tracks
            yt_lib.Add(new Track(sc_res_json));
            //albums
            if (sc_res_json["album"].ToString()!="none"&&yt_album.Count(a => a.Id == (string)sc_res_json["album"]["id"]) == 0)
                yt_album.Add(new Album((JObject)sc_res_json));
            //artists
            foreach (var artist in sc_res_json["artists"])
                if(yt_artist.Count(a => a.Id == (string)artist["id"]) == 0)
                    yt_artist.Add(new Artist((JObject)artist));
        }
        //ライブラリに追加
        lib.AddYtmusic(yt_lib, yt_album, yt_artist);
    }

    public ObservableCollection<Track> GetTracks()
    {
        //DBから取得
        var db_tmp = DB.Read("select * from tracks joid location on tracks.location_id = location.id");
        ObservableCollection<Track> tracks = new ObservableCollection<Track>();
        foreach (var db_row in db_tmp)
        {
            //重複削除
            var track = new Track(db_row);
            if (tracks.Count(t => t.Id == track.Id) == 0)
                tracks.Add(track);
            else
            {
                
            }
        }
        return tracks;
    }
}