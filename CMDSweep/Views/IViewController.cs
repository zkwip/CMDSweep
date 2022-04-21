namespace CMDSweep.Views;

interface IViewController
{
    public GameApp App { get; }

    public bool Step();

}