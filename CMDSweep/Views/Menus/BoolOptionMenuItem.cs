using CMDSweep.Data;
using System.Collections.Generic;

namespace CMDSweep.Views.Menus;

class BoolOptionMenuItem : OptionMenuItem<bool>
{
    public BoolOptionMenuItem(string title, GameSettings settings) : base(title, new List<bool>() { false, true }, x => x ? "Yes" : "No", settings) { }
}
