using System.Collections.Generic;

namespace CMDSweep.Data;

class SaveData
{
    public List<Difficulty> Difficulties;
    public Difficulty CurrentDifficulty;
    public string PlayerName;

    public SaveData() { }

    public SaveData(List<Difficulty> difficulties)
    {
        Difficulties = new List<Difficulty>(difficulties);
        CurrentDifficulty = Difficulties[0];
        PlayerName = "You";
    }
}
