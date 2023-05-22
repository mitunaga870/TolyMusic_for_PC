using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace TolyMusic_for_PC;

public class Other
{
    public static bool CheckDBValue(Dictionary<string,object> dic, string key)
    {
        return dic.ContainsKey(key) && dic[key] != DBNull.Value;
    }

    public static ObservableCollection<Track> LibDictoTrack(Collection<Dictionary<string, object>> dics)
    {
        //リザルト作成
        var result = new ObservableCollection<Track>();
        foreach (var dic in dics)
        {
            //ローカルの別デバイスは除外
            if(CheckDBValue(dic,"device_name"))
                if(dic["device_name"].ToString() != Environment.MachineName)
                    continue;
            //track作成
            var track = new Track(dic);
            track.addArtist(dic);
            //重複確認
            if (result.Where(t => t.Id == track.Id).Count() > 0)
            {
                var added_track = result.Where(t => t.Id == track.Id).ToList()[0];
                //アーティストの存在確認
                if(track.Artists.Count == 0)
                    continue;
                else//重複していたらアーティストを追加
                    result.Where(t => t.Id == track.Id).ToList()[0].Artists.Add(track.Artists[0]);
                //登録済みがローカルあるいは重複よりも優先度が高ければやめる
                if(added_track.location == 0||added_track.location <= track.location)
                    continue;
                else //そうでないときは重複を優先
                {
                    result.Where(t => t.Id == track.Id).ToList()[0].location = track.location;
                    result.Where(t => t.Id == track.Id).ToList()[0].Path = track.Path;
                    result.Where(t => t.Id == track.Id).ToList()[0].youtube_id = track.youtube_id;
                }
            }
            else
                result.Add(track);
        }
        return result;
    }
}