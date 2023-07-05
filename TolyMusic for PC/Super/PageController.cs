using System.Windows.Controls;

namespace TolyMusic_for_PC.Super
{
    public abstract class PageController
    {
        protected ViewModel vm;
        protected Player player;
        protected Queue queue;
        protected Grid container;
        protected StackPanel func_container;

        public PageController(ViewModel vm, Player player, Queue queue, Grid container, StackPanel funcContainer)
        {
            this.vm = vm;
            this.player = player;
            this.queue = queue;
            this.container = container;
            func_container = funcContainer;
        }
        
        public abstract void Go(string page);
    }
}