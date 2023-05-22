using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using Newtonsoft.Json.Linq;

namespace TolyMusic_for_PC
{
    public class Artist
    {
        public Artist(Dictionary<string, object> item)
        {
            Id = item["artist_id"].ToString();
            Name = item["artist_name"].ToString();
            if(Other.CheckDBValue(item,"artist_name_pron"))
                Name_pron = item["artist_name_pron"].ToString();
        }
        //ytMusic用
        public Artist(JObject json)
        {
            Id = json["id"].ToString();
            Name = json["name"].ToString();
            Name_pron = null;
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Name_pron { get; set; }
    }
}