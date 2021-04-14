using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord.SlashCommands
{
    public class SlashCommandModule<T> : ISlashCommandModule  where T : class, IDiscordInteraction
    {
        /// <summary>
        ///     The underlying interaction of the command.
        /// </summary>
        /// <seealso cref="T:Discord.IDiscordInteraction"/>
        /// <seealso cref="T:Discord.WebSocket.SocketInteraction" />
        public T Interaction { get; private set; }

        void ISlashCommandModule.SetContext(IDiscordInteraction interaction)
        {
            var newValue = interaction as T;
            Interaction = newValue ?? throw new InvalidOperationException($"Invalid interaction type. Expected {typeof(T).Name}, got {interaction.GetType().Name}.");
        }

        public async Task<IMessage> Reply(string text = null, Embed embed = null, bool isTTS = false, AllowedMentions allowedMentions = null, RequestOptions options = null)
        {
            if (Interaction is SocketInteraction interaction)
            {
                return await interaction.FollowupAsync(text, embed, isTTS, allowedMentions, options);
            }
            return null;
        }
    }
}
