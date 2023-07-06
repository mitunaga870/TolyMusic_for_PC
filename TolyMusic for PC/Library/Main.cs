using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
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

    public ObservableCollection<Track> GetTracks(string id,ViewModel.TypeEnum id_type)
    {
        ObservableCollection<Track> result = new ObservableCollection<Track>();
        Collection<Dictionary<string,object>> db_tmp;
        Collection<MySqlParameter> param;
        switch (id_type)
        {
            case ViewModel.TypeEnum.All:
                //DBから取得
                db_tmp = DB.Read(
                    "select * from tracks t join location l on t.track_id= l.track_id left join album al on t.album_id = al.album_id left join track_artist ta on t.track_id = ta.track_id left join artist ar on ta.artist_id = ar.artist_id left join device d on l.device_id = d.device_id");
                result = Other.LibDictoTracks(db_tmp);
                break;
            case ViewModel.TypeEnum.Album:
                if (Regex.Match(id, @"no_.*").Success)
                {
                    foreach (var track in vm.Curt_Album.Tracks)
                        result.Add(track);
                }
                else
                {
                    param = new Collection<MySqlParameter>();
                    param.Add(new MySqlParameter("@album_id", id));
                    db_tmp = DB.Read(
                        "select distinct * from tracks t join location l on t.track_id= l.track_id left join album al on t.album_id = al.album_id left join track_artist ta on t.track_id = ta.track_id left join artist ar on ta.artist_id = ar.artist_id left join device d on l.device_id = d.device_id left join album_artist aa on al.album_id = aa.album_id where aa.album_id = @album_id",
                        param);
                    result = Other.LibDictoTracks(db_tmp);
                }
                break;
            case ViewModel.TypeEnum.Artist:
                param = new Collection<MySqlParameter>();
                param.Add(new MySqlParameter("@artist_id", id));
                db_tmp = DB.Read(
                    "select distinct * from tracks t join location l on t.track_id= l.track_id left join album al on t.album_id = al.album_id left join track_artist ta on t.track_id = ta.track_id left join artist ar on ta.artist_id = ar.artist_id left join device d on l.device_id = d.device_id where ar.artist_id = @artist_id"
                    ,param);
                result = Other.LibDictoTracks(db_tmp);
                break;
            default:
                throw new Exception("Invalid ViewModel.TypeEnum");
        }
        return result;
    }
    public ObservableCollection<Album> GetAlbums(string id,ViewModel.TypeEnum id_type)
    {
        Collection<Dictionary<string,object>> db_tmp;
        ObservableCollection<Album> result = new ObservableCollection<Album>();
        switch (id_type)
        {
            case ViewModel.TypeEnum.All:
                //DBから取得
                db_tmp = DB.Read("SELECT distinct * from album join album_artist aa on album.album_id = aa.album_id join artist a on aa.artist_id = a.artist_id");
                result = Other.LibDictoAlbums(db_tmp);
                break;
            case ViewModel.TypeEnum.Artist:
                //DBから取得
                var param = new Collection<MySqlParameter>();
                param.Add(new MySqlParameter("@artist_id", id));
                db_tmp = DB.Read("SELECT distinct * from album join album_artist aa on album.album_id = aa.album_id join artist a on aa.artist_id = a.artist_id where a.artist_id = @artist_id",param);
                result = Other.LibDictoAlbums(db_tmp);
                //アルバムを持たない曲を取得
                db_tmp = DB.Read(
                    "select distinct * from tracks t join location l on t.track_id= l.track_id left join album al on t.album_id = al.album_id left join track_artist ta on t.track_id = ta.track_id left join artist ar on ta.artist_id = ar.artist_id left join device d on l.device_id = d.device_id left join album_artist aa on al.album_id = aa.album_id where ar.artist_id = @artist_id and al.album_id is null"
                    ,param);
                if (db_tmp.Count > 0)
                {
                    var noAlbum = new Album();
                    noAlbum.Id = "no_" + id;
                    noAlbum.Title = "No Album";
                    foreach (var track in Other.LibDictoTracks(db_tmp))
                    {
                        noAlbum.Tracks.Add(track);
                    }

                    result.Add(noAlbum);
                }
                break;
            default:
                throw new Exception("Invalid ViewModel.TypeEnum");
        }
        return result;
    }
    public ObservableCollection<Artist> GetArtists(string id,ViewModel.TypeEnum id_type)
    {
        ObservableCollection<Artist> result = new ObservableCollection<Artist>();
        Collection<Dictionary<string,object>> db_tmp;
        switch (id_type)
        {
            case ViewModel.TypeEnum.All:
                //DBから取得
                db_tmp = DB.Read("SELECT distinct * from artist");
                result = Other.LibDictoArtists(db_tmp);
                break;
            default:
                throw new Exception("Invalid ViewModel.TypeEnum");
        }
        return result;
    }
}