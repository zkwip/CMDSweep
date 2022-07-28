namespace Zkwip.Sweep.Board
{
    public class ChunkInfo
    {
        public readonly int Id;
        public int ChunkToLeft { get;  private set; }
        public int ChunkToRight { get; private set; }
        public int ChunkAbove { get; private set; }
        public int ChunkBelow { get; private set; }

        public readonly Board Board;
    }
}