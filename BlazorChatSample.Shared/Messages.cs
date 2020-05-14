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
        /// Event name when an update game state event is received
        /// </summary>
        public const string UPDATEGAMESTATE = "UpdateGameState";

        /// <summary>
        /// Name of the Hub method to register a new user
        /// </summary>
        public const string REGISTER = "Register";

        /// <summary>
        /// Name of the Hub method to send a message
        /// </summary>
        public const string SEND = "SendMessage";


        public const string PLAYCARD = "PlayCard";
        public const string WITHDRAWCARD = "WithdrawCard";
        public const string REQDEALING = "RequestDealing";

    }
}
