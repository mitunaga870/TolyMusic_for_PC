using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace TolyMusic_for_PC.Property
{
    public abstract partial class PropertyWindow : Window
    {
        protected ViewModel vm;
        protected int Curt_num;
        protected Collection<Artist> AllArtist;
        protected Collection<Album> AllAlbum;
        protected Collection<Track> AllTrack;
        protected string prev_id;
        
        public ObservableCollection<Artist> AddedArtist;
        public ObservableCollection<Artist> AddedGroup;

        public PropertyWindow(ViewModel vm)
        {
            this.vm = vm;
            AllArtist = DB_Func.GetAllArtist();
            AllAlbum = DB_Func.GetAllAlbum();
            AllTrack = DB_Func.GetAllTrack();
            AddedArtist = new ObservableCollection<Artist>();
            ContentRendered += (sender, e) => Load();
        }
        protected abstract void Load();
        protected abstract void Send_Data(object sender, RoutedEventArgs e);
        protected void Prev(object sender, RoutedEventArgs e)
        {
            if (Curt_num == 0)
                return;
            Curt_num--;
            Load();
        }
        protected void Next(object sender, RoutedEventArgs e)
        {
            if (Curt_num == vm.Tracks.Count - 1)
                return;
            Curt_num++;
            Load();
        }
    }
}