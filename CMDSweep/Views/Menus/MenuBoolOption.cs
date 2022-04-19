using System.Collections.Generic;

namespace CMDSweep.Views.Menus;

class MenuBoolOption : MenuChoice<bool>
{
    public MenuBoolOption(string title) : base(title, new List<bool>() { false, true }, x => x ? "Yes" : "No") { }
}
