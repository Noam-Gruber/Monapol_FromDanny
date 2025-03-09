using System.Collections.Generic;

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
            Spaces.Add(new BoardSpace(0, "Go", 0, 0, true));  // דוגמת שטח מיוחד
            Spaces.Add(new BoardSpace(1, "Mediterranean Avenue", 60, 2));
            Spaces.Add(new BoardSpace(2, "Community Chest", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace(3, "Baltic Avenue", 60, 4));
            Spaces.Add(new BoardSpace(4, "Income Tax", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace(5, "Reading Railroad", 200, 25));
            Spaces.Add(new BoardSpace(6, "Oriental Avenue", 100, 6));
            Spaces.Add(new BoardSpace(7, "Chance", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace(8, "Vermont Avenue", 100, 6));
            Spaces.Add(new BoardSpace(9, "Connecticut Avenue", 120, 8));
            Spaces.Add(new BoardSpace(10, "Jail", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace(11, "St. Charles Place", 140, 10));
            Spaces.Add(new BoardSpace(12, "Electric Company", 150, 10));
            Spaces.Add(new BoardSpace(13, "States Avenue", 140, 10));
            Spaces.Add(new BoardSpace(14, "Virginia Avenue", 160, 12));
            Spaces.Add(new BoardSpace(15,"States Railroad", 200, 25));
            Spaces.Add(new BoardSpace(16, "St. James Place", 180, 14));
            Spaces.Add(new BoardSpace(17, "Community Chest", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace(18, "Tennessee Avenue", 180, 14));
            Spaces.Add(new BoardSpace(19, "New York Avenue", 200, 16));
            Spaces.Add(new BoardSpace(20, "Free Parking", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace(21, "Kentucky Avenue", 220, 18));
            Spaces.Add(new BoardSpace(22, "Chance", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace(23, "Indiana Avenue", 220, 18));
            Spaces.Add(new BoardSpace(24, "Illinois Avenue", 240, 20));
            Spaces.Add(new BoardSpace(25, "B&O Railroad", 200, 25));
            Spaces.Add(new BoardSpace(26, "Atlantic Avenue", 260, 22));
            Spaces.Add(new BoardSpace(27, "Ventnor Avenue", 260, 22));
            Spaces.Add(new BoardSpace(28, "Water Works", 150, 10));
            Spaces.Add(new BoardSpace(29, "Marvin Gardens", 280, 24));
            Spaces.Add(new BoardSpace(30, "Go to Jail", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace(31, "Pacific Avenue", 300, 26));
            Spaces.Add(new BoardSpace(32, "North Carolina Avenue", 300, 26));
            Spaces.Add(new BoardSpace(33, "Community Chest", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace(34, "Pennsylvania Railroad", 200, 25));
            Spaces.Add(new BoardSpace(35, "Chestnut Street", 320, 28));
            Spaces.Add(new BoardSpace(36, "Marvin Gardens", 320, 28));
            Spaces.Add(new BoardSpace(37, "Park Place", 350, 35));
            Spaces.Add(new BoardSpace(38, "Luxury Tax", 0, 0, true));  // שטח מיוחד
            Spaces.Add(new BoardSpace(39, "Boardwalk", 400, 50));
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
