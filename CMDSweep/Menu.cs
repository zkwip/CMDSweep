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
        public StyleData FocusBoxStyle { get; internal set; }
        public StyleData FocusTitleStyle { get; internal set; }
        public MenuItem SelectedItem { get => CurrentList.Items[CurrentList.Index]; }
        public MenuList CurrentList => game.currentMenuList;
        internal Dictionary<string, ConsoleColor> Colors { get => game.Settings.Colors; }

        public MenuVisualizer(GameApp g)
        {
            game = g;
            MenuTextStyle = new StyleData(Colors["menu-fg"], Colors["menu-bg"]);
            FocusBoxStyle = new StyleData(Colors["menu-fg-highlight-box"], Colors["menu-bg-highlight-box"]);
            FocusTitleStyle = new StyleData(Colors["menu-fg-highlight-title"], Colors["menu-bg-highlight-title"]);

        }

        public bool Visualize(RefreshMode mode)
        {
            Renderer.ClearScreen(MenuTextStyle);
            Renderer.HideCursor(MenuTextStyle);
            Renderer.SetTitle(game.Settings.Texts["menu-title"]);
            UpdateMeasurements();

            if (CurrentList == null) return false;
            VisualizeList(CurrentList);

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

        public string CenterAlign(string text, int length)
        {
            int offset = (length - text.Length) / 2;
            text += "".PadRight(offset);
            return text.PadLeft(length);
        }
    }

    public class MenuList
    {
        public MenuList ParentMenu;
        public GameApp Game;

        public List<MenuItem> Items { get; private set; }
        public string Title { get; private set; }
        public int Index { get; private set; }
        public MenuItem SelectedItem { get => Items[Index]; }

        public int SelectNext()
        {
            if (Items.Count == 0) return Index;
            for (int i = 0; i < 100; i++)
            {
                Index++;
                if (Index >= Items.Count) Index -= Items.Count;
                if (SelectedItem.Selectable) return Index;
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
                if (SelectedItem.Selectable) return Index;
            }
            return Index;
        }

        public MenuList(string title, GameApp game)
        {
            Game = game;
            Items = new List<MenuItem>();
            Title = title;
            Index = 0;
        }

        public void Add(MenuItem item)
        {
            Items.Add(item);
            item.BindParent(this);
        }

        internal bool HandleInput(InputAction ia)
        {
            if (ia == InputAction.Quit)
            {
                if (ParentMenu == null) return false;
                else Game.OpenMenu(ParentMenu);
            }

            if (SelectedItem != null) SelectedItem.HandleDefaultMenuAction(ia);
            return true;
        }
    }

    public abstract class MenuItem
    {
        internal string Title;
        internal abstract void RenderItemExtras(int row, MenuVisualizer mv, bool focus);
        internal abstract bool HandleOtherActions(InputAction ia);
        internal MenuList Parent;
        internal bool Selectable = true;
        public event EventHandler ValueChanged;

        internal virtual void OnValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);

        internal MenuItem(string title)
        {
            Title = title;
        }

        internal bool HandleDefaultMenuAction(InputAction ia)
        {
            switch (ia)
            {
                case InputAction.Up:
                    Parent.SelectPrevious();
                    return true;
                case InputAction.Down:
                    Parent.SelectNext();
                    return true;
                default:
                    return HandleOtherActions(ia);
            }
        }

        internal void RenderItem(int row, MenuVisualizer mv, bool focus)
        {
            string pref = focus ? mv.game.Settings.Texts["menu-item-prefix-selected"] : mv.game.Settings.Texts["menu-item-prefix"];
            StyleData styl = focus ? mv.FocusTitleStyle : mv.MenuTextStyle;

            mv.Renderer.ClearScreen(mv.MenuTextStyle, row, mv.textCol, mv.colsNeeded);

            // chevron
            //mv.Renderer.PrintAtTile(row, mv.prefixCol, styl, pref);

            mv.Renderer.PrintAtTile(row, mv.textCol, styl, Title);
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
            Selectable = false;
        }

        internal override bool HandleOtherActions(InputAction ia) => false;

        internal override void RenderItemExtras(int row, MenuVisualizer mv, bool focus)
        {
            StyleData s = focus ? mv.FocusBoxStyle : mv.MenuTextStyle;
            mv.Renderer.PrintAtTile(row, mv.optionTextCol, mv.MenuTextStyle, mv.CenterAlign(Subtitle, mv.optionWidth));
        }
    }

    class MenuButton : MenuItem
    {
        internal string Subtitle;
        internal MenuButton(string title, string sub = "") : base("[ " + title + " ]") => Subtitle = sub;

        internal override bool HandleOtherActions(InputAction ia)
        {
            if (ia == InputAction.Dig) OnValueChanged();
            return (ia == InputAction.Dig);
        }

        internal override void RenderItemExtras(int row, MenuVisualizer mv, bool focus)
        {
            //StyleData s = focus ? mv.FocusBoxStyle : mv.MenuTextStyle;
            //mv.Renderer.PrintAtTile(row, mv.textCol, s, Title);
        }
    }

    class MenuChoice<TOption> : MenuItem
    {
        private List<TOption> Options;
        private int SelectedIndex = 0;
        private readonly Func<TOption, string> Display;
        public bool Enabled = true;

        public MenuChoice(string title, List<TOption> options, Func<TOption, string> Display) : base(title)
        {
            Options = new List<TOption>(options);
            this.Display = Display;
            Title = title;
        }

        public TOption SelectedOption => Options[SelectedIndex];
        public string SelectedName => Display(SelectedOption);
        public int Index { get => SelectedIndex; set => SetIndex(value); }

        internal override bool HandleOtherActions(InputAction ia)
        {
            switch (ia)
            {
                case InputAction.Right:
                    Index++;
                    return true;
                case InputAction.Left:
                    Index--;
                    return true;
            }

            return false;
        }

        private void SetIndex(int value)
        {
            SelectedIndex = 0;
            if (Options.Count == 0) return;

            SelectedIndex = value;
            while (SelectedIndex >= Options.Count) SelectedIndex -= Options.Count;
            while (SelectedIndex < 0) SelectedIndex += Options.Count;
        }

        public bool Select(TOption option)
        {
            int id = Options.IndexOf(option);
            if (id != -1) SelectedIndex = id;
            return (id != -1);
        }

        override internal void RenderItemExtras(int row, MenuVisualizer mv, bool focus)
        {
            string text = mv.CenterAlign(SelectedName, mv.optionWidth);
            StyleData styl = focus ? mv.FocusBoxStyle : mv.MenuTextStyle;

            if (Enabled)
            {
                mv.Renderer.PrintAtTile(row, mv.optionCol, mv.MenuTextStyle, mv.game.Settings.Texts["menu-choice-left"]);
                mv.Renderer.PrintAtTile(row, mv.optionTextCol, styl, text);
                mv.Renderer.PrintAtTile(row, mv.optionTextEndCol, mv.MenuTextStyle, mv.game.Settings.Texts["menu-choice-right"]);
            }
            else
            {
                mv.Renderer.PrintAtTile(row, mv.optionTextCol, styl, text);
            }
        }
    }

    class MenuNumberRange : MenuChoice<int>
    {
        public readonly int Min;
        public readonly int Max;

        public MenuNumberRange(string title, int min, int max) : base(title, Range(min, max), x => x.ToString())
        {
            Min = min;
            Max = max;
        }

        static List<int> Range(int min, int max)
        {
            List<int> res = new List<int>();
            for (int i = min; i <= max; i++) res.Add(i);
            return res;
        }

        internal override bool HandleOtherActions(InputAction ia)
        {
            switch (ia)
            {
                case InputAction.Right:
                    Index++;
                    return true;
                case InputAction.Left:
                    Index--;
                    return true;
                case InputAction.Clear:
                    return TryBackspace();

                case InputAction.One: return TryAdd(1);
                case InputAction.Two: return TryAdd(2);
                case InputAction.Three: return TryAdd(3);
                case InputAction.Four: return TryAdd(4);
                case InputAction.Five: return TryAdd(5);
                case InputAction.Six: return TryAdd(6);
                case InputAction.Seven: return TryAdd(7);
                case InputAction.Eight: return TryAdd(8);
                case InputAction.Nine: return TryAdd(9);
                case InputAction.Zero: return TryAdd(0);
            }

            return false;
        }

        private bool TryBackspace()
        {
            int num = SelectedOption;
            int newnum = num / 10;
            if (!Select(newnum)) return Select(Min);
            return true;
        }

        private bool TryAdd(int digit)
        {
            int num = SelectedOption;
            int newnum = num * 10 + digit;

            if (!Select(newnum))
            {
                //Should remove the first digit?
                newnum = newnum % (int)Math.Pow(10, Math.Floor(Math.Log10(newnum)));
                if (!Select(newnum)) return Select(Max);
            }
            return true;
        }
    }

    class MenuBoolOption : MenuChoice<bool>
    {
        public MenuBoolOption(string title) : base(title, new List<bool>() { false, true }, x => x ? "Yes" : "No") { }
    }
}
