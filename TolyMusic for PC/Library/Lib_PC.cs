using System.Windows.Controls;

namespace TolyMusic_for_PC.Library;

public class Lib_PC
{
    //private変数
    private ViewModel vm;
    private Player player;
    private Queue queue;
    private Grid container;
    private StackPanel func_container;
    private Main main;
    private bool loaded = false;
    //コンストラクタ
    public Lib_PC(ViewModel vm, Player player, Queue queue, Grid container, StackPanel funcContainer)
    {
        this.vm = vm;
        this.player = player;
        this.queue = queue;
        this.container = container;
        this.func_container = funcContainer;
        //初期化
        main = new Main(vm, player, queue, container, func_container);
    }
    //ページ遷移
    public void Go(string page)
    {
        if (!loaded)
        {
            main.Init();
            loaded = true;
        }
        switch (page)
        {
            case "tracks":
                vm.Tracks = main.GetTracks();
                break;
            case "albums":
                break;
            case "artists":
                break;
        }
    }
}