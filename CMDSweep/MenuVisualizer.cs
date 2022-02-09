using System;
using System.Collections.Generic;

namespace CMDSweep
{
    public class MenuVisualizer
    {
        internal int textCol = 0;
        internal int optionCol = 0;
        internal IRenderer renderer;

        public StyleData TextStyle { get; internal set; }
        public StyleData BackgroundStyle { get; internal set; }

        public MenuList currentList;

        void Visualize(RefreshMode mode)
        {

        }
    }

    public class MenuList
    {
        public List<MenuItem> Items{ get; private set; }
        public string Title { get; private set; }
        public int Index { get; private set; }
    }

    enum MenuAction
    {
        None, 

        NextItem,
        PreviousItem,
        StartGame,
        CloseAction,
    }

    public abstract class MenuItem
    {
        internal abstract void RenderItem(int row, MenuVisualizer mv);
        internal MenuAction HandleDefaultMenuAction(InputAction ia)
        {
            switch (ia)
            {
                case InputAction.Up:
                    return MenuAction.PreviousItem;
                case InputAction.Down:
                    return MenuAction.NextItem;
                default:
                    return MenuAction.None;
            }
        }

    }

    class MenuChoice<TOption> : MenuItem
    {
        private List<TOption> opts;
        private int selectedIndex = 0;
        private readonly Func<TOption, string> display;

        internal MenuChoice(string title, List<TOption> options, Func<TOption, string> Display)
        {
            opts = new List<TOption>(options);
            display = Display;
            Title = title;
        }

        public TOption Selected => opts[selectedIndex];
        public string SelectedName => display(Selected);
        public int Index { get => selectedIndex; set => SetIndex(value); }
        public string Title { get; private set; }

        private void SetIndex(int value)
        {
            selectedIndex = 0;
            if (opts.Count == 0) return;
            selectedIndex = value;
            while (selectedIndex >= opts.Count) selectedIndex -= opts.Count;
            while (selectedIndex < 0) selectedIndex += opts.Count;
        }

        public void Select(TOption option)
        {
            int id = opts.IndexOf(option);
            selectedIndex = id;
        }

        override internal void RenderItem(int row, MenuVisualizer mv)
        {
            IRenderer renderer = mv.renderer;
            renderer.PrintAtTile(row, mv.optionCol, mv.TextStyle, Title);
            renderer.ClearScreen(mv.BackgroundStyle, row);
        }

    }
}
