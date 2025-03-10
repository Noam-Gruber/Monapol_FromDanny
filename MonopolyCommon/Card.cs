using System;
using System.Collections.Generic;
using System.Text;

namespace MonopolyCommon
{
    public abstract class Card
    {
        public string Description { get; set; }
        public abstract void ApplyEffect(Player player, GameState gameState);
    }

    public class ChanceCard : Card
    {
        public override void ApplyEffect(Player player, GameState gameState)
        {
            // דוגמה לאפקט של קלף Chance
            player.Money += 100; // למשל, השחקן מקבל 100$
        }
    }

    public class CommunityChestCard : Card
    {
        public override void ApplyEffect(Player player, GameState gameState)
        {
            // דוגמה לאפקט של קלף Community Chest
            player.Money -= 50; // למשל, השחקן משלם 50$
        }
    }
}
