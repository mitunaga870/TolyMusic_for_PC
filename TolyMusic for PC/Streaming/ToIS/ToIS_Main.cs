using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using TolyMusic_for_PC.Queue;

namespace TolyMusic_for_PC.Streaming.ToIS;

public class ToIS_Main : Super.Main
{
    public ToIS_Main(ViewModel vm) : base(vm)
    {
    }

    public ToIS_Main(ViewModel vm, Player player, Main queue, Grid container, StackPanel funcContainer) : base(vm, player, queue, container, funcContainer)
    {
    }

    public override void Init()
    {
    }

    public ObservableCollection<Track> GetTracks(string id, ViewModel.TypeEnum filter)
    {
        var result = new ObservableCollection<Track>();
        var dbtmp = new Collection<Dictionary<string,string>>();
        switch (filter)
        {
            
        }

        return result;
    }
}