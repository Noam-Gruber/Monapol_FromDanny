using MonopolyCommon;

namespace MonopolyServer
{
    public class GameState
    {
        public List<Player> Players { get; set; } = new List<Player>();
        public int CurrentPlayerIndex { get; set; }
    }
}
