using System;

namespace CMDSweep
{
    class Program
    {
        static void Main(string[] args)
        {
            IRenderer cmdr = new WinCMDRenderer();
            GameApp g = new GameApp(cmdr);
        }
    }
}
