using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace TolyMusic_for_PC
{
    public class Album
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string TitlePron { get; set; }
        public Collection<Artist> Artists { get; set; }
        //コンストラクタ
        public Album(Dictionary<string, object> dictionary)
        {
            Id = dictionary["album_id"].ToString();
            Title = dictionary["album_title"].ToString();
            TitlePron = dictionary["album_title_pron"].ToString();
            Artists = new Collection<Artist>();
        }
        //yuMusic用
        public Album(JObject json)
        {
            var jjson = json["album"];
            Id = jjson["id"].ToString();
            Title = jjson["name"].ToString();
            Artists = new Collection<Artist>();
            foreach (var artist in json["artists"])
                Artists.Add(new Artist((JObject)artist));
        }
        public void addArtist(Dictionary<string, object> dictionary)
        {
            Artists.Add(new Artist(dictionary));
        }
    }
}