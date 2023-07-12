using System.Windows.Controls;
using TolyMusic_for_PC.Queue;

namespace TolyMusic_for_PC.Streaming.ToIS;

public class ToIS_PC : Super.PageController
{
    private string curt_page;
    private ToIS_func func;
    private ToIS_Main main;
    
    public ToIS_PC(ViewModel vm, Player player, Main queue, Grid container, StackPanel funcContainer) : base(vm, player, queue, container, funcContainer)
    {
        func = new ToIS_func(vm, player, queue, container, funcContainer, this, this);
        main = new ToIS_Main(vm, player, queue, container, funcContainer);
    }

    public override void Go(string page)
    {
        curt_page = page;
        
        //その他ボタンの作成
        //順再生
        Button playall = new Button();
        playall.Content = "順再生";
        playall.Click += func.PlayAll;
        func_container.Children.Add(playall);
        //シャッフル再生
        Button Shuffleall = new Button();
        Shuffleall.Content = "シャッフル再生";
        Shuffleall.Click += func.ShuffleAll;
        func_container.Children.Add(Shuffleall);
        //検索テキストボックスの作成
        TextBox search = new TextBox();
        search.Width = 200;
        search.TextChanged += func.Search;
        func_container.Children.Add(search);
        //ページ遷移
        switch (page)
        {
            case "tracks":
                vm.Tracks = main.GetTracks("",ViewModel.TypeEnum.All);
                vm.Curttype = ViewModel.TypeEnum.Track;
                func.MakeTrackList();
                break;
        }
    }
}