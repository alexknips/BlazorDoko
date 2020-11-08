using BlazorChatSample.Shared;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorChatSample.Server.Hubs
{
    /// <summary>
    /// The SignalR hub 
    /// </summary>
    public class GameHub : Hub
    {
        /// <summary>
        /// connectionId-to-username lookup
        /// </summary>
        /// <remarks>
        /// Needs to be static as the chat is created dynamically a lot
        /// </remarks>
        private static readonly Dictionary<string, Player> userLookup = new Dictionary<string, Player>();

        private static GameState gameState;


        /// <summary>
        /// Send a message to all clients
        /// </summary>
        /// <param name="username"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessage(string username, string message)
        {
            await Clients.All.SendAsync(Messages.RECEIVE, username, message);
        }

        // Request handout/dealing
        public async Task RequestDealing(string usernameDealer, bool bWithNines)
        {
            // 1. calculate GameState
            gameState = new GameState(usernameDealer, userLookup.Values.ToList(), bWithNines);
            // 2. return new GameState
            await Clients.All.SendAsync(Messages.UPDATEGAMESTATE, gameState);
        }
        // Play card
        public async Task PlayCard(string username, Card card)
        {
            // 1. calculate GameState
            gameState.CardPlayed(username, card);
            // 2. return new GameState
            await Clients.All.SendAsync(Messages.UPDATEGAMESTATE, gameState);
        }

        // Take/withdraw card
        public async Task WithdrawCard(string username)
        {
            // 1. calculate GameState
            gameState.CardWithdrawn(username);
            // 2. return new GameState
            await Clients.All.SendAsync(Messages.UPDATEGAMESTATE, gameState);
        }
        // Claim trick
        public async Task ClaimTrick(string username)
        {
            // 1. calculate GameState
            gameState.TrickClaimed(username);
            // 2. return new GameState
            await Clients.All.SendAsync(Messages.UPDATEGAMESTATE, gameState);
        }

        // Offer card to another player
        public async Task OfferCard(string username, string receivingUser, Card card)
        {
            // 1. calculate GameState
            gameState.CardOffered(username, receivingUser, card);
            // 2. return new GameState
            await Clients.All.SendAsync(Messages.UPDATEGAMESTATE, gameState);
        }

        public async Task UpdateGameState()
        {
            await Clients.All.SendAsync(Messages.UPDATEGAMESTATE, gameState);
        }


        /// <summary>
        /// Register username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task Register(string username, int seatnumber)
        {
            var currentId = Context.ConnectionId;
            if (!userLookup.ContainsKey(currentId))
            {
                Player p = new Player();
                p.name = username;
                p.seatnumber = seatnumber;
                // maintain a lookup of connectionId-to-username
                userLookup.Add(currentId, p);
                // re-use existing message for now
                await Clients.AllExcept(currentId).SendAsync(
                    Messages.RECEIVE,
                    username, $"{username} joined the chat");
            }

            string allUsers = "";
            foreach(Player p in userLookup.Values)
            {
                allUsers += p.name + " (" + p.seatnumber + "), ";
            }
            await Clients.All.SendAsync(Messages.RECEIVE,"allplayers",allUsers);

            // if someone connects, it might have been a reconnect. 
            // just resend the gamestate to everyone
            if(gameState == null)
            { // if the game didn't start yet, just initialize it
                gameState = new GameState();
            }
            await Clients.All.SendAsync(Messages.UPDATEGAMESTATE, gameState);
        }

        /// <summary>
        /// Log connection
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("Connected");
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// Log disconnection
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception e)
        {
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
            // try to get connection
            string id = Context.ConnectionId;
            Player player;
            if (!userLookup.TryGetValue(id, out player))
                player.name = "[unknown]";


            userLookup.Remove(id);
            await Clients.AllExcept(Context.ConnectionId).SendAsync(
                Messages.RECEIVE,
                player.name, $"{player.name} has left the chat");
            await base.OnDisconnectedAsync(e);
            
            string allUsers = "";
            foreach(Player p in userLookup.Values)
            {
                allUsers += p.name + " (" + p.seatnumber + "), ";
            }
            await Clients.All.SendAsync(Messages.RECEIVE,"allplayers",allUsers);
        }


    }
}
