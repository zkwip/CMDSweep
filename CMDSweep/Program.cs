using System;

namespace CMDSweep
{
    class Program
    {
        static void Main(string[] args)
        {
            IRenderer cmdr = new WinCMDRenderer();
            Game g = new Game(cmdr);
            Console.Read();
        }
    }
}
