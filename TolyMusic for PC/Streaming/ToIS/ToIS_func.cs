using System.Windows.Controls;
using TolyMusic_for_PC.Queue;

namespace TolyMusic_for_PC.Streaming.ToIS;

public class ToIS_func : Super.PageFunc
{
    public ToIS_func(ViewModel vm, Player player, Main queue, Grid container, StackPanel funcContainer, object main, object PageControler) : base(vm, player, queue, container, funcContainer, main, PageControler)
    {
    }

    protected override void MakeQueue()
    {
        throw new System.NotImplementedException();
    }

    public void MakeTrackList()
    {
        throw new System.NotImplementedException();
    }
}