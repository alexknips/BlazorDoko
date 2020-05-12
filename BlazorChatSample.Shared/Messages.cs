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


        public enum messageType
        {PICKED,UNPICKED,CLAIMED,OFFERED,NUMCARDS,DEALING,REQDEALING,TRADING};

        public static Message DecodeMessage(string message)
        {
            if(!message.Contains("|"))
                return new MessageFailed();
            try{
                messageType msgtype = (messageType)int.Parse(message.Split('|')[0]);
                message = message.Substring(message.IndexOf("|")+1);
                switch(msgtype)
                {
                    case messageType.PICKED:
                        return new MessagePicked(message);
                    case messageType.UNPICKED:
                        return new MessageUnpicked(message);
                    case messageType.NUMCARDS:
                        return new MessageNumCards(message);
                    case messageType.DEALING:
                        return new MessageDealing(message);
                    default:
                        return new MessageFailed();
                }
            }
            catch
            {
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
            public const messageType msgType = messageType.PICKED;
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
                return ((int)msgType).ToString() + "|" + playername + "|" + card.ToString();
            }
        }
        
        /// <summary>
        /// a player withdrew a card, containing playername and card identifier
        /// </summary>
        public class MessageUnpicked : Message{
            public const messageType msgType = messageType.UNPICKED;
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
                return ((int)msgType).ToString() + "|" + playername + "|" + card.ToString();
            }
        }

        /// <summary>
        /// a player claimed the trick, containing playername
        /// </summary>
        public class MessageClaimed : Message{
            public const messageType msgType = messageType.CLAIMED;
            public string playername;
            public MessageClaimed(string playername) // both ctor's have same signature
            {
                this.playername = playername;
            }
            public override string ToString()
            {
                return ((int)msgType).ToString() + "|" + playername;
            }
        }

        /// <summary>
        /// telling the players how many card they get
        /// </summary>
        public class MessageNumCards : Message{
            public const messageType msgType = messageType.NUMCARDS;
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
                return ((int)msgType).ToString() + "|" + numCards.ToString();
            }
        }
        
        /// <summary>
        /// dealing a hand a player, containing playername and list of cards
        /// </summary>
        public class MessageDealing : Message{
            public const messageType msgType = messageType.DEALING;
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
                return ((int)msgType).ToString() + "|" + Server.Deck.HandToString(hand);
            }
        }
        
        /// <summary>
        /// request a new dealing
        /// </summary>
        public class MessageReqDealing : Message{
            public const messageType msgType = messageType.REQDEALING;
            public MessageReqDealing(){}
            public override string ToString()
            {
                return ((int)msgType).ToString();
            }
        }
        
        /// <summary>
        /// invoked when a player trades a card with another one ("poverty of trumps"/"Trumpfabgabe")
        /// </summary>
        public class MessageCardOffered : Message{
            public const messageType msgType = messageType.OFFERED;
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
                return ((int)msgType).ToString() + "|" + playerFrom + "|" + playerTo + "|" + card.ToString();
            }
        }
    }
}
