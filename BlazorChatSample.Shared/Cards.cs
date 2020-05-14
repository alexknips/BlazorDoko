using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorChatSample.Shared
{
    public enum CardColor
    {
        Clovers, Pikes, Hearts, Tiles
    }
    public enum CardType
    {
        Nine, Jack, Queen, King, Ten, Ace
    }

    public class Card
    {
        public CardColor cardColor { get; set; }  
        public CardType cardType { get; set; }  
        // private static readonly Image backgroundImage = Cards.green_back;
        // private static readonly Image backgroundImage_rot = Cards.green_back_rot;

        public static int TypeToPoints(CardType c)
        {
            if (c == CardType.Nine) return 0;
            if (c == CardType.Ten) return 10;
            if (c == CardType.Jack) return 2;
            if (c == CardType.Queen) return 3;
            if (c == CardType.King) return 4;
            if (c == CardType.Ace) return 11;
            return -1;
        }
        public int points
        {
            get { return TypeToPoints(cardType); }
        }

        public Card()
        {
            cardColor = CardColor.Clovers;
            cardType = CardType.Ace;
        }

        public Card(string name)
        {
            Card c = GetCardFromName(name);
            this.cardColor = c.cardColor;
            this.cardType = c.cardType;
        }

        public Card(CardColor cardColor, CardType cardType)
        {
            this.cardColor = cardColor;
            this.cardType = cardType;
        }

        public Card(Card c) : this(c.ToString())
        { 
        }

        // public Image GetImage()
        // {
        //     return (Image)Cards.ResourceManager.GetObject(GetCardFilename());
        // }

        public string GetCardFilename()
        {
            return GetCardFilename(this.cardType, this.cardColor);
        }
        public override string ToString()
        {
            return GetCardName(this.cardType, this.cardColor);
        }

        // public static Image GetBackgroundImage()
        // {
        //     return backgroundImage;
        // }
        // public static Image GetBackgroundImageRot()
        // {
        //     return backgroundImage_rot;
        // }

        public bool IsTrump()
        {
            if (cardColor == CardColor.Tiles)
                return true;
            if (cardType == CardType.Jack || cardType == CardType.Queen)
                return true;
            if (cardType == CardType.Ten && cardColor == CardColor.Hearts)
                return true;
            return false;
        }

        public static Card GetCardFromName(string name)
        {
            CardColor cc = CardColor.Clovers;
            CardType ct = CardType.Ace;

            if (name.StartsWith("9"))
                ct = CardType.Nine;
            else if (name.StartsWith("10"))
                ct = CardType.Ten;
            else if (name.StartsWith("J"))
                ct = CardType.Jack;
            else if (name.StartsWith("Q"))
                ct = CardType.Queen;
            else if (name.StartsWith("K"))
                ct = CardType.King;
            else if (name.StartsWith("A"))
                ct = CardType.Ace;

            if (name.EndsWith("C"))
                cc = CardColor.Clovers;
            else if (name.EndsWith("S"))
                cc = CardColor.Pikes;
            else if (name.EndsWith("H"))
                cc = CardColor.Hearts;
            else if (name.EndsWith("D"))
                cc = CardColor.Tiles;
            return new Card(cc, ct);
        }

        public static string GetCardFilename(CardType cardType, CardColor cardColor)
        {
            return GetCardName(cardType, cardColor).Replace("9","_9").Replace("10","_10");
        }

        public static string GetCardName(CardType cardType, CardColor cardColor)
        {
            string cardname = "";
            if (cardType == CardType.Nine) cardname  = "9";
            if (cardType == CardType.Ten)  cardname  = "10";
            if (cardType == CardType.Jack) cardname  = "J";
            if (cardType == CardType.Queen)cardname  = "Q";
            if (cardType == CardType.King) cardname  = "K";
            if (cardType == CardType.Ace)  cardname  = "A";

            if (cardColor == CardColor.Clovers) cardname += "C";
            if (cardColor == CardColor.Pikes) cardname += "S";
            if (cardColor == CardColor.Hearts) cardname += "H";
            if (cardColor == CardColor.Tiles) cardname += "D";

            return cardname;
        }

        public string GetReadableName()
        {
            string cardname = "";
            if (cardColor == CardColor.Clovers) cardname += "Kreuz";
            if (cardColor == CardColor.Pikes) cardname += "Pik";
            if (cardColor == CardColor.Hearts) cardname += "Herz";
            if (cardColor == CardColor.Tiles) cardname += "Caro";

            if (cardType == CardType.Nine) cardname += " 9";
            if (cardType == CardType.Ten) cardname += " 10";
            if (cardType == CardType.Jack) cardname += "-Bube";
            if (cardType == CardType.Queen) cardname += "-Dame";
            if (cardType == CardType.King) cardname += "-König";
            if (cardType == CardType.Ace) cardname += "-As";

            return cardname;
        }

        public static int Compare(Card c1, Card c2)
        {
            if (c1.cardColor == CardColor.Hearts && c1.cardType == CardType.Ten)
                return -1;
            if (c2.cardColor == CardColor.Hearts && c2.cardType == CardType.Ten)
                return 1;

            if (c1.IsTrump() && !c2.IsTrump())
                return -1;
            if (!c1.IsTrump() && c2.IsTrump())
                return 1;

            if (c1.IsTrump())
            {   // for trumps: type before color
                if ((c1.cardType == CardType.Ace || c1.cardType == CardType.Ten || c1.cardType == CardType.King) && (c2.cardType == CardType.Jack || c2.cardType == CardType.Queen))
                    return 1;
                if ((c2.cardType == CardType.Ace || c2.cardType == CardType.Ten || c2.cardType == CardType.King )&& (c1.cardType == CardType.Jack || c1.cardType == CardType.Queen))
                    return -1;
                if (c1.cardType > c2.cardType)
                    return -1;
                else if (c1.cardType < c2.cardType)
                    return 1;
            } 
            if (c1.cardColor < c2.cardColor)
                return -1;
            else if (c1.cardColor > c2.cardColor)
                return 1;

            if (c1.cardType > c2.cardType)
                return -1;
            else if (c1.cardType < c2.cardType)
                return 1;
            return 0;
        }
    }

    public class Deck
    {
        private List<Card> cards;

        public Deck(bool bWithNines)
        {
            cards = new List<Card>();
            for(int i = 0; i < 4; i++)
            {
                for(int j=(bWithNines?0:1);j<6;j++)
                {
                    Card c = new Card((CardColor)i, (CardType)j);
                    cards.Add(c);
                    cards.Add(c);
                }
            }
        }
        private static Random rng = new Random();

        // Fisher-Yates shuffle
        public void Shuffle()
        {
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
        }

        public List<Card> GetCardsForPlayer(int playernumber)
        {
            List<Card> hand = new List<Card>();
            int cardsProHand = cards.Count / 4;
            if (playernumber >= 0 && playernumber <= 3)
            {
                for(int i=0;i< cardsProHand; i++)
                {
                    hand.Add(cards[i + cardsProHand * playernumber]);
                }
            }
            return hand;
        }

        // public static string HandToString(List<Card> hand)
        // {
        //     string sHand = "";
        //     foreach (Card c in hand)
        //         sHand += c.ToString() + "|";
        //     if(sHand.Length > 1)
        //         return sHand.Substring(0, sHand.Length - 1);
        //     return "";
        // }
        // public static List<Card> StringToHand(string hand)
        // {
        //     string[] sCards = hand.Split('|');
        //     List<Card> cards = new List<Card>();
        //     foreach (string c in sCards)
        //         cards.Add(new Card(c));
        //     return cards;
        // }

        public int NumCardsPerPerson()
        {
            return cards.Count / 4;
        }
    }
}
