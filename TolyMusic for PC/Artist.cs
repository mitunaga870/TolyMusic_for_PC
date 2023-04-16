using System.Collections.ObjectModel;

namespace TolyMusic_for_PC
{
    public class Artist
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Collection<Album> Albums { get; set; }
    }
}