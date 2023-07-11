using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using TolyMusic_for_PC.Library;
using Guid = System.Guid;

namespace TolyMusic_for_PC.Property;

public partial class TrackPreoperty : PropertyWindow
{
    public TrackPreoperty(ViewModel vm) : base(vm)
    {
        Curt_num = vm.Tracks.ToList().FindIndex(x => x.Id == vm.Othermenu_Id);
        InitializeComponent();
    }
    protected override void Load()
    {
        //変更前のID
        prev_id = vm.Tracks[Curt_num].Id;
        //タイトル指定
        Title.Content = vm.Tracks[Curt_num].Title;
        Title_ComboBox.ItemsSource = AllTrack;
        Title_ComboBox.DisplayMemberPath = "Title";
        Title_ComboBox.SelectedIndex = AllTrack.ToList().FindIndex(x => x.Id == vm.Tracks[Curt_num].Id);
        //読み指定
        TitlePron_TextBox.Text = vm.Tracks[Curt_num].Title_pron;
        //アーティスト
        Artist_ComboBox.ItemsSource = AllArtist;
        Artist_ComboBox.DisplayMemberPath = "Name";
        AddedArtist = new ObservableCollection<Artist>(vm.Tracks[Curt_num].Artists);
        Artist_List.ItemsSource = AddedArtist;
        //アルバム
        Album_ComboBox.ItemsSource = AllAlbum;
        Album_ComboBox.DisplayMemberPath = "Title";
        Album_ComboBox.SelectedIndex = AllAlbum.ToList().FindIndex(x => x.Id == vm.Tracks[Curt_num].Album_id);
        //グループ
        Group_ComboBox.ItemsSource = AllArtist;
        Group_ComboBox.DisplayMemberPath = "Name";
        Group_ComboBox.SelectedIndex = AllArtist.ToList().FindIndex(x => x.Id == vm.Tracks[Curt_num].Group_id);
        //トラック番号
        TrackNumber_TextBox.Text = vm.Tracks[Curt_num].TrackNumber.ToString();
    }
    protected override void Send_Data(object sender, RoutedEventArgs e)
    {
        //確認
        if(int.TryParse(TrackNumber_TextBox.Text,out int Tracknum) == false)
        {
            MessageBox.Show("トラック番号が不正です。");
            return;
        }
        string track_id = AllTrack[Title_ComboBox.SelectedIndex].Id;
        string title;
        //タイトル
        if (Title_ComboBox.SelectedIndex == -1 || Regex.IsMatch(track_id,prev_id))
        {//曲名が変えられた場合
            title = Title_ComboBox.Text;
        }
        else
        {
            //他の曲と合併する場合
            if(MessageBox.Show("楽曲情報を統合します。\nよろしいですか？", "確認", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;
            var param = new Collection<MySqlParameter>();
            param.Add(new MySqlParameter("@prev_id", prev_id));
            param.Add(new MySqlParameter("@track_id", track_id));
            //tracksの削除
            DB.NonQuery("delete from tracks where id = @prev_id",param);
            //track_artistの変更
            DB.NonQuery("update track_artist set track_id = @track_id where track_id = @prev_id and artist_id not in (select artist_id from track_artist where track_id = @track_id)", param);
            //locationの変更
            DB.NonQuery("update location set track_id = @track_id where track_id = @prev_id", param);
            //playlist_trackの変更
            DB.NonQuery("update playlist_track set track_id = @track_id where track_id = @prev_id", param);
            //historyの変更
            DB.NonQuery("update history_tracks set track_id = @track_id where track_id = @prev_id", param);
            //title
            title = Title_ComboBox.Text;
        }
        
        //読み
        string title_pron = TitlePron_TextBox.Text;
        
        //アルバム
        string album_id;
        int album_index = Album_ComboBox.SelectedIndex;
        if (String.IsNullOrEmpty(Album_ComboBox.Text))
        {
            album_id = null;
        }
        else if(album_index == -1)
        {//新規アルバムの場合
            album_id = Guid.NewGuid().ToString();
            string album_title = Album_ComboBox.Text;
            //アルバム追加
            var param = new Collection<MySqlParameter>();
            param.Add(new MySqlParameter("@album_id", album_id));
            param.Add(new MySqlParameter("@album_title", album_title));
            DB.NonQuery("insert into album (album_id, album_title) values (@album_id, @album_title)",param);
        }
        else
        {//既存アルバムの場合
            album_id = AllAlbum[album_index].Id;
        }
        
        //作曲者
        string composer_id;
        int composer_index = Composer_ComboBox.SelectedIndex;
        if (String.IsNullOrEmpty(Composer_ComboBox.Text))
        {
            composer_id = null;
        }
        else if (composer_index == -1)
        {//新規作曲者の場合
            composer_id = Guid.NewGuid().ToString();
            string composer_name = Composer_ComboBox.Text;
            //作曲者追加
            DB_Func.AddArtist(composer_id, composer_name);
        }
        else
        {//既存作曲者の場合
            composer_id = AllArtist[composer_index].Id;
        }
        
        //グループ
        string group_id;
        int group_index = Group_ComboBox.SelectedIndex;
        if (String.IsNullOrEmpty(Group_ComboBox.Text))
        {
            group_id = null;
        }
        else if (group_index == -1)
        {//新規グループの場合
            group_id = Guid.NewGuid().ToString();
            string group_name = Group_ComboBox.Text;
            //グループ追加
            DB_Func.AddArtist(group_id, group_name);
        }
        else
        {//既存グループの場合
            group_id = AllArtist[group_index].Id;
        }
        
        //track書き込み
        var tracks_param = new Collection<MySqlParameter>();
        tracks_param.Add(new MySqlParameter("@track_id", track_id));
        tracks_param.Add(new MySqlParameter("@title", title));
        tracks_param.Add(new MySqlParameter("@title_pron", title_pron));
        tracks_param.Add(new MySqlParameter("@album_id", album_id));
        tracks_param.Add(new MySqlParameter("@composer_id", composer_id));
        tracks_param.Add(new MySqlParameter("@group_id", group_id));
        tracks_param.Add(new MySqlParameter("@track_number", Tracknum));
        //track_artist
        string ta_reset = "delete from track_artist where track_id = @track_id";
        var ta_reset_param = new Collection<MySqlParameter>();
        ta_reset_param.Add(new MySqlParameter("@track_id", track_id));
        string ta_quary = "insert into track_artist (track_id, artist_id) values ";
        var ta_param = new Collection<MySqlParameter>();
        int i = 0;
        foreach (var artist in AddedArtist)
        {
            ta_quary += "(@track_id"+i+", @artist_id"+i+"),";
            ta_param.Add(new MySqlParameter("@track_id"+i, track_id));
            string id = artist.Id;
            if (id == null)
            {//新規アーティストの場合
                id = Guid.NewGuid().ToString();
                string name = artist.Name;
                //アーティスト追加
                DB_Func.AddArtist(id, name);
            }
            ta_param.Add(new MySqlParameter("@artist_id"+i, id));
            i++;
        }
        ta_quary = ta_quary.Substring(0, ta_quary.Length - 1);
        //曲情報の書き込み
        DB.NonQuery("update tracks set track_title = @title, track_title_pron = @title_pron, album_id = @album_id, composer_id = @composer_id, group_id = @group_id, track_num = @track_number where track_id = @track_id", tracks_param);
        DB.NonQuery(ta_reset, ta_reset_param);
        DB.NonQuery(ta_quary, ta_param);
    }

    private void Add_Artist(object sender, RoutedEventArgs e)
    {
        Artist Add_Artist = AllArtist[Artist_ComboBox.SelectedIndex];
        if(AddedArtist.Count(x => x.Id == Add_Artist.Id) == 0)
            AddedArtist.Add(Add_Artist);
    }

    private void DelArtist(object sender, RoutedEventArgs e)
    {
        Button button = sender as Button;
        int index = AddedArtist.IndexOf(AddedArtist.First(a => a.Id == button.Uid));
        AddedArtist.RemoveAt(index);
    }
}