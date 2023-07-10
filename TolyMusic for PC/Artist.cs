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
            Groups = new ObservableCollection<Artist>();
        }
        //ytMusic用
        public Artist(JObject json)
        {
            Id = json["id"].ToString();
            Name = json["name"].ToString();
            Name_pron = null;
            Groups = new ObservableCollection<Artist>();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Name_pron { get; set; }
        public ObservableCollection<Artist> Groups { get; set; }
        //表示用
        public string Group
        {
            get
            {
                string res = "";
                foreach (var group in Groups)
                    res += group.Name + ",";
                res = res.Remove(res.Length - 1);
                return res;
            }
        }
    }
}