using MonopolyCommon;

namespace MonopolyServer
{
    public class CardManager
    {
        private Queue<Card> ChanceCards { get; set; }
        private Queue<Card> CommunityChestCards { get; set; }

        public CardManager()
        {
            InitializeChanceCards();
            InitializeCommunityChestCards();
        }

        private void InitializeChanceCards()
        {
            ChanceCards = new Queue<Card>();
            ChanceCards.Enqueue(new ChanceCard { Description = "Collect $100" });
            ChanceCards.Enqueue(new ChanceCard { Description = "Advance to GO" });
        }

        private void InitializeCommunityChestCards()
        {
            CommunityChestCards = new Queue<Card>();
            CommunityChestCards.Enqueue(new CommunityChestCard { Description = "Pay $50 fine" });
            CommunityChestCards.Enqueue(new CommunityChestCard { Description = "Bank error in your favor, collect $200" });
        }

        public Card DrawChanceCard()
        {
            var card = ChanceCards.Dequeue();
            ChanceCards.Enqueue(card);
            return card;
        }

        public Card DrawCommunityChestCard()
        {
            var card = CommunityChestCards.Dequeue();
            CommunityChestCards.Enqueue(card);
            return card;
        }
    }
}
