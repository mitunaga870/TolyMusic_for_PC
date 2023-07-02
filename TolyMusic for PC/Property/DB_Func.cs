using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using TolyMusic_for_PC.Library;

namespace TolyMusic_for_PC.Property;

public class DB_Func
{
    public static Collection<Artist> GetAllArtist()
    {
        var res = DB.Read("SELECT distinct * FROM Artist");
        return Other.LibDictoArtists(res);
    }
    public static Collection<Album> GetAllAlbum()
    {
        var res = DB.Read("SELECT distinct * FROM Album");
        return Other.LibDictoAlbums(res);
    }

    public static Collection<Track> GetAllTrack()
    {
        var res = DB.Read("SELECT distinct * FROM Tracks");
        return Other.LibDictoTracks(res);
    }
    
    public static void AddArtist(string id,string name)
    {
        var param = new Collection<MySqlParameter>();
        param.Add(new MySqlParameter("@id", id));
        param.Add(new MySqlParameter("@name", name));
        DB.NonQuery("INSERT INTO Artist (Id,Name) VALUES (@id,@name)", param);
    }
}