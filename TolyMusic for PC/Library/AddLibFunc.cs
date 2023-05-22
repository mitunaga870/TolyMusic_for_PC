using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using MySql.Data.MySqlClient;

namespace TolyMusic_for_PC.Library
{
    public class AddLibFunc
    {
        ViewModel vm;
        private Local.Main local;
        public AddLibFunc(ViewModel vm)
        {
            this.vm = vm;
            local = new Local.Main();
        }
        //local
        //トラックリストからライブラリに追加
        public void AddLocalListTracks(Collection<Track> tracks)
        {
            //マシン名の取得
            string device_id = Properties.Settings.Default.MachineID;
            if (!Properties.Settings.Default.LibraryAddedMachine)
            {
                device_id = AddMachine();
                if(device_id == null)
                    return;
            }
            //localのアーティストリストを取得
            Collection<Artist> localArtists = local.GetArtists();
            //既存DB情報取得
            //既存トラック確認用
            Collection<Dictionary<string, object>> tmp = DB.Read("select * from tracks");
            Collection<Track> addedtracks = new Collection<Track>();
            foreach (var item in tmp)
                addedtracks.Add(new Track(item));
            //既存ロケーション確認用
            tmp = DB.Read("select path,device_id from location where location = 0");
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
            tmp = DB.Read("select * from artist");
            Collection<Artist> addedartist = new Collection<Artist>();
            foreach (var item in tmp)
            {
                addedartist.Add(new Artist(item));
            }
            //artist重複確認
            //新規アーティストリスト
            Collection<Artist> newArtists = new Collection<Artist>();//新規アーティストリスト
            Collection<Artist> addArtists = new Collection<Artist>();//追加アーティストリスト
            Collection<Artist> swpArtists = new Collection<Artist>();//重複アーティストリスト
            Dictionary<string,string> swpArtistId = new Dictionary<string, string>();//重複アーティストIDリスト
            foreach (var artist in localArtists)
            {
                if (addedartist.Count(a => a.Name == artist.Name) == 0)//新規アーティスト
                    newArtists.Add(artist);
                else
                {
                    swpArtists.Add(artist);
                    swpArtistId.Add(artist.Id,addedartist.Where(a => a.Name == artist.Name).ToArray()[0].Id);
                }
            }
            //album重複確認
            tmp = DB.Read("select * from album");
            Collection<Album> addedalbum = new Collection<Album>();//仮想ライブラリアルバムリスト
            foreach (var item in tmp)
            {
                addedalbum.Add(new Album(item));
            }
            Collection<Album> localAlbums = local.GetAlbums();
            Collection<Album> addAlbums = new Collection<Album>();//追加アルバムリスト
            Collection<Album> newAlbums = new Collection<Album>();//新規アルバムリスト
            Collection<Album> swpAlbums = new Collection<Album>();//重複アルバムリスト
            Dictionary<string,string> swpAlbumId = new Dictionary<string, string>();//重複アルバムIDリスト
            foreach (var album in localAlbums)
            {
                if(addedalbum.Count(a => a.Title == album.Title) == 0)
                    newAlbums.Add(album);
                else
                {
                    swpAlbums.Add(album);
                    swpAlbumId.Add(album.Id,addedalbum.Where(a => a.Title == album.Title).ToArray()[0].Id);
                }
            }
            //ベースクエリ作成
            //track
            string track_quary = "INSERT INTO tracks (track_id,track_title,track_title_pron,album_id,composer_id,group_id,track_num,duration) VALUES ";
            Collection<MySqlParameter> track_parameters = new Collection<MySqlParameter>();
            //location
            string location_quary = "INSERT INTO location (track_id,location,device_id,path) VALUES ";
            Collection<MySqlParameter> location_parameters = new Collection<MySqlParameter>();
            //track_artist
            string track_artist_quary = "INSERT INTO track_artist (track_id,artist_id) VALUES ";
            Collection<MySqlParameter> track_artist_parameters = new Collection<MySqlParameter>();
            Uri cwd = new Uri(Properties.Settings.Default.LocalDirectryPath);
            //各トラックに応じてループによるクエリ作成
            int i,k=0,l=0;
            for (i = 0; i < tracks.Count; i++)
            {
                //トラック重複確認
                bool continue_switch = false;
                foreach (var location in addedlocation)
                {
                    //データベースのpathをローカルパスに変換
                    string tmp_path = cwd.AbsolutePath + "/" + location["path"];
                    tmp_path = tmp_path.Replace("/","\\");
                    tmp_path = Uri.UnescapeDataString(tmp_path);
                    if (tracks[i].Path == tmp_path)
                    {
                        //locationに追加済みのときはスキップしこのデバイス出ない場合はこのデバイスを追加
                        if(location["device_id"] != device_id)
                        {
                            //locationのみの追加
                            location_quary += "(@track_id" + l + ",@location" + l + ",@device_id" + l + ",@path" + l + "),";
                            location_parameters.Add(new MySqlParameter("@track_id" + l, tracks[i].Id));
                            location_parameters.Add(new MySqlParameter("@location" + l, 0));
                            location_parameters.Add(new MySqlParameter("@device_id" + l, device_id));
                            location_parameters.Add(new MySqlParameter("@path" + l, tmp_path));
                        }
                        continue_switch = true;
                        break;
                    }
                }
                if(continue_switch)
                    continue;
                //track情報のみの追加ではないか確認
                if (addedtracks.Count(track => track.Title == tracks[i].Title) == 0)
                {
                    //track_artist
                    foreach (var artist in tracks[i].Artists)
                    {
                        track_artist_quary += "(@track_id" + k + ",@artist_id" + k + "),";
                        track_artist_parameters.Add(new MySqlParameter("@track_id" + k, tracks[i].Id));
                        //IDの置き換え
                        string artistId = artist.Id;
                        if (swpArtists.Count(a => a.Id == artist.Id) != 0)
                            artistId = swpArtistId[artist.Id];
                        track_artist_parameters.Add(new MySqlParameter("@artist_id" + k, artistId));
                        //新規は追加リストに追加
                        if (newArtists.Count(a => a.Name == artist.Name) != 0)
                        {
                            addArtists.Add(artist);
                            newArtists.Remove(newArtists.Where(a => a.Name == artist.Name).ToArray()[0]);
                        }
                        k++;
                    }

                    //track
                    track_quary += "(@track_id" + i + ",@track_title" + i + ",@track_title_pron" + i + ",@album_id" +
                                   i + ",@composer_id" + i + ",@group_id" + i + ",@track_num" + i + ",@duration" + i +
                                   "),";
                    track_parameters.Add(new MySqlParameter("@track_id" + i, tracks[i].Id));
                    track_parameters.Add(new MySqlParameter("@track_title" + i, tracks[i].Title));
                    track_parameters.Add(new MySqlParameter("@track_title_pron" + i, tracks[i].Title_pron));
                    //album_id
                    string album_id = tracks[i].Album_id;
                    string album_title = localAlbums.Where(a => a.Id == album_id).ToArray()[0].Title;
                    //IDの置き換え
                    if (swpAlbums.Count(a => a.Id == album_id) != 0)
                        album_id = swpAlbumId[album_id];
                    track_parameters.Add(new MySqlParameter("@album_id" + i, album_id));
                    //新記事は追加リストに追加
                    if (newAlbums.Count(a => a.Title == album_title) != 0)
                    {
                        addAlbums.Add(newAlbums.Where(a => a.Id == album_id).ToArray()[0]);
                        newAlbums.Remove(newAlbums.Where(a => a.Title == album_title).ToArray()[0]);
                    }

                    //composer_id
                    string composer_id = tracks[i].Composer_id;
                    if (composer_id != null)
                    {
                        string composer_name = localArtists.Where(a => a.Id == composer_id).ToArray()[0].Name;
                        //IDの置き換え
                        if (swpArtists.Count(a => a.Id == composer_id) != 0)
                            composer_id = swpArtistId[composer_id];
                        //新規は追加リストに追加
                        if (newArtists.Count(a => a.Name == composer_name) != 0)
                        {
                            addArtists.Add(newArtists.Where(a => a.Id == composer_id).ToArray()[0]);
                            newArtists.Remove(newArtists.Where(a => a.Name == composer_name).ToArray()[0]);
                        }
                    }

                    track_parameters.Add(new MySqlParameter("@composer_id" + i, composer_id));
                    //group_id
                    string group_id = tracks[i].Group_id;
                    if (group_id != null)
                    {
                        string group_name = localArtists.Where(a => a.Id == group_id).ToArray()[0].Name;
                        //IDの置き換え
                        if (swpArtists.Count(a => a.Id == group_id) != 0)
                            group_id = swpArtistId[group_id];
                        //新規は追加リストに追加
                        if (newArtists.Count(a => a.Name == group_name) != 0)
                        {
                            addArtists.Add(newArtists.Where(a => a.Id == group_id).ToArray()[0]);
                            newArtists.Remove(newArtists.Where(a => a.Name == group_name).ToArray()[0]);
                        }
                    }

                    track_parameters.Add(new MySqlParameter("@group_id" + i, group_id));
                    track_parameters.Add(new MySqlParameter("@track_num" + i, tracks[i].TrackNumber));
                    track_parameters.Add(new MySqlParameter("@duration" + i, tracks[i].Duration));
                }
                //location
                location_quary += "(@track_id" + l + ",@location" + l + ",@device_id" + l + ",@path" + l + "),";
                location_parameters.Add(new MySqlParameter("@track_id" + l, tracks[i].Id));
                location_parameters.Add(new MySqlParameter("@location" + l, "0"));
                location_parameters.Add(new MySqlParameter("@device_id" + l, device_id));
                //パスを相対パスに変換
                string postpath;
                Uri path = new Uri(tracks[i].Path);
                Uri postpathuri = cwd.MakeRelativeUri(path);
                postpath = postpathuri.ToString();
                postpath = postpath.Replace(cwd.Segments[cwd.Segments.Length - 1] + "/", "");
                location_parameters.Add(new MySqlParameter("@path" + l, postpath));
                l++;
            }
            //artist
            string artist_quary = "INSERT INTO artist (artist_id,artist_name,artist_name_pron) VALUES ";
            Collection<MySqlParameter> artist_parameters = new Collection<MySqlParameter>();
            //addArtistsによるクエリ作成
            i = 0;
            foreach (var artist in addArtists)
            {
                artist_quary += "(@artist_id"+i+",@artist_name"+i+",@artist_name_pron"+i+"),";
                artist_parameters.Add(new MySqlParameter("@artist_id"+i, artist.Id));
                artist_parameters.Add(new MySqlParameter("@artist_name"+i, artist.Name));
                artist_parameters.Add(new MySqlParameter("@artist_name_pron"+i, artist.Name_pron));
                i++;
            }
            //album
            string album_quary = "INSERT INTO album (album_id,album_title,album_title_pron) VALUES ";
            Collection<MySqlParameter> album_parameters = new Collection<MySqlParameter>();
            //album_aritst
            string album_artist_quary = "INSERT INTO album_artist (album_id,artist_id) VALUES ";
            Collection<MySqlParameter> album_artist_parameters = new Collection<MySqlParameter>();
            //addAlbumsによるクエリ作成
            i = 0;
            int j = 0;
            foreach (var album in addAlbums)
            {
                //album
                album_quary += "(@album_id"+i+",@album_title"+i+",@album_title_pron"+i+"),";
                album_parameters.Add(new MySqlParameter("@album_id"+i, album.Id));
                album_parameters.Add(new MySqlParameter("@album_title"+i, album.Title));
                album_parameters.Add(new MySqlParameter("@album_title_pron"+i, album.TitlePron));
                //album_artist
                foreach (var artist in album.Artists)
                {
                    album_artist_quary += "(@album_id"+j+",@artist_id"+j+"),";
                    album_artist_parameters.Add(new MySqlParameter("@album_id"+j, album.Id));
                    //アーティストIDの置き換え
                    string artist_id = artist.Id;
                    if (swpArtists.Count(a => a.Id == artist_id) != 0)
                        artist_id = swpArtistId[artist_id];
                    album_artist_parameters.Add(new MySqlParameter("@artist_id"+j, artist_id));
                    j++;
                }
                i++;
            }
            //最後のカンマを削除
            track_quary = track_quary.Remove(track_quary.Length - 1);
            location_quary = location_quary.Remove(location_quary.Length - 1);
            track_artist_quary = track_artist_quary.Remove(track_artist_quary.Length - 1);
            artist_quary = artist_quary.Remove(artist_quary.Length - 1);
            album_quary = album_quary.Remove(album_quary.Length - 1);
            album_artist_quary = album_artist_quary.Remove(album_artist_quary.Length - 1);
            //クエリ実行
            if (track_parameters.Count > 0)
                DB.NonQuery(track_quary, track_parameters);
            if (location_parameters.Count > 0)
                DB.NonQuery(location_quary, location_parameters);
            if (track_artist_parameters.Count > 0)
                DB.NonQuery(track_artist_quary, track_artist_parameters);
            if (artist_parameters.Count > 0)
                DB.NonQuery(artist_quary, artist_parameters);
            if (album_parameters.Count > 0)
                DB.NonQuery(album_quary, album_parameters);
            if (album_artist_parameters.Count > 0)
                DB.NonQuery(album_artist_quary, album_artist_parameters);
            if(track_parameters.Count+location_parameters.Count+track_artist_parameters.Count+artist_parameters.Count+album_parameters.Count+album_artist_parameters.Count > 0)
                MessageBox.Show("データベースに曲を追加しました。");
            else
                MessageBox.Show("データベースに曲を追加する必要はありませんでした。");
        }
        
