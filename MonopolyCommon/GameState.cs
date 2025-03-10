using MonopolyServer;
using System.Collections.Generic;

namespace MonopolyCommon
{
    public class GameState
    {
        public List<Player> Players { get; set; } = new List<Player>();
        public int CurrentPlayerIndex { get; set; }
        public Board Board { get; set; } = new Board();
    }
}
