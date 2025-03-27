
namespace MonopolyCommon
{
    /// <summary>
    /// Represents a card in the Monopoly game.
    /// </summary>
    public abstract class Card
    {
        /// <summary>
        /// Gets or sets the description of the card.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Applies the effect of the card to the specified player and game state.
        /// </summary>
        /// <param name="player">The player to apply the effect to.</param>
        /// <param name="gameState">The current state of the game.</param>
        public abstract void ApplyEffect(Player player, GameState gameState);
    }

    /// <summary>
    /// Represents a Chance card in the Monopoly game.
    /// </summary>
    public class ChanceCard : Card
    {
        /// <summary>
        /// Applies the effect of the Chance card to the specified player and game state.
        /// </summary>
        /// <param name="player">The player to apply the effect to.</param>
        /// <param name="gameState">The current state of the game.</param>
        public override void ApplyEffect(Player player, GameState gameState)
        {
            // Example effect of a Chance card
            player.Money += 100; // For example, the player receives $100
        }
    }

    /// <summary>
    /// Represents a Community Chest card in the Monopoly game.
    /// </summary>
    public class CommunityChestCard : Card
    {
        /// <summary>
        /// Applies the effect of the Community Chest card to the specified player and game state.
        /// </summary>
        /// <param name="player">The player to apply the effect to.</param>
        /// <param name="gameState">The current state of the game.</param>
        public override void ApplyEffect(Player player, GameState gameState)
        {
            // Example effect of a Community Chest card
            player.Money -= 50; // For example, the player pays $50
        }
    }
}
