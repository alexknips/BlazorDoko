using System.Collections.Generic;

namespace BlazorChatSample.Shared
{
    /// <summary>
    /// Stores shared names used in both client and hub
    /// </summary>
    public static class Messages
    {
        /// <summary>
        /// Event name when a message is received
        /// </summary>
        public const string RECEIVE = "ReceiveMessage";

        /// <summary>
        /// Name of the Hub method to register a new user
        /// </summary>
        public const string REGISTER = "Register";

        /// <summary>
        /// Name of the Hub method to send a message
        /// </summary>
        public const string SEND = "SendMessage";

        public const string PICKED = "Picked";
        public const string UNPICKED = "Unpicked";
        public const string CLAIMED = "Claimed";
        public const string OFFERED = "Offered";
        public const string NUMCARDS = "NumCards";
        public const string DEALING = "Dealing";
        public const string TRADING = "Trading";

        public static Message DecodeMessage(string message)
        {
            if(!message.Contains("|"))
                return new MessageFailed();
            string msgtype = message.Split('|')[0];
            message = message.Substring(message.IndexOf("|")+1);
            switch(msgtype)
            {
                case PICKED:
                    return new MessagePicked(message);
                case UNPICKED:
                    return new MessageUnpicked(message);
                case NUMCARDS:
                    return new MessageNumCards(message);
                case DEALING:
                    return new MessageDealing(message);
                default:
                    return new MessageFailed();
            }
        }

        public static string EncodeMessage(Message message)
        {
            return message.ToString();
        }

        public interface Message{
            // public const string messageType;
        }
        public class MessageFailed : Message{
        }
        
        /// <summary>
        /// a player picked a card, containing playername and card identifier
        /// </summary>
        public class MessagePicked : Message{
            public const string messageType = PICKED;
            public string playername;
            public Server.Card card;
            public MessagePicked(string msg)
            {
                string[] msgparts = msg.Split('|');
                if(msgparts.Length == 2){
                    playername = msgparts[0];
                    card = new Server.Card(msgparts[1]);
                }
            }
            public MessagePicked(string playername, Server.Card card)
            {
                this.playername = playername;
                this.card = card;
            }
            public override string ToString()
            {
                return messageType + "|" + playername + "|" + card.ToString();
            }
        }
        
        /// <summary>
        /// a player withdrew a card, containing playername and card identifier
        /// </summary>
        public class MessageUnpicked : Message{
            public const string messageType = UNPICKED;
            public string playername;
            public Server.Card card;
            public MessageUnpicked(string msg)
            {
                string[] msgparts = msg.Split('|');
                if(msgparts.Length == 2){
                    playername = msgparts[0];
                    card = new Server.Card(msgparts[1]);
                }
            }
            public MessageUnpicked(string playername, Server.Card card)
            {
                this.playername = playername;
                this.card = card;
            }
            public override string ToString()
            {
                return messageType + "|" + playername + "|" + card.ToString();
            }
        }

        /// <summary>
        /// a player claimed the trick, containing playername
        /// </summary>
        public class MessageClaimed : Message{
            public const string messageType = CLAIMED;
            public string playername;
            public MessageClaimed(string playername) // both ctor's have same signature
            {
                this.playername = playername;
            }
            public override string ToString()
            {
                return messageType + "|" + playername;
            }
        }

        /// <summary>
        /// telling the players how many card they get
        /// </summary>
        public class MessageNumCards : Message{
            public const string messageType = NUMCARDS;
            public int numCards;
            public MessageNumCards(string msg)
            {
                numCards = int.Parse(msg);
            }
            public MessageNumCards(int numCards)
            {
                this.numCards = numCards;
            }
            public override string ToString()
            {
                return messageType + "|" + numCards.ToString();
            }
        }
        
        /// <summary>
        /// dealing a hand a player, containing playername and list of cards
        /// </summary>
        public class MessageDealing : Message{
            public const string messageType = DEALING;
            public List<Server.Card> hand;
            public MessageDealing(string msg)
            {
                hand = Server.Deck.StringToHand(msg);
            }
            public MessageDealing(List<Server.Card> hand)
            {
                this.hand = hand;
            }
            public override string ToString()
            {
                return messageType + "|" + Server.Deck.HandToString(hand);
            }
        }
        
        /// <summary>
        /// invoked when a player trades a card with another one ("poverty of trumps"/"Trumpfabgabe")
        /// </summary>
        public class MessageCardOffered : Message{
            public const string messageType = OFFERED;
            public string playerFrom;
            public string playerTo;
            public Server.Card card;
            public MessageCardOffered(string msg)
            {
                string[] msgparts = msg.Split('|');
                if(msgparts.Length == 3){
                    playerFrom = msgparts[0];
                    playerTo = msgparts[1];
                    card = new Server.Card(msgparts[2]);
                }
            }
            public MessageCardOffered(string playerFrom, string playerTo, Server.Card card)
            {
                this.playerFrom = playerFrom;
                this.playerTo = playerTo;
                this.card = card;
            }
            public override string ToString()
            {
                return messageType + "|" + playerFrom + "|" + playerTo + "|" + card.ToString();
            }
        }
    }
}
