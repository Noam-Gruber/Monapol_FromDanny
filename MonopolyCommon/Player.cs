using System.Collections.Generic;

namespace MonopolyCommon
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Position { get; set; } = 0;
        public int Money { get; set; } = 1500;
        public string CurrentProperty { get; set; }
        public List<string> OwnedProperties { get; set; } = new List<string>();
    }
}