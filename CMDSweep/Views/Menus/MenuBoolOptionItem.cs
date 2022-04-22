using CMDSweep.Data;
using System.Collections.Generic;

namespace CMDSweep.Views.Menus;

class MenuBoolOptionItem : MenuOptionItem<bool>
{
    public MenuBoolOptionItem(string title, GameSettings settings) : base(title, new List<bool>() { false, true }, x => x ? "Yes" : "No", settings) { }
}
