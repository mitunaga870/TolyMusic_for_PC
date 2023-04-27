using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using MySqlConnector;

namespace TolyMusic_for_PC.Library
{
    public class Main
    {
        ViewModel vm;
        DB lib;
        private Local.Main local;
        public Main(ViewModel vm)
        {
            this.vm = vm;
            lib = new DB();
            local = new Local.Main();
        }
        //ライブラリにリスト上の曲を追加
        public void AddListTracks(Collection<Track> tracks)
        {
            //マシン名の取得
            if (!Properties.Settings.Default.LibraryAddedMachine)
            {
                if(!AddMachine())
                    return;
            }
            //localのアーティストリストを取得
            Collection<Artist> localArtists = local.GetArtists();
            //既存DB情報取得
            //既存トラック確認用
            Collection<Dictionary<string,object>> tmp = lib.Read("select path,device_id from location where location = 0");
            Collection<Dictionary<string,string>> addedlocation = new Collection<Dictionary<string,string>>();
            foreach (var item in tmp)
            {
                addedlocation.Add(new Dictionary<string, string>()
                {
                    {"path",item["path"].ToString()},
                    {"device_id",item["device_id"].ToString()}
                });
            }
            //既存アーティスト確認用
            tmp = lib.Read("select * from artist");
            Collection<Artist> addedartist = new Collection<Artist>();
            foreach (var item in tmp)
            {
                addedartist.Add(new Artist(item));
            }
            //artist重複確認
            //新規アーティストリスト
            Collection<Artist> newArtists = new Collection<Artist>();//新規アーティストリスト
            Collection<Artist> addArtists = new Collection<Artist>();//追加アーティストリスト
            Collection<Artist> oldArtists = new Collection<Artist>();//既存アーティストリスト
            foreach (var artist in localArtists)
            {
                if (addedartist.Count(a => a.Name == artist.Name) == 0)//新規アーティスト
                    newArtists.Add(artist);
                else
                {
                    oldArtists.Add(artist);
                    string tmpId;
                    tmpId = addedartist.Where(a => a.Name == artist.Name).ToArray()[0].Id;
                    artist.Id = tmpId; 
                }
            }
            //album重複確認
            tmp = lib.Read("select * from album");
            Collection<Album> addedalbum = new Collection<Album>();//仮想ライブラリアルバムリスト
            foreach (var item in tmp)
            {
                addedalbum.Add(new Album(item));
            }
            Collection<Album> localAlbums = local.GetAlbums();
            Collection<Album> addAlbums = new Collection<Album>();//追加アルバムリスト
            Collection<Album> newAlbums = new Collection<Album>();//新規アルバムリスト
            foreach (var album in localAlbums)
            {
                if(addedalbum.Count(a => a.Title == album.Title) == 0)
                    newAlbums.Add(album);
                else
                {
                    string tmpId;
                    tmpId = addedalbum.Where(a => a.Title == album.Title).ToArray()[0].Id;
                    album.Id = tmpId;
                }
            }
            //ベースクエリ作成
            //track
            string track_quary = "INSERT INTO tracks (track_id,track_title,track_title_pron,composer_id,group_id,track_num,duration,location,device_id,path) VALUES ";
            Collection<MySqlParameter> track_parameters = new Collection<MySqlParameter>();
            //location
            string location_quary = "INSERT INTO location (track_id,location,machine_name,path) VALUES ";
            Collection<MySqlParameter> location_parameters = new Collection<MySqlParameter>();
            //track_artist
            string track_artist_quary = "INSERT INTO track_artist (track_id,artist_id) VALUES ";
            Collection<MySqlParameter> track_artist_parameters = new Collection<MySqlParameter>();
            Uri cwd = new Uri(Properties.Settings.Default.LocalDirectryPath);
            //album_aritst
            string album_artist_quary = "INSERT INTO album_artist (album_id,artist_id) VALUES ";
            Collection<MySqlParameter> album_artist_parameters = new Collection<MySqlParameter>();
            //各トラックに応じてループによるクエリ作成
            for (int i = 0; i < tracks.Count; i++)
            {
                //トラック重複確認
                if (addedlocation.Count(x => x["path"] == tracks[i].Path) != 0)
                    continue;
                //track_artist
                foreach (var artist in tracks[i].Artists)
                {
                    track_artist_quary += "(@track_id" + i + ",@artist_id" + i + "),";
                    track_artist_parameters.Add(new MySqlParameter("@track_id" + i, tracks[i].Id));
                    track_artist_parameters.Add(new MySqlParameter("@artist_id" + i, artist.Id));
                    //新規は追加リストに追加
                    if (newArtists.Count(a => a.Name == artist.Name) != 0)
                        addArtists.Add(artist);
                }
                //track
                track_quary += "(@track_id" + i + ",@track_title" + i + ",@track_title_pron" + i + ",@album_id" + i + ",@composer_id" + i + ",@group_id" + i + ",@track_num" + i + ",@duration" + i + ",@location" + i + ",@device_id" + i + ",@path" + i + "),";
                track_parameters.Add(new MySqlParameter("@track_id" + i, tracks[i].Id));
                track_parameters.Add(new MySqlParameter("@track_title"+i, tracks[i].Title));
                track_parameters.Add(new MySqlParameter("@track_title_pron"+i, tracks[i].Title_pron));
                //album_id
                string album_id = tracks[i].Album_id;
                string album_title = localAlbums.Where(a => a.Id == album_id).ToArray()[0].Title;
                track_parameters.Add(new MySqlParameter("@album_id"+i, album_id));
                //新記事は追加リストに追加
                if (newAlbums.Count(a => a.Title == album_title) != 0)
                    addAlbums.Add(newAlbums.Where(a => a.Id == album_id).ToArray()[0]);
                //composer_id
                string composer_id = tracks[i].Composer_id;
                string composer_name = localArtists.Where(a => a.Id == composer_id).ToArray()[0].Name;
                if (newArtists.Count(a => a.Name == composer_name) != 0)
                    addArtists.Add(newArtists.Where(a => a.Id == composer_id).ToArray()[0]);
                track_parameters.Add(new MySqlParameter("@composer_id"+i, composer_id));
                //group_id
                string group_id = tracks[i].Group_id;
                string group_name = localArtists.Where(a=>a.Id == group_id).ToArray()[0].Name;
                //新規は追加リストに追加
                if(newArtists.Count(a => a.Name == group_name) != 0)
                    addArtists.Add(newArtists.Where(a => a.Id == group_id).ToArray()[0]);
                track_parameters.Add(new MySqlParameter("@group_id" + i, group_id));
                track_parameters.Add(new MySqlParameter("@track_num" + i, tracks[i].TrackNumber));
                track_parameters.Add(new MySqlParameter("@duration" + i, tracks[i].Duration));
                //location
                location_quary += "(@track_id" + i + ",@location" + i + ",@machine_name" + i + ",@path" + i + "),";
                location_parameters.Add(new MySqlParameter("@track_id" + i, tracks[i].Id));
                location_parameters.Add(new MySqlParameter("@location" + i, 0));
                location_parameters.Add(new MySqlParameter("@machine_name" + i, Environment.MachineName));
                //パスを相対パスに変換
                string postpath;
                Uri path = new Uri(tracks[i].Path);
                Uri postpathuri = cwd.MakeRelativeUri(path);
                postpath = postpathuri.ToString();
                location_parameters.Add(new MySqlParameter("@path" + i, postpath));
            }
            //artist
            string artist_quary = "INSERT INTO artist (artist_id,artist_name,artist_name_pron) VALUES ";
            Collection<MySqlParameter> artist_parameters = new Collection<MySqlParameter>();
            //newArtistsによるクエリ作成
            foreach (var artist in addArtists)
            {
                artist_quary += "(@artist_id,@artist_name,@artist_name_pron),";
                artist_parameters.Add(new MySqlParameter("@artist_id", artist.Id));
                artist_parameters.Add(new MySqlParameter("@artist_name", artist.Name));
                artist_parameters.Add(new MySqlParameter("@artist_name_pron", artist.Name_pron));
            }
            //最後のカンマを削除
            track_quary = track_quary.Remove(track_quary.Length - 1);
            location_quary = location_quary.Remove(location_quary.Length - 1);
            track_artist_quary = track_artist_quary.Remove(track_artist_quary.Length - 1);
            artist_quary = artist_quary.Remove(artist_quary.Length - 1);
            //クエリ実行
            lib.NonQuery(track_quary, track_parameters);
            lib.NonQuery(location_quary, location_parameters);
            lib.NonQuery(track_artist_quary, track_artist_parameters);
            lib.NonQuery(artist_quary, artist_parameters);
        }
        //Device設定
        private bool AddMachine()//成功時true
        {
            Collection<MySqlParameter> parameters = new Collection<MySqlParameter>();
            parameters[0] = new MySqlParameter("@device_id", Environment.MachineName);
            Collection<Dictionary<string,object>> res = lib.Read("select * from devices where device_id = @device_id",parameters);
            if(res.Count == 0)
            {
                lib.NonQuery("insert into devices (device_id) values (@device_id)", parameters);
                Properties.Settings.Default.LibraryAddedMachine = true;
                Properties.Settings.Default.Save();
                return true;
            }
            MessageBox.Show("このマシン名はすでに登録されています。\n設定画面から変更してください。");
            return false;
        }
        //リザルトをトラックに変更
        private Collection<Track> ResultToTrack(Collection<Dictionary<string,object>> result)
        {
            Collection<Track> tracks = new Collection<Track>();
            foreach (Dictionary<string, object> item in result)
            {
                tracks.Add(new Track(item));
            }
            return tracks;
        }
    }
}