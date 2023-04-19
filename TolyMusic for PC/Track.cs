namespace TolyMusic_for_PC
{
    public class Track
    {
        public string Composer_id;
        public string Group_id;
        public string Track_number;
        public string id { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public string Title_pron { get; set; }
        string Artist { get; set; }
        string Album { get; set; }
        string Genre { get; set; }
        string Year { get; set; }
        public int Duration { get; set; }
        bool Cover { get; set; }
        string Lyrics { get; set; }
        string AlbumArtist { get; set; }
        string Composer { get; set; }
        int TrackNumber { get; set; }
    }
}