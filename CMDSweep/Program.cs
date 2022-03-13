namespace CMDSweep;
class Program
{
    static void Main(string[] args)
    {
        IRenderer cmdr = new WinCMDRenderer();
        _ = new GameApp(cmdr);
    }
}
