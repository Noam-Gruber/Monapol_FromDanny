using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyServer
{
    public class BoardSpace
    {
        public string Name { get; set; }
        public int PurchasePrice { get; set; }   // מחיר קנייה
        public int RentPrice { get; set; }        // מחיר שכירות
        public bool IsSpecial { get; set; }       // האם השטח הוא שטח מיוחד (כמו "Go", "Jail" וכו')

        // מבנה שטח רגיל או מיוחד
        public BoardSpace(string name, int purchasePrice = 0, int rentPrice = 0, bool isSpecial = false)
        {
            Name = name;
            PurchasePrice = purchasePrice;
            RentPrice = rentPrice;
            IsSpecial = isSpecial;
        }
    }
}
