namespace MonopolyServer
{
    public class BoardSpace
    {
        public int Position { get; set; }
        public string Name { get; set; }
        public int PurchasePrice { get; set; }
        public int RentPrice { get; set; }
        public bool IsSpecial { get; set; }
        public bool IsOwned => OwnedByPlayerId != null;
        public string OwnedByPlayerId { get; set; }
        public bool IsChance { get; set; } = false;
        public bool IsCommunityChest { get; set; } = false;

        public BoardSpace(int position, string name, int purchasePrice, int rentPrice, bool isSpecial = false)
        {
            Position = position;
            Name = name;
            PurchasePrice = purchasePrice;
            RentPrice = rentPrice;
            IsSpecial = isSpecial;
        }
    }

}
