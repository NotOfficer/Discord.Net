using System;

namespace Discord
{
    /// <summary>
    ///     The response type for an <see cref="IDiscordInteraction"/>.
    /// </summary>
    public enum InteractionResponseType : byte
    {
        /// <summary>
        ///     ACK a Ping.
        /// </summary>
        Pong = 1,

        /// <summary>
        ///     ACK a command without sending a message, eating the user's input.
        /// </summary>
        [Obsolete("DEPRECATED", true)]
        Acknowledge = 2,

        /// <summary>
        ///     Respond with a message, eating the user's input.
        /// </summary>
        [Obsolete("DEPRECATED", true)]
        ChannelMessage = 3,

        /// <summary>
        ///     Respond to an interaction with a message.
        /// </summary>
        ChannelMessageWithSource = 4,

        /// <summary>
        ///     ACK an interaction and edit to a response later, the user sees a loading state.
        /// </summary>
        DeferredChannelMessageWithSource = 5
    }
}
