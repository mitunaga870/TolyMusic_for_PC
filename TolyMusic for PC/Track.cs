using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        public Track(Dictionary<string,object> dictionary)
        {
            Album_id = dictionary["album_id"].ToString();
            Composer_id = dictionary["composer_id"].ToString();
            Group_id = dictionary["group_id"].ToString();
            Id = dictionary["track_id"].ToString();
            Title = dictionary["track_title"].ToString();
            Path = dictionary["path"].ToString();
            Title_pron = dictionary["track_title_pron"].ToString();
            Duration = (double) dictionary["duration"];
            var truck_num = dictionary["track_num"];
            if (DBNull.Value != truck_num)
                TrackNumber = (int)truck_num;
            else
                TrackNumber = 0;
            Artists = new Collection<Artist>();
        }
        public void addArtist(Dictionary<string,object> dictionary)
        {
            Artists.Add(new Artist(dictionary));
        }
    }
}