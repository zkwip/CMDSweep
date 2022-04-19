namespace CMDSweep.Views.Menus;

class MenuButton : MenuItem
{
    internal string Subtitle;
    internal MenuButton(string title, string sub = "") : base("[ " + title + " ]") => Subtitle = sub;

    internal override bool HandleItemActions(InputAction ia)
    {
        if (ia == InputAction.Dig) OnValueChanged();
        return ia == InputAction.Dig;
    }

    internal override void RenderItemExtras(int row, MenuVisualizer mv, bool focus) { }
}
