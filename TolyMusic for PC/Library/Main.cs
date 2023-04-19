using System.Collections.ObjectModel;
using MySqlConnector;

namespace TolyMusic_for_PC.Library
{
    public class Main
    {
        ViewModel vm;
        DB lib;
        public Main(ViewModel vm)
        {
            this.vm = vm;
            lib = new DB();
        }
        //ライブラリにリスト上の曲を追加
        public void AddListTracks(Collection<Track> tracks)
        {
            string quary = "INSERT INTO tracks (track_id,track_title,track_title_pron,composer_id,group_id,track_num,duration,location,device_id,path) VALUES ";
            Collection<MySqlParameter> parameters = new Collection<MySqlParameter>();
            for (int i = 0; i < tracks.Count; i++)
            {
                parameters.Add(new MySqlParameter("@track_id" + i, tracks[i].id));
                parameters.Add(new MySqlParameter("@track_title"+i, tracks[i].Title));
                parameters.Add(new MySqlParameter("@track_title_pron"+i, tracks[i].Title_pron));
                parameters.Add(new MySqlParameter("@composer_id"+i, tracks[i].Composer_id));
                parameters.Add(new MySqlParameter("@group_id" + i, tracks[i].Group_id));
                parameters.Add(new MySqlParameter("@track_num" + i, tracks[i].Track_number));
                parameters.Add(new MySqlParameter("@duration" + i, tracks[i].Duration));
                parameters.Add(new MySqlParameter("@location" + i, "Local"));
                parameters.Add(new MySqlParameter("@device_id" + i, Properties.Settings.Default.DeviceID));
                parameters.Add(new MySqlParameter("@path" + i, tracks[i].Path));
            }
        }
    }
}