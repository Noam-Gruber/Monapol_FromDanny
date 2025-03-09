namespace MonopolyServer
{
    public class Board
    {
        public List<BoardSpace> Spaces { get; private set; }  // רשימת השטחים על הלוח
        public Dictionary<string, int> PlayerPositions { get; private set; }  // מיקום השחקנים בלוח

        public Board()
        {
            PlayerPositions = new Dictionary<string, int>();

            Spaces = new List<BoardSpace>();

            // יצירת שטחים עם מחירים ושמות - כאן נממש את השטחים הרגילים והמיוחדים
            Spaces.Add(new BoardSpace("Go", 0, 0, true));  // דוגמת שטח מיוחד
            Spaces.Add(new BoardSpace("Mediterranean Avenue", 60, 2));
            Spaces.Add(new BoardSpace("Community Chest", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace("Baltic Avenue", 60, 4));
            Spaces.Add(new BoardSpace("Income Tax", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace("Reading Railroad", 200, 25));
            Spaces.Add(new BoardSpace("Oriental Avenue", 100, 6));
            Spaces.Add(new BoardSpace("Chance", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace("Vermont Avenue", 100, 6));
            Spaces.Add(new BoardSpace("Connecticut Avenue", 120, 8));
            Spaces.Add(new BoardSpace("Jail", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace("St. Charles Place", 140, 10));
            Spaces.Add(new BoardSpace("Electric Company", 150, 10));
            Spaces.Add(new BoardSpace("States Avenue", 140, 10));
            Spaces.Add(new BoardSpace("Virginia Avenue", 160, 12));
            Spaces.Add(new BoardSpace("States Railroad", 200, 25));
            Spaces.Add(new BoardSpace("St. James Place", 180, 14));
            Spaces.Add(new BoardSpace("Community Chest", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace("Tennessee Avenue", 180, 14));
            Spaces.Add(new BoardSpace("New York Avenue", 200, 16));
            Spaces.Add(new BoardSpace("Free Parking", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace("Kentucky Avenue", 220, 18));
            Spaces.Add(new BoardSpace("Chance", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace("Indiana Avenue", 220, 18));
            Spaces.Add(new BoardSpace("Illinois Avenue", 240, 20));
            Spaces.Add(new BoardSpace("B&O Railroad", 200, 25));
            Spaces.Add(new BoardSpace("Atlantic Avenue", 260, 22));
            Spaces.Add(new BoardSpace("Ventnor Avenue", 260, 22));
            Spaces.Add(new BoardSpace("Water Works", 150, 10));
            Spaces.Add(new BoardSpace("Marvin Gardens", 280, 24));
            Spaces.Add(new BoardSpace("Go to Jail", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace("Pacific Avenue", 300, 26));
            Spaces.Add(new BoardSpace("North Carolina Avenue", 300, 26));
            Spaces.Add(new BoardSpace("Community Chest", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace("Pennsylvania Railroad", 200, 25));
            Spaces.Add(new BoardSpace("Chestnut Street", 320, 28));
            Spaces.Add(new BoardSpace("Marvin Gardens", 320, 28));
            Spaces.Add(new BoardSpace("Park Place", 350, 35));
            Spaces.Add(new BoardSpace("Luxury Tax", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace("Boardwalk", 400, 50));
        }

        // פונקציה לעדכון המיקום של שחקן בלוח
        public void UpdatePlayerPosition(string playerId, int newPosition)
        {
            if (PlayerPositions.ContainsKey(playerId))
            {
                PlayerPositions[playerId] = newPosition;
            }
            else
            {
                PlayerPositions.Add(playerId, newPosition);
            }
        }

        // פונקציה להחזרת המיקום של שחקן בלוח (כולל המידע על השטח)
        public string GetPlayerPositionDisplay(string playerId)
        {
            if (PlayerPositions.ContainsKey(playerId))
            {
                int position = PlayerPositions[playerId];
                BoardSpace space = Spaces[position];
                return $"{space.Name} - Purchase Price: {space.PurchasePrice}, Rent Price: {space.RentPrice}";
            }
            return "Player not on board";
        }
    }
}
