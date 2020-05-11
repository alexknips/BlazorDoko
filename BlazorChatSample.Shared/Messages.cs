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

    }
}
