using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using MySql.Data.MySqlClient;
using TolyMusic_for_PC.Library;

namespace TolyMusic_for_PC.Property;

public partial class ArtistProperty : PropertyWindow
{
    public ArtistProperty(ViewModel vm) : base(vm)
    {
        Curt_num = vm.Artists.ToList().FindIndex(x => x.Id == vm.Othermenu_Id);
        InitializeComponent();
    }

    protected override void Load()
    {
        //変更前のID
        prev_id = vm.Artists[Curt_num].Id;
        //タイトル
        Title.Content = vm.Artists[Curt_num].Name;
        //名前
        Name_Combobox.ItemsSource = AllArtist;
        Name_Combobox.DisplayMemberPath = "Name";
        Name_Combobox.SelectedIndex = AllArtist.ToList().FindIndex(x => x.Id == vm.Artists[Curt_num].Id);
        //グループ
        AddedGroup = vm.Artists[Curt_num].Groups;
        Group_List.ItemsSource = AddedGroup;
        Group_ComboBox.ItemsSource = AllArtist;
        Group_ComboBox.DisplayMemberPath = "Name";
    }

    protected override void Send_Data(object sender, RoutedEventArgs e)
    {
        string artist_id = AllArtist[Name_Combobox.SelectedIndex].Id;
        //名前
        string name;
        if(artist_id == prev_id || Name_Combobox.SelectedIndex == -1)
        {
            name = Name_Combobox.Text;
        }
        else
        {
            if(MessageBox.Show("アーティストを統合します。\nよろしいですか？","確認",MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;
            name = AllArtist[Name_Combobox.SelectedIndex].Name;
            var param = new Collection<MySqlParameter>();
            param.Add(new MySqlParameter("@prev_id", prev_id));
            param.Add(new MySqlParameter("@artist_id", artist_id));
            DB.NonQuery("update track_artist set artist_id = @artist_id where artist_id = @prev_id and track_id not in (select track_id from (select track_id from track_artist where artist_id = @artist_id)tmp)", param);
            DB.NonQuery("update album_artist set artist_id = @artist_id where artist_id = @prev_id and album_id not in (select album_id from (select album_id from album_artist where artist_id = @artist_id)tmp)", param);
            DB.NonQuery("update artist_group set artist_id = @artist_id where artist_id = @prev_id and group_id not in (select group_id from (select group_id from artist_group where artist_id = @artist_id)tmp)", param);
            DB.NonQuery("delete from artist where artist_id = @prev_id", param);
        }
        //ベースクエリ
        var artist_query = "update artist set artist_name = @name where artist_id = @id";
        //パラメータ
        var artist_param = new Collection<MySqlParameter>();
        artist_param.Add(new MySqlParameter("@name", name));
        artist_param.Add(new MySqlParameter("@id", artist_id));
        //クエリ実行
        DB.NonQuery(artist_query, artist_param);
    }

    private void Add_Group(object sender, RoutedEventArgs e)
    {
        Artist Add_Group = AllArtist[Group_ComboBox.SelectedIndex];
        if(AddedGroup.Count(x => x.Id == Add_Group.Id) == 0)
            AddedGroup.Add(Add_Group);
    }
}