using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TolyMusic_for_PC;

public class Playlist
{
    //メンバ
    public string Id { get; set;}
    public string Title { get; set; }
    public ObservableCollection<Track> Tracks;
    //コンストラクタ
    public Playlist(string id, string title)
    {
        Id = id;
        Title = title;
        Tracks = new ObservableCollection<Track>();
    }
    public Playlist(Dictionary<string, object> dic)
    {
        Id = dic["playlist_id"].ToString();
        Title = dic["playlist_title"].ToString();
        Tracks = new ObservableCollection<Track>();
    }
    //トラック追加
    public void AddTrack(Track track)
    {
        if (Tracks.Count(t => t.Id == track.Id) == 0)
            Tracks.Add(track);
    }
}