        //Ytmusic
        //追加
        public void AddYtmusic(Collection<Track> tracks, Collection<Album> albums, Collection<Artist> artists)
        {
            //既存DB情報取得
            //既存トラック確認用
            Collection<Dictionary<string,object>> tmp = DB.Read("select * from tracks");
            Collection<Track> addedtrack = new Collection<Track>();
            foreach (var item in tmp)
                addedtrack.Add(new Track(item));
            //既存ロケーション
            tmp = DB.Read("select track_id,youtube_id from location");
            Collection<Dictionary<string,string>> addedlocation = new Collection<Dictionary<string,string>>();
            foreach (var item in tmp)
                addedlocation.Add(new Dictionary<string, string>(){{"track_id",item["track_id"].ToString()},{"youtube_id",item["youtube_id"].ToString()}});
            //既存アーティスト確認用
            tmp = DB.Read("select * from artist");
            Collection<Artist> addedartist = new Collection<Artist>();
            foreach (var item in tmp)
                addedartist.Add(new Artist(item));
            //album重複確認
            tmp = DB.Read("select * from album");
            Collection<Album> addedalbum = new Collection<Album>();//仮想ライブラリアルバムリスト
            foreach (var item in tmp)
                addedalbum.Add(new Album(item));
            
            //Artist
            //youtubeid_artistid変換用
            Dictionary<string, string> swpArtistId = new Dictionary<string, string>();
            //ベースクエリ
            string artist_quary = "INSERT INTO artist (artist_id,artist_name) VALUES ";
            //パラメータ
            Collection<MySqlParameter> artist_parameters = new Collection<MySqlParameter>();
            int i;
            for (i = 0; i < artists.Count; i++)
            {
                //追加済み確認変数
                bool added = addedartist.Count(a => a.Name == artists[i].Name) != 0;
                //ID置き換え
                string artist_id;
                if (added)//追加済みのとき
                    artist_id = addedartist.Where(a => a.Name == artists[i].Name).ToArray()[0].Id;
                else //新規アーティストのとき
                    artist_id = Guid.NewGuid().ToString();
                swpArtistId.Add(artists[i].Id, artist_id);
                artists[i].Id = artist_id;
                //追加済みのときにはスキップ
                if (added)
                    continue;
                //クエリ追加
                artist_quary += "(@artist_id"+i+",@artist_name"+i+"),";
                //パラメータ追加
                artist_parameters.Add(new MySqlParameter("@artist_id"+i, artists[i].Id));
                artist_parameters.Add(new MySqlParameter("@artist_name"+i, artists[i].Name));
            }
            //最後の一文字を削除
            artist_quary = artist_quary.Remove(artist_quary.Length - 1);

            //Album・Album_Artist
            //youtubeid_albumid変換用
            Dictionary<string, string> swpAlbumId = new Dictionary<string, string>();
            //ベースクエリ
            string album_quary = "INSERT INTO album (album_id,album_title) VALUES ";
            string album_artist_quary = "INSERT INTO album_artist (album_id,artist_id) VALUES ";
            //パラメータ
            Collection<MySqlParameter> album_parameters = new Collection<MySqlParameter>();
            Collection<MySqlParameter> album_artist_parameters = new Collection<MySqlParameter>();
            int j = 0;
            for (i = 0; i < albums.Count; i++)
            {
                //追加済み確認変数
                bool added = addedalbum.Count(a => a.Title == albums[i].Title) != 0;
                //album
                //ID置き換え
                string album_id;
                if (added)//追加済みのとき
                    album_id = addedalbum.Where(a => a.Title == albums[i].Title).ToArray()[0].Id;
                else //新規アルバムのとき
                    album_id = Guid.NewGuid().ToString();
                swpAlbumId.Add(albums[i].Id, album_id);
                albums[i].Id = album_id;
                //追加済みのときにはスキップ
                if (added)
                    continue;
                //クエリ追加
                album_quary += "(@album_id"+i+",@album_title"+i+"),";
                //パラメータ追加
                album_parameters.Add(new MySqlParameter("@album_id"+i, albums[i].Id));
                album_parameters.Add(new MySqlParameter("@album_title"+i, albums[i].Title));
                //album_artist
                foreach (var artist in albums[i].Artists)
                {
                    //クエリ追加
                    album_artist_quary += "(@album_id"+j+",@artist_id"+j+"),";
                    //artist_id置き換え
                    string artist_id = swpArtistId[artist.Id];
                    //パラメータ追加
                    album_artist_parameters.Add(new MySqlParameter("@album_id"+j, albums[i].Id));
                    album_artist_parameters.Add(new MySqlParameter("@artist_id"+j, artist_id));
                    j++;
                }
            }
            //最後の一文字を削除
            album_quary = album_quary.Remove(album_quary.Length - 1);
            album_artist_quary = album_artist_quary.Remove(album_artist_quary.Length - 1);

            //track・location・track_artist
            //ベースクエリ作成
            string track_quary = "INSERT INTO tracks (track_id,track_title,album_id,duration) VALUES ";
            string location_quary = "INSERT INTO location (track_id,location,youtube_id,priority) VALUES ";
            string track_artist_quary = "INSERT INTO track_artist (track_id,artist_id) VALUES ";
            //パラメータ
            Collection<MySqlParameter> track_parameters = new Collection<MySqlParameter>();
            Collection<MySqlParameter> location_parameters = new Collection<MySqlParameter>();
            Collection<MySqlParameter> track_artist_parameters = new Collection<MySqlParameter>();
            j = 0;
            for (i = 0; i < tracks.Count; i++)
            {
                //追加済み確認変数
                bool location_added = addedlocation.Count(at => at["youtube_id"] == tracks[i].Id) != 0;
                bool track_added = addedtrack.Count(t => t.Title == tracks[i].Title) != 0;
                string track_id;
                //track
                if(location_added)
                    continue;
                if (track_added)
                    //trackid取得
                    track_id = addedtrack.Where(t => t.Title == tracks[i].Title).ToArray()[0].Id;
                else
                {
                    //trackid作成
                    track_id = Guid.NewGuid().ToString();
                    //tracks
                    //クエリ追加
                    track_quary += "(@track_id" + i + ",@track_title" + i + ",@album_id" + i + ",@duration" + i + "),";
                    //パラメータ追加
                    track_parameters.Add(new MySqlParameter("@track_id" + i, track_id));
                    track_parameters.Add(new MySqlParameter("@track_title" + i, tracks[i].Title));
                    string album_id = tracks[i].Album_id;
                    if (album_id != null)
                        album_id = swpAlbumId[tracks[i].Album_id];
                    track_parameters.Add(new MySqlParameter("@album_id" + i, album_id));
                    track_parameters.Add(new MySqlParameter("@duration" + i, tracks[i].Duration));
                    //track_artist
                    foreach (var artist in tracks[i].Artists)
                    {
                        //クエリ追加
                        track_artist_quary += "(@track_id" + j + ",@artist_id" + j + "),";
                        //artist_id置き換え
                        string artist_id = swpArtistId[artist.Id];
                        //パラメータ追加
                        track_artist_parameters.Add(new MySqlParameter("@track_id" + j, track_id));
                        track_artist_parameters.Add(new MySqlParameter("@artist_id" + j, artist_id));
                        j++;
                    }
                }
                //location
                //クエリ追加
                location_quary += "(@track_id" + i + ",1,@youtube_id" + i + ",1),";
                //パラメータ追加
                location_parameters.Add(new MySqlParameter("@track_id" + i, track_id));
                location_parameters.Add(new MySqlParameter("@youtube_id" + i, tracks[i].Id));
            }
            //最後の一文字を削除
            track_quary = track_quary.Remove(track_quary.Length - 1);
            location_quary = location_quary.Remove(location_quary.Length - 1);
            track_artist_quary = track_artist_quary.Remove(track_artist_quary.Length - 1);
            
            //クエリ実行
            if(album_parameters.Count != 0)
                DB.NonQuery(album_quary, album_parameters);
            if (album_artist_parameters.Count != 0)
                DB.NonQuery(album_artist_quary, album_artist_parameters);
            if(artist_parameters.Count != 0)
                DB.NonQuery(artist_quary, artist_parameters);
            if(track_parameters.Count != 0)
                DB.NonQuery(track_quary, track_parameters);
            if(location_parameters.Count != 0)
                DB.NonQuery(location_quary, location_parameters);
            if(track_artist_parameters.Count != 0)
                DB.NonQuery(track_artist_quary, track_artist_parameters);
            if(track_parameters.Count+location_parameters.Count+track_artist_parameters.Count+artist_parameters.Count+album_parameters.Count+album_artist_parameters.Count > 0)
                MessageBox.Show("データベースに曲を追加しました。");
            else
                MessageBox.Show("データベースに曲を追加する必要はありませんでした。");
        }
        
        //Device設定
        private string AddMachine()//成功時true
        {
            Collection<MySqlParameter> parameters = new Collection<MySqlParameter>();
            parameters.Add(new MySqlParameter("@device_name", Environment.MachineName));
            Collection<Dictionary<string,object>> res = DB.Read("select * from device where device_name = @device_name",parameters);
            if(res.Count == 0)
            {
                string id = Guid.NewGuid().ToString();
                parameters.Add(new MySqlParameter("@device_id", id));
                DB.NonQuery("insert into device (device_id,device_name) values (@device_id,@device_name)", parameters);
                Properties.Settings.Default.LibraryAddedMachine = true;
                Properties.Settings.Default.MachineID = id;
                Properties.Settings.Default.Save();
                return id;
            }
            MessageBox.Show("このマシン名はすでに登録されています。\n設定画面から変更してください。");
            return null;
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