using System.Collections.ObjectModel;

namespace TolyMusic_for_PC
{
    public class Album
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string artist { get; set; }
        public string genre { get; set; }
        public string year { get; set; }
        public string path { get; set; }
        public Collection<Track> Tracks { get; set; }
    }
}