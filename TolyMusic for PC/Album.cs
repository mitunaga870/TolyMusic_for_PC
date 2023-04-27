using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        }
    }
}