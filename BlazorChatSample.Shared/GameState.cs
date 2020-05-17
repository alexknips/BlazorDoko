﻿using System.Collections.Generic;
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

        public Dictionary<string, Card> CurrentTrick { get; set; }
        public Dictionary<string, Card> LastTrick { get; set; }
        public string StartingPlayer { get; set; }
        public List<string> UsernameList { get; set; }
        public List<string> ActivePlayers { 
            get{ 
                if(PlayerStates!=null) 
                    return PlayerStates.Keys.ToList();
                return new List<string>();
            }
        }

        public enum GamePhase{
            waitingForStart, Playing, Done
        }
        public GamePhase gamePhase { get; set; } = GamePhase.waitingForStart;

        public GameState()
        {
            gamePhase = GamePhase.waitingForStart;
            PlayerStates = new Dictionary<string, PlayerGameState>();
            CurrentTrick = new Dictionary<string, Card>();
            LastTrick = new Dictionary<string, Card>();
        }
        public GameState(string dealerUsername, List<Player> allusers, bool bWithNines)
        {
            allusers.Sort(Player.Compare);
            this.UsernameList = new List<string>();
            foreach(Player p in allusers)
            {
                UsernameList.Add(p.name);
            }
            List<string> activePlayers = new List<string>();
            if (allusers.Count < 4)
            {
                // only for debugging purposes!
                activePlayers = UsernameList;
                for(int i=UsernameList.Count;i<4;i++)
                {
                    activePlayers.Add("player " + (i+1).ToString());
                }
                int idxDealer = UsernameList.IndexOf(dealerUsername);
                int idxStartingPlayer = 0;
                StartingPlayer = activePlayers[idxStartingPlayer];
                // throw new System.InvalidOperationException("at least 4 players required");
            }
            else if (UsernameList.Count == 4)
            {
                activePlayers = UsernameList;
                int idxDealer = UsernameList.IndexOf(dealerUsername);
                int idxStartingPlayer = (idxDealer + 1) % 4;
                StartingPlayer = activePlayers[idxStartingPlayer];
            }
            else if (UsernameList.Count == 5)
            {
                activePlayers = UsernameList;
                int idxDealer = UsernameList.IndexOf(dealerUsername);
                StartingPlayer = activePlayers[(idxDealer+1)%5];
                activePlayers.Remove(dealerUsername);
            }
            else if (UsernameList.Count == 6)
            {
                activePlayers = UsernameList;
                int idxDealer = UsernameList.IndexOf(dealerUsername);
                string otherPassingPlayer = UsernameList[(idxDealer +3)%6];
                StartingPlayer = activePlayers[(idxDealer+1)%6];
                activePlayers.Remove(dealerUsername);
                activePlayers.Remove(otherPassingPlayer);
            }
            else if (allusers.Count >= 7)
            {
                throw new System.NotImplementedException("might follow some day...");
            }

            var deck = new Shared.Deck(bWithNines);
            deck.Shuffle();

            CurrentTrick = new Dictionary<string, Card>();
            LastTrick  = new Dictionary<string, Card>();
            PlayerStates = new Dictionary<string, PlayerGameState>();
            for (int i = 0; i < activePlayers.Count; i++)
            {
                var gameStatePlayer = new PlayerGameState(deck.GetCardsForPlayer(i));
                PlayerStates.Add(activePlayers[i], gameStatePlayer);
            }
        }

        // When a player claims a trick a next trick is started
        // Thus the lastTrick is the previous CurrentTrick
        // The CurrentTrick is set to be empty for each player
        public void TrickClaimed(string claimingPlayer)
        {
            LastTrick = new Dictionary<string, Card>(CurrentTrick);

            int valueOfTrick = 0;
            List<string> activePlayers = PlayerStates.Keys.ToList();
            foreach (string players in activePlayers)
            {
                valueOfTrick += CurrentTrick[players].points;
            }
            PlayerStates[claimingPlayer].Points += valueOfTrick;
            PlayerStates[claimingPlayer].numTricks++;

            CurrentTrick = new Dictionary<string, Card>();

            int? numCards = null;
            foreach(string player in PlayerStates.Keys)
                if(numCards == null)
                    numCards = PlayerStates[player].Hand.Count;
                else if(PlayerStates[player].Hand.Count != numCards)
                    throw new System.Exception("number of cards of players differ");

            // set to phase "playing" after first complete trick. 
            // maybe people want to withdraw and go back to initial phase for trading cards
            if(numCards > 0)
                gamePhase = GamePhase.Playing;  
            else
                gamePhase = GamePhase.Done;
        }

        public void CardPlayed(string playingUser, Card c)
        {
            if (CurrentTrick.ContainsKey(playingUser))
                throw new System.InvalidOperationException("another card had already been played");

            CurrentTrick.Add(playingUser,new Card(c));

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
            if (!CurrentTrick.ContainsKey(withdrawingUser))
            {
                throw new System.InvalidOperationException("there is no card to withdraw");
            }
            var card = CurrentTrick[withdrawingUser];
            PlayerStates[withdrawingUser].Hand.Add(new Card(card));
            CurrentTrick.Remove(withdrawingUser);
        }

        public void CardOffered(string fromUser, string toUser, Card card)
        {
            if (fromUser == toUser)
                throw new System.InvalidOperationException("From and To user cannot be equal");

            var cardToRemove = PlayerStates[fromUser].Hand.First(x=>x.cardColor==card.cardColor&&x.cardType==card.cardType);
            PlayerStates[fromUser].Hand.Remove(cardToRemove);
            PlayerStates[toUser].Hand.Add(new Card(card));
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
            Hand.Sort(Card.Compare);
            Points = 0;
            numTricks = 0;
        }
    }
}
