namespace CMDSweep.Layout.Text;

internal class Padding
{
    internal static string PadText(string text, int length, HorzontalAlignment alignment)
    {
        int padding = length - text.Length;

        if (padding == 0)
            return text;

        int offset = (int)alignment * padding / 2;

        if (padding < 0)
            return text.Substring(-offset, length);

        text = "".PadRight(offset) + text;
        return text.PadRight(length);

    }
}
