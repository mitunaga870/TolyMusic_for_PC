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
            Collection<Artist> newArtists = new Collection<Artist>();
            Collection<Artist> addArtists = new Collection<Artist>();
            foreach (var artist in localArtists)
            {
                if (addedartist.Count(a => a.Name == artist.Name) == 0)
                {
                    newArtists.Add(artist);
                    addedartist.Add(artist);
                }
            }
            //ベースクエリ作成
            //track
            string track_quary = "INSERT INTO tracks (track_id,track_title,track_title_pron,composer_id,group_id,track_num,duration,location,device_id,path) VALUES ";
            Collection<MySqlParameter> track_parameters = new Collection<MySqlParameter>();
            //location
            string location_quary = "INSERT INTO location (track_id,location,machine_name,path) VALUES ";
            Collection<MySqlParameter> location_parameters = new Collection<MySqlParameter>();
            Uri cwd = new Uri(Properties.Settings.Default.LocalDirectryPath);
            for (int i = 0; i < tracks.Count; i++)
            {
                //トラック重複確認
                if (addedlocation.Count(x => x["path"] == tracks[i].Path) != 0)
                    continue;
                //track
                track_quary += "(@track_id" + i + ",@track_title" + i + ",@track_title_pron" + i + ",@composer_id" + i + ",@group_id" + i + ",@track_num" + i + ",@duration" + i + ",@location" + i + ",@device_id" + i + ",@path" + i + "),";
                track_parameters.Add(new MySqlParameter("@track_id" + i, tracks[i].Id));
                track_parameters.Add(new MySqlParameter("@track_title"+i, tracks[i].Title));
                track_parameters.Add(new MySqlParameter("@track_title_pron"+i, tracks[i].Title_pron));
                string composer_id = addedartist.Where(a => a.Name == tracks[i].Composer_id).ToArray()[0].Id;
                track_parameters.Add(new MySqlParameter("@composer_id"+i, composer_id));
                string group_id = addedartist.Where(a => a.Name == tracks[i].Group_id).ToArray()[0].Id;
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
            //最後のカンマを削除
            track_quary = track_quary.Remove(track_quary.Length - 1);
            //クエリ実行
            lib.NonQuery(track_quary, track_parameters);
            lib.NonQuery(location_quary, location_parameters);
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