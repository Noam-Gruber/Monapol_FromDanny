namespace MonopolyCommon
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public int Money { get; set; } = 1500;
    }
}