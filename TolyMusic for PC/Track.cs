using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace TolyMusic_for_PC
{
    public class Track
    {
        public string Composer_id;
        public string Group_id;
        public string Id { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public string Title_pron { get; set; }
        public double Duration { get; set; }
        public int TrackNumber { get; set; }
        public string Album_id { get; set; }
        public Collection<Artist> Artists { get; set; }
        public int location { get; set; }
        public string youtube_id { get; set; }

        //コンストラクタ
        //SQL用
        public Track(Dictionary<string,object> dictionary)
        {
            Id = dictionary["track_id"].ToString();
            Title = dictionary["track_title"].ToString();
            if(Other.CheckDBValue(dictionary,"album_id"))
                Album_id = dictionary["album_id"].ToString();
            if(Other.CheckDBValue(dictionary,"composer_id"))
                Composer_id = dictionary["composer_id"].ToString();
            if(Other.CheckDBValue(dictionary,"group_id"))
                Group_id = dictionary["group_id"].ToString();
            if(Other.CheckDBValue(dictionary,"path"))
                Path = dictionary["path"].ToString();
            if(Other.CheckDBValue(dictionary,"track_title_pron"))
                Title_pron = dictionary["track_title_pron"].ToString();
            if (Other.CheckDBValue(dictionary,"duration"))
                Duration = (double) dictionary["duration"];
            if (Other.CheckDBValue(dictionary,"track_num"))
                TrackNumber = (int)dictionary["track_num"];
            else
                TrackNumber = -1;
            if(Other.CheckDBValue(dictionary,"location"))
                location = (int)dictionary["location"];
            if(Other.CheckDBValue(dictionary,"youtube_id"))
                youtube_id = dictionary["youtube_id"].ToString();
            Artists = new Collection<Artist>();
        }
        //yuMusic用
        public Track(JObject json)
        {
            Id = json["videoId"].ToString();
            Title = json["title"].ToString();
            if (json.ContainsKey("album"))
            {
                var album = json["album"].ToString();
                if (album != "none")
                    Album_id = json["album"]["id"].ToString();
            }
            Artists = new Collection<Artist>();
            Duration = (double)json["duration_seconds"];
            foreach (var artist in json["artists"])
                Artists.Add(new Artist((JObject)artist));
        }
        public void addArtist(Dictionary<string,object> dictionary)
        {
            var artist = new Artist(dictionary);
            if(!Artists.Contains(artist))
                Artists.Add(artist);
        }
    }
}