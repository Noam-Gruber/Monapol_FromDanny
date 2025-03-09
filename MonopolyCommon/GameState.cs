using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyCommon
{
    public class GameState
    {
        public List<Player> Players { get; set; } = new List<Player>();
        public int CurrentPlayerIndex { get; set; }
    }
}
