﻿using System.Windows.Controls;

namespace TolyMusic_for_PC.Super
{
    public abstract class Main
    {
        //protected変数
        protected ViewModel vm;
        protected Player player;
        protected Queue queue;
        protected Grid container;
        protected StackPanel func_container;
        
        public Main(ViewModel vm, Player player, Queue queue, Grid container, StackPanel funcContainer)
        {
            this.vm = vm;
            this.player = player;
            this.queue = queue;
            this.container = container;
            this.func_container = funcContainer;
        }

        public abstract void Init();
    }
}