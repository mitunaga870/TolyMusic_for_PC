using System.Collections.ObjectModel;

namespace TolyMusic_for_PC
{
    public class Artist
    {
        string name { get; set; }
        Collection<Track> Tracks { get; set; }
    }
}