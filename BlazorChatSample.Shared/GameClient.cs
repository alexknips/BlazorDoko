﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorChatSample.Shared
{

    /// <summary>
    /// Generic client class that interfaces .NET Standard/Blazor with SignalR Javascript client
    /// </summary>
    public class GameClient : IAsyncDisposable
    {
        public const string HUBURL = "/GameHub";

        private readonly string _hubUrl;
        private HubConnection _hubConnection;

        public enum PlayerPhase{playing,waiting};
        public PlayerPhase playerPhase {get;set;} = PlayerPhase.waiting;

        /// <summary>
        /// Ctor: create a new client for the given hub URL
        /// </summary>
        /// <param name="siteUrl">The base URL for the site, e.g. https://localhost:1234 </param>
        /// <remarks>
        /// Changed client to accept just the base server URL so any client can use it, including ConsoleApp!
        /// </remarks>
        public GameClient(string username, string siteUrl)
        {
            // check inputs
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(siteUrl))
                throw new ArgumentNullException(nameof(siteUrl));
            // save username
            _username = username;
            // set the hub URL
            _hubUrl = siteUrl.TrimEnd('/') + HUBURL;
        }

        /// <summary>
        /// Name of the chatter
        /// </summary>
        private readonly string _username;

        /// <summary>
        /// Flag to show if started
        /// </summary>
        private bool _started = false;

        public GameState gameState { get; set; }

        /// <summary>
        /// Start the SignalR client 
        /// </summary>
        public async Task StartAsync()
        {
            if (!_started)
            {
                // create the connection using the .NET SignalR client
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    // .WithAutomaticReconnect()
                    .Build();
                Console.WriteLine("GameClient: calling Start()");

                // add handler for receiving messages
                _hubConnection.On<string, string>(Messages.RECEIVE, (user, message) =>
                 {
                     HandleReceiveMessage(user, message);
                 });

                // add handler for receiving messages
                _hubConnection.On<GameState>(Messages.UPDATEGAMESTATE, (gamestate) =>
                 {
                     Console.WriteLine(gamestate.ToString());
                     // TODO: sending a gamestate does not work. find fix
                     HandleGameUpdate(gamestate);
                 });

                // start the connection
                await _hubConnection.StartAsync();

                Console.WriteLine("GameClient: Start returned");
                _started = true;

                // register user on hub to let other clients know they've joined
                await _hubConnection.SendAsync(Messages.REGISTER, _username);
            }
        }

        /// <summary>
        /// Handle an inbound message from a hub
        /// </summary>
        /// <param name="method">event name</param>
        /// <param name="message">message content</param>
        private void HandleReceiveMessage(string username, string message)
        {
            // raise an event to subscribers
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(username, message));
        }

        /// <summary>
        /// Handle an inbound gamestate message from a hub
        /// </summary>
        /// <param name="gameState">the new game state (which is then to parse)</param>
        private void HandleGameUpdate(GameState gameState)
        {
            Console.WriteLine("handling");
            // raise an event to subscribers
            GameUpdateReceived?.Invoke(this, new GameUpdateEventArgs(gameState));
        }

        /// <summary>
        /// Event raised when this client receives a message
        /// </summary>
        /// <remarks>
        /// Instance classes should subscribe to this event
        /// </remarks>
        public event MessageReceivedEventHandler MessageReceived;
        public event GameUpdateEventHandler GameUpdateReceived;

        /// <summary>
        /// Send a message to the hub
        /// </summary>
        /// <param name="message">message to send</param>
        public async Task SendAsync(string message)
        {
            // check we are connected
            if (!_started)
                throw new InvalidOperationException("Client not started");
            // send the message
            await _hubConnection.SendAsync(Messages.SEND, _username, message);
        }

        public async Task PlayCard(int idx){
            Card card = gameState.PlayerStates[_username].Hand[idx];
            Console.WriteLine(_username);
            Console.WriteLine(card);
            await _hubConnection.SendAsync(Messages.PLAYCARD, _username, card);
        }

        public async Task ReqDealing(bool withNines = true){
            await _hubConnection.SendAsync(Messages.REQDEALING, _username, withNines);
        }
        

        /// <summary>
        /// Stop the client (if started)
        /// </summary>
        public async Task StopAsync()
        {
            if (_started)
            {
                // disconnect the client
                await _hubConnection.StopAsync();
                // There is a bug in the mono/SignalR client that does not
                // close connections even after stop/dispose
                // see https://github.com/mono/mono/issues/18628
                // this means the demo won't show "xxx left the chat" since 
                // the connections are left open
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                _started = false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("GameClient: Disposing");
            await StopAsync();
        }

        public List<Card> GetOwnCards()
        {
            Console.WriteLine("getowncards");
            if(gameState!= null && gameState.PlayerStates!= null && gameState.PlayerStates.Keys.Contains(_username))
                return gameState.PlayerStates[_username].Hand;
            return null;
        }
    }

    /// <summary>
    /// Delegate for the message handler
    /// </summary>
    /// <param name="sender">the SignalRclient instance</param>
    /// <param name="e">Event args</param>
    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
    
    /// <summary>
    /// Delegate for the gameUpdate handler
    /// </summary>
    /// <param name="sender">the SignalRclient instance</param>
    /// <param name="e">Event args</param>
    public delegate void GameUpdateEventHandler(object sender, GameUpdateEventArgs e);

    /// <summary>
    /// Message received argument class
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(string username, string message)
        {
            Username = username;
            Message = message;
        }

        /// <summary>
        /// Name of the message/event
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Message data items
        /// </summary>
        public string Message { get; set; }

    }


    public class GameUpdateEventArgs : EventArgs
    {
        public GameUpdateEventArgs(GameState newGameState)
        {
            GameState = newGameState;
        }
        public GameState GameState { get; set; }
    }

}
