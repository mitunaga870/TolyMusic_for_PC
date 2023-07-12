using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TolyMusic_for_PC.Library;

public class Main
{
    //private変数
    private ViewModel vm;
    private Player player;
    private Queue.Main queue;
    private Grid container;
    private StackPanel func_container;
    private AddLibFunc lib;
    //コンストラクタ
    public Main(ViewModel vm, Player player, Queue.Main queue, Grid container, StackPanel funcContainer)
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
            case ViewModel.TypeEnum.Playlist:
                param = new Collection<MySqlParameter>();
                param.Add(new MySqlParameter("@playlist_id", id));
                db_tmp = DB.Read("select distinct * from tracks t join location l on t.track_id = l.track_id left join album al on t.album_id = al.album_id left join track_artist ta on t.track_id = ta.track_id left join artist ar on ta.artist_id = ar.artist_id left join device d on l.device_id = d.device_id left join playlist_track pt on t.track_id = pt.track_id where pt.playlist_id = @playlist_id",param);
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
                db_tmp = DB.Read("SELECT distinct * from album join album_artist aa on album.album_id = aa.album_id left join artist a on aa.artist_id = a.artist_id where a.artist_id = @artist_id",param);
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

    public void Del_track(string track_id)
    {
        //値の確認
        if (String.IsNullOrEmpty(track_id))
            throw new Exception("Invalid Value");
        //パラメータ・クエリ作成
        var param = new Collection<MySqlParameter>();
        param.Add(new MySqlParameter("@track_id", track_id));
        var tracks_q = "delete from tracks where track_id = @track_id";
        var ta_q = "delete from track_artist where track_id = @track_id";
        var pa_q = "delete from playlist_track where track_id = @track_id";
        var location_q = "delete from location where track_id = @track_id";
        var history_q = "delete from history_tracks where track_id = @track_id";
        //BAN
        int i = 0;
        var res =DB.Read("select location,youtube_id,tois_id from location where track_id = @track_id", param);
        var ban_quary = "insert into ban_track (location,youtube_id) ";
        var ban_param = new Collection<MySqlParameter>();
        foreach (var dic in res)
        {
            ban_param.Add(new MySqlParameter("@location"+i, dic["location"]));
            switch (dic["location"])
            {
                case 1://youtube
                    ban_quary += "values (@location"+i+",@ban_id"+i+");";
                    ban_param.Add(new MySqlParameter("@ban_id"+i, dic["youtube_id"]));
                    break;
            }

            i++;
        }
        //実行
        DB.NonQuery(tracks_q, param);
        DB.NonQuery(ta_q, param);
        DB.NonQuery(pa_q, param);
        DB.NonQuery(location_q, param);
        DB.NonQuery(history_q, param);
        DB.NonQuery(ban_quary, ban_param);
    }

    public ObservableCollection<Playlist> GetPlaylists()
    {
        var result = new ObservableCollection<Playlist>();
        var dics = DB.Read("select * from playlist left join playlist_track pt on playlist.playlist_id = pt.playlist_id left join tracks t on pt.track_id = t.track_id");
        foreach (var dic in dics)
        {
            var playlist = new Playlist(dic);
            if (result.Count(p => p.Id == playlist.Id) > 0)
                playlist = result.First(p => p.Id == playlist.Id);
            else
                result.Add(playlist);
            playlist.AddTrack(new Track(dic));
        }
        return result;
    }

    public void Add_playlist(string playlistId, string Id, ViewModel.TypeEnum type)
    {
        Collection<MySqlParameter> param = new Collection<MySqlParameter>();
        param.Add(new MySqlParameter("@playlist_id", playlistId));
        string query = "insert into playlist_track (playlist_id,track_id) values ";
        switch (type)
        {
            case ViewModel.TypeEnum.Track:
                query += "(@playlist_id,@track_id)";
                param.Add(new MySqlParameter("@track_id", Id));
                break;
            default:
                var tracks = GetTracks(Id, type);
                int i = 0;
                foreach (var track in tracks)
                {
                    query += "(@playlist_id,@track_id" + i + "),";
                    param.Add(new MySqlParameter("@track_id"+i, track.Id));
                    i++;
                }
                query = query.Remove(query.Length - 1);
                break;
        }
        DB.NonQuery(query , param);
    }

    public void Make_playlist(string title, string Id, ViewModel.TypeEnum type)
    {
        string playlistId = Guid.NewGuid().ToString();
        var playlist_param = new Collection<MySqlParameter>();
        playlist_param.Add(new MySqlParameter("@playlist_id", playlistId));
        playlist_param.Add(new MySqlParameter("@playlist_title", title));
        
        string tp_query = "insert into playlist_track (playlist_id,track_id) values ";
        var tp_param = new Collection<MySqlParameter>();
        tp_param.Add(new MySqlParameter("@playlist_id", playlistId));
        switch (type)
        {
            case ViewModel.TypeEnum.Track:
                tp_query += "(@playlist_id,@track_id)";
                tp_param.Add(new MySqlParameter("@track_id", Id));
                break;
            default:
                var tracks = GetTracks(Id, type);
                int i = 0;
                foreach (var track in tracks)
                {
                    tp_query += "(@playlist_id,@track_id" + i + "),";
                    tp_param.Add(new MySqlParameter("@track_id"+i, track.Id));
                    i++;
                }
                tp_query = tp_query.Remove(tp_query.Length - 1);
                break;
        }
        
        DB.NonQuery("insert into playlist (playlist_id,playlist_title) values (@playlist_id,@playlist_title)", playlist_param);
        DB.NonQuery(tp_query , tp_param);
    }

    public void Del_playlist(string playlistId, string Id, ViewModel.TypeEnum type)
    {
        var query = "delete from playlist_track where playlist_id = @playlist_id";
        var param = new Collection<MySqlParameter>();
        param.Add(new MySqlParameter("@playlist_id", playlistId));
        switch (type)
        {
            case ViewModel.TypeEnum.Track:
                query += " and track_id = @track_id";
                param.Add(new MySqlParameter("@track_id", Id));
                break;
            default:
                var tracks = GetTracks(Id, type);
                int i = 0;
                foreach (var track in tracks)
                {
                    query += " and track_id = @track_id" + i;
                    param.Add(new MySqlParameter("@track_id"+i, track.Id));
                    i++;
                }
                break;
        }

        DB.NonQuery(query, param);
    }

    public void Update_playlist(string id, string titleText)
    {
        var param = new Collection<MySqlParameter>();
        param.Add(new MySqlParameter("@playlist_id", id));
        param.Add(new MySqlParameter("@playlist_title", titleText));
        DB.NonQuery("update playlist set playlist_title = @playlist_title where playlist_id = @playlist_id", param);
    }
    
    public void Del_playlist(string id)
    {
        var param = new Collection<MySqlParameter>();
        param.Add(new MySqlParameter("@playlist_id", id));
        DB.NonQuery("delete from playlist where playlist_id = @playlist_id", param);
        DB.NonQuery("delete from playlist_track where playlist_id = @playlist_id", param);
    }
}