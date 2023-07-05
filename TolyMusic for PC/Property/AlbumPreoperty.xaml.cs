using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using MySql.Data.MySqlClient;
using TolyMusic_for_PC.Library;

namespace TolyMusic_for_PC.Property;

public partial class AlbumPreoperty : PropertyWindow
{
    public AlbumPreoperty(ViewModel vm) : base(vm)
    {
        Curt_num = vm.Albums.ToList().FindIndex(x => x.Id == vm.Preoperty_Id);
        InitializeComponent();
    }

    protected override void Load()
    {
        //タイトル
        Title.Content = vm.Albums[Curt_num].Title;
        Title_Combobox.ItemsSource = AllAlbum;
        Title_Combobox.DisplayMemberPath = "Title";
        Title_Combobox.SelectedIndex = AllAlbum.ToList().FindIndex(x => x.Id == vm.Albums[Curt_num].Id);
        //読み
        TitlePron_Textbox.Text = vm.Albums[Curt_num].TitlePron;
        //アーティスト
        Artist_ComboBox.ItemsSource = AllArtist;
        Artist_ComboBox.DisplayMemberPath = "Name";
        AddedArtist = new ObservableCollection<Artist>(vm.Albums[Curt_num].Artists);
        Artist_List.ItemsSource = AddedArtist;
    }

    protected override void Send_Data(object sender, RoutedEventArgs e)
    {
        string album_id = vm.Albums[Curt_num].Id;
        //タイトル
        string title;
        if (Title_Combobox.SelectedIndex == -1)
        {//曲名が変えられた場合
            title = Title_Combobox.Text;
        }
        else
        {//他の曲と合併する場合
            if (MessageBox.Show("アルバム情報を統合します。\nよろしいですか？", "確認", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;
            string prev_id = album_id;
            album_id = AllAlbum[Title_Combobox.SelectedIndex].Id;
            //アルバム情報の統合
            var param = new Collection<MySqlParameter>();
            param.Add(new MySqlParameter("@prev_id", prev_id));
            param.Add(new MySqlParameter("@album_id", album_id));
            DB.NonQuery("UPDATE tracks SET album_id = @album_id WHERE album_id = @prev_id", param);
            DB.NonQuery("UPDATE album_artist SET album_id = @album_id WHERE album_id = @prev_id AND artist_id not in (select artist_id from album_artist where album_id = @album_id)", param);
            DB.NonQuery("DELETE FROM album WHERE album_id = @prev_id", param);
            title = Title_Combobox.Text;
        }
        //読み
        string title_pron = TitlePron_Textbox.Text;
        //クエリ・パラメータ設定
        string album_query = "UPDATE albums SET title = @title, title_pron = @title_pron WHERE album_id = @album_id";
        var album_param = new Collection<MySqlParameter>();
        album_param.Add(new MySqlParameter("@album_id", album_id));
        album_param.Add(new MySqlParameter("@title", title));
        album_param.Add(new MySqlParameter("@title_pron", title_pron));
        //アーティスト
        var reset_query = "Delete from album_artist where album_id = @album_id";
        string artist_query = "INSERT INTO album_artist (album_id, artist_id) VALUES (@album_id, @artist_id)";
        var artist_param = new Collection<MySqlParameter>();
        int i = 0;
        foreach (var artist in AddedArtist)
        {
            artist_query += "(@album_id" + i + ",@artist_id" + i + ")";
            album_param.Add(new MySqlParameter());
        }
    }

    private void Add_Artist(object sender, RoutedEventArgs e)
    {
        Artist Add_Artist = AllArtist[Artist_ComboBox.SelectedIndex];
        if(AddedArtist.Count(x => x.Id == Add_Artist.Id) == 0)
            AddedArtist.Add(Add_Artist);
    }
}