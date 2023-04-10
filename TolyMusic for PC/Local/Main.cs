using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace TolyMusic_for_PC.Local
{
    public class Main
    {
        public Main()
        {
            //初回時にはファイルを漁る
            Init();
        }

        public void Init()//ローカル内ファイル用DBの初期化
        {
            //ファイルのパスを配列で取得
            String[] files = System.IO.Directory.GetFiles("D:\\music\\","*",SearchOption.AllDirectories);
            //テーブルの初期化
            Local.DB.NonQuery(new String[]{ 
                "drop table IF EXISTS tracks;",
                "drop table IF EXISTS artist;",
                "drop table IF EXISTS album;",
                "drop table IF EXISTS \"group\";",
                "drop table IF EXISTS track_artist;",
                
                "drop table IF EXISTS artist_group;",
                "drop table IF EXISTS album_artist;",
                "drop table IF EXISTS album_group;",
                "drop table IF EXISTS playlist;",
                "drop table IF EXISTS playlist_track;",
                "drop table IF EXISTS history_track;",
                "drop table IF EXISTS history_playlist;",
                "drop table IF EXISTS device;", 
            });
            Local.DB.NonQuery(new string[] 
            { 
                "create table IF NOT EXISTS tracks(track_id char(32) primary key,track_title varchar(255),track_title_pron varchar(255),album_id char(32),composer_id char(32),group_id char(32),track_number int,duration double,location int,play_count int,device_id char(32),path varchar(255),youtube_id varchar(255),tois_favearchives_id char(32));", 
                "create table artist(    artist_id char(32) primary key,    artist_name varchar(255),    artist_name_pron varchar(255));", 
                "create table album(    album_id char(32) primary key,    album_title varchar(255),    album_title_pron varchar(255))", 
                "create table \"group\"(    group_id char(32) primary key,    group_name varchar(255),    group_name_pron varchar(255));", 
                "create table track_artist(    track_id char(32),    artist_id char(32));", 
                "create table artist_group(    artist_id char(32),    group_id char(32));", 
                "create table album_artist(    album_id char(32),    artist_id char(32));", 
                "create table album_group(    album_id char(32),    group_id char(32));", 
                "create table playlist(    playlist_id char(32) primary key,    playlist_title varchar(255),    play_count int);", 
                "create table playlist_track(    playlist_id char(32),    track_id char(32),      track_number int);", 
                "create table history_track(    history_num int auto_increment,    track_id char(32));", 
                "create table history_playlist(    history_num int auto_increment,    playlist_id char(32));", 
                "create table device(    device_id char(32) primary key,    device_name varchar(255),    device_description varchar(255));",
            });
            //じゅうふくアーティスト・アルバムデータ保存用の変数を用意
            var album = new Dictionary<string, string>();
            var artist = new Dictionary<string, string>();
            //tracks及びtrack_albumテーブルに書き込む
            List<SQLiteParameter[]> params_tracks = new List<SQLiteParameter[]>();
            List<SQLiteParameter[]> params_track_album = new List<SQLiteParameter[]>();
            foreach (string file in files){
                //パラメータ配列の作成
                SQLiteParameter[] param_tracks = new SQLiteParameter[8];
                //ファイルのタグを取得
                TagLib.File f = TagLib.File.Create(file);
                //タグデータの文字化けが予想される場合・パスから取得するように
                bool Tagfail = Regex.Match(f.Tag.Title,@"�").Success;
                //楽曲idを生成
                string trackid = Guid.NewGuid().ToString(); //挿入可能なパラメータは挿入
                param_tracks[0] = (new SQLiteParameter("track_id", trackid));
                if(Tagfail){
                    param_tracks[1] = (new SQLiteParameter("track_title", Path.GetFileNameWithoutExtension(file)));
                    param_tracks[2] = (new SQLiteParameter("track_num", null));
                }
                else
                {
                    param_tracks[1] = (new SQLiteParameter("track_title", f.Tag.Title));
                    param_tracks[2] = (new SQLiteParameter("track_num", f.Tag.TrackCount));
                }
                param_tracks[3] = (new SQLiteParameter("duration", f.Properties.Duration.TotalSeconds));
                param_tracks[4] = (new SQLiteParameter("path", file));

                //アルバムの存在確認及びアルバムidの生成・取得
                string album_title;
                if (f.Tag.Album != null&&!Tagfail)
                {
                    album_title = f.Tag.Album;
                }
                else
                {
                    album_title = Path.GetFileName(Path.GetDirectoryName(file));
                }
                if (album.ContainsKey(album_title))
                {
                    param_tracks[5] = new SQLiteParameter("$album_id", album[album_title]);
                }
                else
                {
                    string albumid = Guid.NewGuid().ToString();
                    album.Add(album_title, albumid);
                    param_tracks[5] = new SQLiteParameter("$album_id", albumid);
                }
                //グループの存在確認及びグループidの生成・取得
                string group_name;
                if (f.Tag.FirstAlbumArtist != null&&!Tagfail&&!(Regex.Match(f.Tag.FirstAlbumArtist,@"�").Success))
                {
                    group_name = f.Tag.FirstAlbumArtist;
                    if (artist.ContainsKey(group_name))
                    { 
                        param_tracks[6] = new SQLiteParameter("$group_id", artist[group_name]);
                    }
                    else 
                    { 
                        string groupid = Guid.NewGuid().ToString(); 
                        artist.Add(group_name, groupid); 
                        param_tracks[6] = new SQLiteParameter("$group_id", groupid);
                    }
                }
                else
                {
                    param_tracks[6] = new SQLiteParameter("$group_id", null);
                }
                
                //作曲者の存在確認及び作曲者idの生成・取得
                if (f.Tag.FirstComposer != null&&!Tagfail)
                {
                    if (artist.ContainsKey(f.Tag.FirstComposer))
                    {
                        param_tracks[7] = new SQLiteParameter("$composer_id", artist[f.Tag.FirstComposer]);
                    }
                    else
                    {
                        string composerid = Guid.NewGuid().ToString();
                        artist.Add(f.Tag.FirstComposer, composerid);
                        param_tracks[7] = new SQLiteParameter("$composer_id", composerid);
                    }
                }
                else
                {
                    param_tracks[7] = new SQLiteParameter("$composer_id", null);
                }
                params_tracks.Add(param_tracks);
                //track_album用パラメータづくり
                SQLiteParameter[] param_track_album = new SQLiteParameter[2];
                //挿入可能なパラメータは挿入
                param_track_album[0]=(new SQLiteParameter("track_id", trackid));
                //アーティストの存在確認及びアーティストidの生成・取得
                if (f.Tag.FirstPerformer != null&&!Tagfail&&!(Regex.Match(f.Tag.Performers[0],@"�").Success))
                {
                    foreach (var performer in f.Tag.Performers)
                    {
                        if (artist.ContainsKey(performer))
                        {
                            param_track_album[1] = new SQLiteParameter("$artist_id", artist[performer]);
                        }
                        else
                        {
                            string artistid = Guid.NewGuid().ToString();
                            artist.Add(performer, artistid);
                            param_track_album[1] = new SQLiteParameter("$artist_id", artistid);
                        }
                    }
                }
                else
                {
                    string tmp = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(file)));
                    string[] artists = tmp.Split(new string[]{", "},StringSplitOptions.None);
                    foreach (var performer in artists)
                    {
                        if (artist.ContainsKey(performer))
                        { 
                            param_track_album[1] = new SQLiteParameter("$artist_id", artist[performer]);
                        }
                        else
                        { 
                            string artistid = Guid.NewGuid().ToString(); 
                            artist.Add(performer, artistid); 
                            param_track_album[1] = new SQLiteParameter("$artist_id", artistid);
                        }
                    }
                }
                params_track_album.Add(param_track_album);
            };
            Local.DB.NonQuery(@"INSERT INTO tracks (track_id,track_title,album_id,composer_id,group_id,track_number,duration,play_count,path) values ($track_id,$track_title,$album_id,$composer_id,$group_id,$track_num,$duration,0,$path)",params_tracks);
            Local.DB.NonQuery(@"INSERT INTO track_artist (track_id,artist_id) values ($track_id,$artist_id)",params_track_album);
            //alubmsテーブルに書き込む
            List<SQLiteParameter[]> params_albums = new List<SQLiteParameter[]>();
            album.ToList().ForEach(a =>
            {
                params_albums.Add(new SQLiteParameter[] { new SQLiteParameter("$album_id", a.Value), new SQLiteParameter("$album_title", a.Key) });
            });
            Local.DB.NonQuery(@"INSERT INTO album (album_id,album_title) values ($album_id,$album_title)", params_albums);
            //artistsテーブルに書き込む
            List<SQLiteParameter[]> params_artists = new List<SQLiteParameter[]>();
            artist.ToList().ForEach(a =>
            {
                params_artists.Add(new SQLiteParameter[] { new SQLiteParameter("$artist_id", a.Value), new SQLiteParameter("$artist_name", a.Key) });
            });
            Local.DB.NonQuery(@"INSERT INTO artist (artist_id,artist_name) values ($artist_id,$artist_name)", params_artists);
            MessageBox.Show("ローカル用データベース構築完了");
        }
        
        public ObservableCollection<Track> GetTracks()
        {
            Collection<Dictionary<string,object>> res = Local.DB.Reader("SELECT * FROM tracks");
            ObservableCollection<Track> result = new ObservableCollection<Track>();
            foreach (var row in res)
            {
                Track track = new Track
                {
                    id = row["track_id"].ToString(),
                    Title = row["track_title"].ToString(),
                    Path = row["path"].ToString(),
                };
                result.Add(track);
            }
            return result;
        }
    }
}