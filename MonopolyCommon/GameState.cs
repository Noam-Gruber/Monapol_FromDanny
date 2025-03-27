using MonopolyServer;
using System.Collections.Generic;

namespace MonopolyCommon
{
    /// <summary>
    /// Represents the state of the game.
    /// </summary>
    public class GameState
    {
        /// <summary>
        /// Gets or sets the list of players in the game.
        /// </summary>
        public List<Player> Players { get; set; } = new List<Player>();

        /// <summary>
        /// Gets or sets the index of the current player.
        /// </summary>
        public int CurrentPlayerIndex { get; set; }

        /// <summary>
        /// Gets or sets the game board.
        /// </summary>
        public Board Board { get; set; } = new Board();

        /// <summary>
        /// Gets or sets the game ID.
        /// </summary>
        public string GameId { get; set; } = string.Empty;
    }
}
