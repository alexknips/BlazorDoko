using System.Collections.Generic;
using System.Linq;

namespace BlazorChatSample.Shared
{
    /// <summary>
    /// 
    /// </summary>
    public class GameState
    {
        // Hands
        public Dictionary<string, PlayerGameState> PlayerStates { get; set; }

        public Dictionary<string, Card?> CurrentTrick { get; set; }
        public Dictionary<string, Card> LastTrick { get; set; }
        public string StartingPlayer { get; set; }
        public List<string> AllUsers { get; set; }

        public enum GamePhase{
            waitingForStart, Playing, Done
        }
        public GamePhase gamePhase { get; set; }

        public GameState()
        {
            gamePhase = GamePhase.waitingForStart;
            PlayerStates = new Dictionary<string, PlayerGameState>();
            CurrentTrick = new Dictionary<string, Card?>();
            LastTrick = new Dictionary<string, Card>();
        }
        public GameState(string dealerUsername, List<string> allusers, bool bWithNines)
        {
            this.AllUsers = allusers;
            List<string> activePlayers = new List<string>();
            if (allusers.Count < 4)
            {
                // only for debugging purposes!
                activePlayers = allusers;
                for(int i=allusers.Count;i<4;i++)
                {
                    activePlayers.Add("player " + (i+1).ToString());
                }
                int idxDealer = allusers.IndexOf(dealerUsername);
                int idxStartingPlayer = 0;
                StartingPlayer = activePlayers[idxStartingPlayer];
                // throw new System.InvalidOperationException("at least 4 players required");
            }
            else if (allusers.Count == 4)
            {
                activePlayers = allusers;
                int idxDealer = allusers.IndexOf(dealerUsername);
                int idxStartingPlayer = (idxDealer + 1) % 4;
                StartingPlayer = activePlayers[idxStartingPlayer];
            }
            else if (allusers.Count == 5)
            {
                activePlayers = allusers;
                int idxDealer = allusers.IndexOf(dealerUsername);
                StartingPlayer = activePlayers[(idxDealer+1)%5];
                activePlayers.Remove(dealerUsername);
            }
            else if (allusers.Count == 6)
            {
                activePlayers = allusers;
                int idxDealer = allusers.IndexOf(dealerUsername);
                string otherPassingPlayer = allusers[(idxDealer +3)%6];
                StartingPlayer = activePlayers[(idxDealer+1)%6];
                activePlayers.Remove(dealerUsername);
                activePlayers.Remove(otherPassingPlayer);
            }
            else if (allusers.Count >= 7)
            {
                throw new System.NotImplementedException("might follow some day...");
            }

            gamePhase = GamePhase.Playing;
            var deck = new Shared.Deck(bWithNines);

            CurrentTrick = new Dictionary<string, Card>();
            LastTrick  = new Dictionary<string, Card>();
            PlayerStates = new Dictionary<string, PlayerGameState>();
            for (int i = 0; i < activePlayers.Count; i++)
            {
                var gameStatePlayer = new PlayerGameState(deck.GetCardsForPlayer(i));
                PlayerStates.Add(activePlayers[i], gameStatePlayer);
                CurrentTrick.Add(activePlayers[i], null);
                LastTrick.Add(activePlayers[i], null);
            }
        }

        // When a player claims a trick a next trick is started
        // Thus the lastTrick is the previous CurrentTrick
        // The CurrentTrick is set to be empty for each player
        public void TrickClaimed(string claimingPlayer)
        {
            LastTrick = CurrentTrick;

            int valueOfTrick = 0;
            foreach (string players in CurrentTrick.Keys)
            {
                valueOfTrick += CurrentTrick[players].points;
                CurrentTrick[players] = null;
            }
            PlayerStates[claimingPlayer].Points += valueOfTrick;

            int? numCards = null;
            foreach(string player in PlayerStates.Keys)
                if(numCards == 0)
                    numCards = PlayerStates[player].Hand.Count;
                else if(PlayerStates[player].Hand.Count != numCards)
                    throw new System.Exception("number of cards of players differ");

            if(numCards == 0) // all cards had been played
            {
                EndOfGame();
            }
        }

        // when all cards had been played
        public void EndOfGame()
        {
            gamePhase = GamePhase.Done;
        }

        public void CardPlayed(string playingUser, Card c)
        {
            if (CurrentTrick[playingUser] != null)
                throw new System.InvalidOperationException("another card had already been played");

            CurrentTrick[playingUser] = c;

            try
            {
                var cardToRemove = PlayerStates[playingUser].Hand.First(x=>x.cardColor==c.cardColor&&x.cardType==c.cardType);
                PlayerStates[playingUser].Hand.Remove(cardToRemove);
            }
            catch
            {
                throw new System.Exception("could not remove card although it was played");
            }
        }

        public void CardWithdrawn(string withdrawingUser)
        {
            if (CurrentTrick[withdrawingUser] == null)
            {
                throw new System.InvalidOperationException("there is no card to withdraw");
            }
            var card = CurrentTrick[withdrawingUser];
            PlayerStates[withdrawingUser].Hand.Add(card);
            CurrentTrick[withdrawingUser] = null;
        }

        public void CardOffered(string fromUser, string toUser, Card card)
        {
            if (fromUser == toUser)
                throw new System.InvalidOperationException("From and To user cannot be equal");

            PlayerStates[fromUser].Hand.Remove(card);
            PlayerStates[toUser].Hand.Add(card);
        }
    }

    public class PlayerGameState
    {
        public List<Card> Hand { get; set; } 
        public int Points { get; set; } 
        public int numTricks { get; set; }   

        public PlayerGameState()
        {
            Hand = new List<Card>();
            Points = 0;
            numTricks = 0;
        }
        public PlayerGameState(List<Card> hand)
        {
            Hand = hand;
            Points = 0;
            numTricks = 0;
        }
    }
}
