using System.Collections.Generic;

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

        public GameState(string dealerUsername, List<string> allusers, bool bWithNines)
        {
            List<string> activePlayers = new List<string>();
            if (allusers.Count < 4)
            {
                throw new System.InvalidOperationException("at least 4 players required");
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
                activePlayers.Remove(dealerUsername);
                int idxStartingPlayer = idxDealer % 4;
                StartingPlayer = activePlayers[idxStartingPlayer];
            }
            else if (allusers.Count >= 6)
            {
                throw new System.NotImplementedException("will follow soon...");
            }


            var deck = new Shared.Deck(bWithNines);

            CurrentTrick = new Dictionary<string, Card>();
            for (int i = 0; i < 4; i++)
            {
                var gameStatePlayer = new PlayerGameState(deck.GetCardsForPlayer(i));
                PlayerStates.Add(activePlayers[i], gameStatePlayer);
                CurrentTrick.Add(activePlayers[i], null);
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
        }

        public void CardPlayed(string playingUser, Card c)
        {
            if (CurrentTrick[playingUser] != null)
                throw new System.InvalidOperationException("another card had already been played");

            CurrentTrick[playingUser] = c;

            try
            {
                PlayerStates[playingUser].Hand.Remove(c);
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

        public PlayerGameState()
        {
            Hand = new List<Card>();
        }
        public PlayerGameState(List<Card> hand)
        {
            Hand = hand;
            Points = 0;
        }
    }
}
