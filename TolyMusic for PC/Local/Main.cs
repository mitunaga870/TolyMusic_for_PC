using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace TolyMusic_for_PC.Local
{
    public class Main
    {
        //変数宣言
        public enum id_type
        {
            artist,
            album,
            track
        }
        //private変数
        private ViewModel vm;
        private Player player;
        private Queue queue;
        private Grid container;
        private StackPanel func_container;

        public Main(){}
        public Main(ViewModel vm, Player player, Queue queue, Grid container, StackPanel funcContainer)
        {
            this.vm = vm;
            this.player = player;
            this.queue = queue;
            this.container = container;
            this.func_container = funcContainer;
        }

        public void Init()//ローカル内ファイル用DBの初期化
        {
            
            //ファイルのパスを配列で取得
            Collection<string> files = new Collection<string>();
            foreach (var path in Properties.Settings.Default.LocalDirectryPath.Split(','))
            {
                if (path == "")
                {
                    continue;
                }
                string[] tmp_strary = System.IO.Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                foreach (string tmp_str in tmp_strary)
                {
                    files.Add(tmp_str);
                }
            }
            //DBが存在しない時作成
            Local.DB.NonQuery(new string[] 
            { 
                "create table IF NOT EXISTS tracks(track_id char(32) primary key,track_title varchar(255),track_title_pron varchar(255),album_id char(32),composer_id char(32),group_id char(32),track_num int,duration double,location int,play_count int,device_id char(32),path varchar(255),youtube_id varchar(255),tois_favearchives_id char(32));", 
                "create table IF NOT EXISTS artist(    artist_id char(32) primary key,    artist_name varchar(255),    artist_name_pron varchar(255));", 
                "create table IF NOT EXISTS album(    album_id char(32) primary key,    album_title varchar(255),    album_title_pron varchar(255))", 
                "create table IF NOT EXISTS \"group\"(    group_id char(32) primary key,    group_name varchar(255),    group_name_pron varchar(255));", 
                "create table IF NOT EXISTS track_artist(    track_id char(32),    artist_id char(32));", 
                "create table IF NOT EXISTS artist_group(    artist_id char(32),    group_id char(32));", 
                "create table IF NOT EXISTS album_artist(    album_id char(32),    artist_id char(32));", 
                "create table IF NOT EXISTS album_group(    album_id char(32),    group_id char(32));", 
                "create table IF NOT EXISTS playlist(    playlist_id char(32) primary key,    playlist_title varchar(255),    play_count int);", 
                "create table IF NOT EXISTS playlist_track(    playlist_id char(32),    track_id char(32),      track_num int);", 
                "create table IF NOT EXISTS history_track(    history_num int auto_increment,    track_id char(32));", 
                "create table IF NOT EXISTS history_playlist(    history_num int auto_increment,    playlist_id char(32));", 
                "create table IF NOT EXISTS device(    device_id char(32) primary key,    device_name varchar(255),    device_description varchar(255));",
            });
            //既存トラックのパスを取得
            Collection<Track> tracks = GetTracks();
            //じゅうふくアーティスト・アルバムデータ保存用の変数を用意
            var added_artist = new Dictionary<string, string>();
            var added_album = new Dictionary<string, string>();
            foreach (var a in GetArtists())
            {
                added_artist.Add(a.Name, a.Id);
            }
            foreach (var a in GetAlbums())
            {
                added_album.Add(a.Title, a.Id);
            }
            //新規用ディレクトリ
            var album = new Dictionary<string, string>();
            var artist = new Dictionary<string, string>();
            //tracks及びtrack_albumテーブルに書き込む
            List<SQLiteParameter[]> params_tracks = new List<SQLiteParameter[]>();
            List<SQLiteParameter[]> params_track_album = new List<SQLiteParameter[]>();
            foreach (string file in files){
                //拡張子の確認
                if (Path.GetExtension(file) != ".mp3"&& Path.GetExtension(file) != ".m4a"&& Path.GetExtension(file) != ".flac" && Path.GetExtension(file) != ".wav")
                    continue;
                //既に登録されているか確認
                if(tracks.Where(x => x.Path == file).Count() != 0)
                    continue;
                //パラメータ配列の作成
                SQLiteParameter[] param_tracks = new SQLiteParameter[9];
                //ファイルのタグを取得
                TagLib.File f = TagLib.File.Create(file);
                //タグデータの文字化けが予想される場合・パスから取得するように
                bool Tagfail = Regex.Match(f.Tag.Title,@"�").Success;
                //楽曲idを生成
                string trackid = Guid.NewGuid().ToString(); //挿入可能なパラメータは挿入
                param_tracks[0] = (new SQLiteParameter("track_id", trackid));
                if(Tagfail)
                {
                    string title = Path.GetFileNameWithoutExtension(file);
                    title = Regex.Replace(title, @"[0-9]+-", "");
                    param_tracks[1] = (new SQLiteParameter("track_title", title));
                    param_tracks[2] = new SQLiteParameter("track_title_pron", null);
                    param_tracks[3] = (new SQLiteParameter("track_num", null));
                }
                else
                {
                    param_tracks[1] = (new SQLiteParameter("track_title", f.Tag.Title));
                    param_tracks[2] = new SQLiteParameter("track_title_pron", f.Tag.TitleSort);
                    param_tracks[3] = (new SQLiteParameter("track_num", f.Tag.TrackCount));
                }
                param_tracks[4] = (new SQLiteParameter("duration", f.Properties.Duration.TotalSeconds));
                param_tracks[5] = (new SQLiteParameter("path", file));

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
                if (added_album.ContainsKey(album_title))
                {
                    param_tracks[6] = new SQLiteParameter("$album_id", added_album[album_title]);
                }
                else
                {
                    string albumid = Guid.NewGuid().ToString();
                    album.Add(album_title, albumid);
                    added_album.Add(album_title, albumid);
                    param_tracks[6] = new SQLiteParameter("$album_id", albumid);
                }
                //グループの存在確認及びグループidの生成・取得
                string group_name;
                if (f.Tag.FirstAlbumArtist != null&&!Tagfail&&!(Regex.Match(f.Tag.FirstAlbumArtist,@"�").Success))
                {
                    group_name = f.Tag.FirstAlbumArtist;
                    if (added_artist.ContainsKey(group_name))
                    { 
                        param_tracks[7] = new SQLiteParameter("$group_id", artist[group_name]);
                    }
                    else 
                    { 
                        string groupid = Guid.NewGuid().ToString(); 
                        artist.Add(group_name, groupid); 
                        added_artist.Add(group_name, groupid);
                        param_tracks[7] = new SQLiteParameter("$group_id", groupid);
                    }
                }
                else
                {
                    param_tracks[7] = new SQLiteParameter("$group_id", null);
                }
                
                //作曲者の存在確認及び作曲者idの生成・取得
                if (f.Tag.FirstComposer != null&&!Tagfail)
                {
                    if (added_artist.ContainsKey(f.Tag.FirstComposer))
                    {
                        param_tracks[8] = new SQLiteParameter("$composer_id", added_artist[f.Tag.FirstComposer]);
                    }
                    else
                    {
                        string composerid = Guid.NewGuid().ToString();
                        artist.Add(f.Tag.FirstComposer, composerid);
                        added_artist.Add(f.Tag.FirstComposer, composerid);
                        param_tracks[8] = new SQLiteParameter("$composer_id", composerid);
                    }
                }
                else
                {
                    param_tracks[8] = new SQLiteParameter("$composer_id", null);
                }
                params_tracks.Add(param_tracks);
                //アーティストの存在確認及びアーティストidの生成・取得
                if (f.Tag.FirstPerformer != null&&!Tagfail&&!(Regex.Match(f.Tag.Performers[0],@"�").Success))
                {
                    foreach (var performers in f.Tag.Performers)
                    {
                        foreach (var performer in Regex.Split(performers,@"[、,]"))
                        {
                            SQLiteParameter[] param_track_album = new SQLiteParameter[2];
                            param_track_album[0]=(new SQLiteParameter("track_id", trackid));
                            string performer_name = Regex.Replace(performer, @"(\s),*", "");
                            if (added_artist.ContainsKey(performer_name))
                            {
                                param_track_album[1] = new SQLiteParameter("$artist_id", added_artist[performer_name]);
                            }
                            else
                            {
                                string artistid = Guid.NewGuid().ToString();
                                artist.Add(performer_name, artistid);
                                added_artist.Add(performer_name, artistid);
                                param_track_album[1] = new SQLiteParameter("$artist_id", artistid);
                            }
                            params_track_album.Add(param_track_album);
                        }
                    }
                }
                else
                {
                    string tmp = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(file)));
                    foreach (var performer in Regex.Split(tmp, @"[、,]"))
                    {
                        SQLiteParameter[] param_track_album = new SQLiteParameter[2];
                        param_track_album[0]=(new SQLiteParameter("track_id", trackid));
                        if (added_artist.ContainsKey(performer))
                        { 
                            param_track_album[1] = new SQLiteParameter("$artist_id", added_artist[performer]);
                        }
                        else
                        { 
                            string artistid = Guid.NewGuid().ToString(); 
                            artist.Add(performer, artistid); 
                            added_artist.Add(performer, artistid);
                            param_track_album[1] = new SQLiteParameter("$artist_id", artistid);
                        }
                        params_track_album.Add(param_track_album);
                    }
                }
            };
            Local.DB.NonQuery(@"INSERT INTO tracks (track_id,track_title,track_title_pron,album_id,composer_id,group_id,track_num,duration,play_count,path) values ($track_id,$track_title,$track_title_pron,$album_id,$composer_id,$group_id,$track_num,$duration,0,$path)",params_tracks);
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
            Collection<Dictionary<string,object>> res = Local.DB.Reader("SELECT * FROM tracks join track_artist on tracks.track_id = track_artist.track_id join artist on track_artist.artist_id = artist.artist_id");
            ObservableCollection<Track> result = new ObservableCollection<Track>();
            result = MakeTracks(res);
            return result;
        }
        public ObservableCollection<Track> GetTracks(string id, id_type filtter)
        {
            Collection<Dictionary<string, object>> res;
            ObservableCollection<Track> result = new ObservableCollection<Track>();
            switch (filtter)
            {
                case id_type.album:
                    res = Local.DB.Reader("SELECT * FROM tracks join track_artist on tracks.track_id = track_artist.track_id join artist on track_artist.artist_id = artist.artist_id WHERE album_id = @id", new SQLiteParameter[] { new SQLiteParameter("@id", id) });
                    break;
                case id_type.artist:
                    res = DB.Reader("SELECT * FROM tracks t join track_artist ta on t.track_id = ta.track_id join artist a on ta.artist_id = a.artist_id WHERE t.track_id IN (SELECT track_id FROM track_artist WHERE artist_id = @id) OR t.composer_id = @id OR t.group_id = @id OR t.album_id IN (SELECT album_id FROM album_group where group_id = @id) OR t.album_id IN (SELECT album_id From album_artist WHERE artist_id = @id)", new SQLiteParameter[] { new SQLiteParameter("@id", id) });
                    break;
                default:
                    throw new NotImplementedException();
            }
            result = MakeTracks(res);
            return result;
        }
        public ObservableCollection<Album> GetAlbums()
        {
            Collection<Dictionary<string, object>> res = Local.DB.Reader("SELECT * FROM album a join tracks t on a.album_id = t.album_id join track_artist ta on t.track_id = ta.track_id join artist ar on ta.artist_id = ar.artist_id");
            ObservableCollection<Album> result = new ObservableCollection<Album>();
            result = MakeAlbum(res);
            return result;
        }
        public ObservableCollection<Album> GetAlbums(string id, id_type filtter)
        {
            switch (filtter)
            {
                case id_type.artist:
                    Collection<Dictionary<string, object>> res = Local.DB.Reader("SELECT * FROM album a join tracks t on a.album_id = t.album_id join track_artist ta on t.track_id = ta.track_id join artist ar on ta.artist_id = ar.artist_id WHERE a.album_id IN (SELECT album_id FROM tracks WHERE track_id IN (SELECT track_id FROM track_artist WHERE artist_id = @id) OR composer_id = @id OR group_id = @id) OR a.album_id IN (SELECT album_id FROM album_artist WHERE artist_id = @id)", new SQLiteParameter[] { new SQLiteParameter("@id", id) });
                    ObservableCollection<Album> result = new ObservableCollection<Album>();
                    result = MakeAlbum(res);
                    return result;
                default:
                    throw new NotImplementedException();
            }
        }
        public ObservableCollection<Artist> GetArtists()
        {
            Collection<Dictionary<string,object>> res = Local.DB.Reader("SELECT * FROM artist");
            ObservableCollection<Artist> result = new ObservableCollection<Artist>();
            foreach (var row in res)
            {
                Artist artist = new Artist(row);
                result.Add(artist);
            }
            return result;
        }
        //private関数
        private ObservableCollection<Track> MakeTracks(Collection<Dictionary<string,object>> reader)
        { 
            Collection<string> addedid = new Collection<string>();
            ObservableCollection<Track> result = new ObservableCollection<Track>();
            foreach (var row in reader)
            {
                if (addedid.Contains(row["track_id"].ToString()))
                {
                    Track item = result[result.Count - 1];
                    item.addArtist(row);
                }
                else
                {
                    Track item = new Track(row);
                    item.addArtist(row);
                    result.Add(item);
                    addedid.Add(row["track_id"].ToString());
                }
            }
            return result;
        }
        private ObservableCollection<Album> MakeAlbum(Collection<Dictionary<string, object>> reader)
        {
            Collection<string> addedid = new Collection<string>();
            ObservableCollection<Album> result = new ObservableCollection<Album>();
            foreach (var row in reader)
            {
                if(addedid.Contains(row["album_id"].ToString()))
                {
                    Album item = result.First(r => r.Id == row["album_id"].ToString());
                    item.addArtist(row);
                }
                else
                {
                    Album item = new Album(row);
                    item.addArtist(row);
                    result.Add(item);
                    addedid.Add(row["album_id"].ToString());
                }
            }
            return result;
        }
    }
}