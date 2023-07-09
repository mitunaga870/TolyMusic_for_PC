using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TolyMusic_for_PC.Library;

namespace TolyMusic_for_PC.Streaming;

public class Yt_Func
{
    private ViewModel vm;
    private Player player;
    private Queue queue;
    private Grid container;
    private AddLibFunc lib;
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
        string script = "scripts\\Youtube\\Get_Lib_Song.py";
        //スクリプト結果をjson配列に変換
        string[] sc_res_ary = Python.Get(script);
        //スクリプト結果をリストに格納
        Collection<Track> yt_lib = new Collection<Track>();
        Collection<Album> yt_album = new Collection<Album>();
        Collection<Artist> yt_artist = new Collection<Artist>();
        foreach (string sc_res_line in sc_res_ary)
        {
            if(sc_res_line == "")
                continue;
            //予約後置き換え・Jsonに変換
            JObject sc_res_json = Other.JsonParse(sc_res_line);
            //tracks
            yt_lib.Add(new Track(sc_res_json));
            //albums
            if (yt_album.Count(a => a.Id == (string)sc_res_json["album"]["id"]) == 0)
                yt_album.Add(new Album(sc_res_json));
            foreach (var artist in sc_res_json["artists"])
                if(yt_artist.Count(a => a.Id == (string)artist["id"]) == 0)
                    yt_artist.Add(new Artist((JObject)artist));
        }
        //ライブラリに送信
        lib.AddYtmusic(yt_lib, yt_album, yt_artist);
    }

    public void Add_PlayingTrack(object sender, RoutedEventArgs e)
    {
        Add_Track(vm.Curt_YoutubeId);
    }

    public void Add_Track(string id)
    {
        string[] res;
        try
        {
            res = Python.Get("scripts\\Youtube\\GetTrackData.py", new[] {id});
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            return;
        }
        JObject TrackData = Other.JsonParse(res[0]);
        Track track = new Track(TrackData);
        var albums = new Collection<Album>();
        if (TrackData.ContainsKey("album"))
            albums.Add(new Album(TrackData));
        var artists = new Collection<Artist>();
        foreach (var artist in TrackData["artists"])
            artists.Add(new Artist((JObject)artist));
        //ライブラリに送信
        lib.AddYtmusic(new Collection<Track> {track}, albums, artists);
    }
}