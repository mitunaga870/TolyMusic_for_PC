using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;

namespace TolyMusic_for_PC
{
    public class Artist
    {
        public Artist(Dictionary<string, object> item)
        {
            Id = item["artist_id"].ToString();
            Name = item["artist_name"].ToString();
            Name_pron = item["artist_name_pron"].ToString();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Name_pron { get; set; }
    }
}