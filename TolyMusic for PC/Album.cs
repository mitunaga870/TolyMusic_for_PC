using System.Collections.ObjectModel;

namespace TolyMusic_for_PC
{
    public class Album
    {
        string title { get; set; }
        string artist { get; set; }
        string genre { get; set; }
        string year { get; set; }
        string path { get; set; }
        Collection<Track> Tracks { get; set; }
        int length { get; set; }
    }
}