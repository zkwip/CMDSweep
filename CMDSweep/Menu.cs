using System;
using System.Collections.Generic;

namespace CMDSweep
{
    public class MenuVisualizer
    {
        internal int prefixCol = 2;
        internal int textCol = 4;
        internal int optionCol = 10;
        internal int optionTextCol = 10;
        internal int optionTextEndCol = 10;
        internal int colsNeeded = 0;
        internal int optionWidth = 0;

        internal int titleRow = 0;
        internal int textRow = 0;
        internal int rowScale = 1;

        internal IRenderer Renderer { get => game.Renderer; }
        internal GameApp game;

        public StyleData MenuTextStyle { get; internal set; }
        public StyleData HighlightStyle { get; internal set; }


        public MenuList currentList;

        internal Dictionary<string,ConsoleColor> Colors { get => game.Settings.Colors; }

        public MenuVisualizer(GameApp g)
        {
            game = g;
            MenuTextStyle = new StyleData(Colors["menu-fg"], Colors["menu-bg"]);
            HighlightStyle = new StyleData(Colors["menu-fg-highlight"], Colors["menu-bg-highlight"]);
           
            currentList = new MenuList("Main Menu");
            currentList.Add(new MenuChoice<int>("Number", new List<int> { 1, 2, 3, 4, 5 }, x => x.ToString()));
            currentList.Add(new MenuChoice<Difficulty>("Difficulty", game.Settings.Difficulties, x => x.Name));
            currentList.Add(new MenuText("Button 1", "Col 2"));
            currentList.Add(new MenuText("Button 2", "Hoi"));

        }

        public bool Visualize(RefreshMode mode)
        {
            Renderer.ClearScreen(MenuTextStyle);
            Renderer.HideCursor(MenuTextStyle);
            Renderer.SetTitle(game.Settings.Texts["menu-title"]);
            UpdateMeasurements();

            if (currentList == null) return false;
            VisualizeList(currentList);

            return true;
        }

        private void UpdateMeasurements()
        {
            Dictionary<string, int> dims = game.Settings.Dimensions;

            optionWidth = dims["menu-col2-width"];
            colsNeeded = dims["menu-indent"] + dims["menu-col1-width"] + optionWidth + 2 * dims["menu-box-padding"];

            prefixCol = Math.Max(0, Renderer.Bounds.Width - colsNeeded) / 2;
            textCol = prefixCol + dims["menu-indent"];
            optionCol = textCol + dims["menu-col1-width"];
            optionTextCol = optionCol + dims["menu-box-padding"];
            optionTextEndCol = optionCol + optionWidth + dims["menu-box-padding"];

            int rowsNeeded = 2 + dims["menu-title-space"] + dims["menu-row-scale"] * (dims["menu-rows"] - 1);
            titleRow = Math.Max(0, Renderer.Bounds.Height - rowsNeeded) / 2;
            textRow = titleRow + 1 + dims["menu-title-space"];
        }

        private void VisualizeList(MenuList list)
        {
            RenderTitle(list.Title);
            for (int i = 0; i < list.Items.Count; i++)
                list.Items[i].RenderItem(MapIndexToRow(i), this, list.Index == i);
        }

        private int MapIndexToRow(int i) => textRow + game.Settings.Dimensions["menu-row-scale"] * i; 
        private void RenderTitle(string title) => Renderer.PrintAtTile(titleRow, textCol, MenuTextStyle, title);

        public string centerAlign(string text, int length)
        {
            int offset = (length - text.Length) / 2;
            text += "".PadRight(offset);
            return text.PadLeft(length);
        }
    }

    public class MenuList
    {
        public List<MenuItem> Items{ get; private set; }
        public string Title { get; private set; }
        public int Index { get; private set; }

        public int SelectNext()
        {
            if (Items.Count == 0) return Index;
            for (int i = 0; i < 100; i++)
            {
                Index++;
                if (Index >= Items.Count) Index -= Items.Count;
                if (Items[Index].selectable) return Index;
            }
            return Index;
        }

        public int SelectPrevious()
        {
            if (Items.Count == 0) return Index;
            for (int i = 0; i < 100; i++)
            {
                Index--;
                if (Index < 0) Index += Items.Count;
                if (Items[Index].selectable) return Index;
            }
            return Index;
        }

        public MenuList(string title)
        {
            Items = new List<MenuItem>();
            Title = title;
            Index = 0;
        }

        public void Add(MenuItem item)
        {
            Items.Add(item);
            item.BindParent(this);
        }
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
        internal string Title;
        internal abstract void RenderItemExtras(int row, MenuVisualizer mv, bool focus);
        internal MenuList Parent;
        internal readonly bool selectable = true;
        public event EventHandler ValueChanged;

        internal virtual void OnValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);

        internal MenuItem(string title)
        {
            Title = title;
        }

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

        internal void RenderItem(int row, MenuVisualizer mv, bool focus)
        {
            string pref = focus ? mv.game.Settings.Texts["menu-item-prefix-selected"] : mv.game.Settings.Texts["menu-item-prefix"];

            mv.Renderer.ClearScreen(mv.MenuTextStyle, row, mv.textCol, mv.colsNeeded);
            mv.Renderer.PrintAtTile(row, mv.prefixCol, mv.MenuTextStyle, pref);
            mv.Renderer.PrintAtTile(row, mv.textCol, mv.MenuTextStyle, Title);
            RenderItemExtras(row, mv, focus);
        }

        internal void BindParent(MenuList menuList) => Parent = menuList;
    }

    class MenuText : MenuItem
    {
        internal string Subtitle;
        internal MenuText(string title, string sub = "") : base(title)
        {
            Subtitle = sub;
        }

        // todo: button actions

        internal override void RenderItemExtras(int row, MenuVisualizer mv, bool focus)
        {
            StyleData s = focus ? mv.HighlightStyle : mv.MenuTextStyle;
            mv.Renderer.PrintAtTile(row, mv.optionTextCol, s, Subtitle);
        }
    }

    class MenuChoice<TOption> : MenuItem
    {
        private List<TOption> opts;
        private int selectedIndex = 0;
        private readonly Func<TOption, string> display;

        internal MenuChoice(string title, List<TOption> options, Func<TOption, string> Display) : base(title)
        {
            opts = new List<TOption>(options);
            display = Display;
            Title = title;
        }

        public TOption Selected => opts[selectedIndex];
        public string SelectedName => display(Selected);
        public int Index { get => selectedIndex; set => SetIndex(value); }

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

        override internal void RenderItemExtras(int row, MenuVisualizer mv, bool focus)
        {
            string text = mv.centerAlign(display(opts[selectedIndex]),mv.optionWidth);
            StyleData styl = focus ? mv.HighlightStyle : mv.MenuTextStyle;

            mv.Renderer.PrintAtTile(row, mv.optionCol, mv.MenuTextStyle, mv.game.Settings.Texts["menu-choice-left"]);
            mv.Renderer.PrintAtTile(row, mv.optionTextCol, styl, text);
            mv.Renderer.PrintAtTile(row, mv.optionTextEndCol, mv.MenuTextStyle, mv.game.Settings.Texts["menu-choice-right"]);
        }

    }
}
