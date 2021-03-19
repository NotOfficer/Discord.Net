using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Model = Discord.API.ApplicationCommandInteractionDataOption;

namespace Discord.WebSocket
{
    /// <summary>
    ///     Represents a Websocket-based <see cref="IApplicationCommandInteractionDataOption"/> recieved by the gateway
    /// </summary>
    public class SocketInteractionDataOption : IApplicationCommandInteractionDataOption
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public object Value { get; }

        /// <summary>
        ///      The sub command options recieved for this sub command group.
        /// </summary>
        public IReadOnlyCollection<SocketInteractionDataOption> Options { get; }

        private readonly DiscordSocketClient _discord;
        private readonly ulong _guild;

        internal SocketInteractionDataOption() { }
        internal SocketInteractionDataOption(Model model, DiscordSocketClient discord, ulong guild)
        {
            Name = model.Name;
            Value = model.Value.IsSpecified ? model.Value.Value : null;
            _discord = discord;
            _guild = guild;

            Options = model.Options.IsSpecified
                ? model.Options.Value.Select(x => new SocketInteractionDataOption(x, discord, guild)).ToImmutableArray().ToReadOnlyCollection()
                : null;

        }

        // Converters
        public static explicit operator bool(SocketInteractionDataOption option)
            => (bool)option.Value;
        // The default value is of type long, so an implementaiton of of the long option is trivial
        public static explicit operator int(SocketInteractionDataOption option)
            => unchecked(
            (int)(long)option.Value
            );
        public static explicit operator string(SocketInteractionDataOption option)
            => option.Value.ToString();

        public static explicit operator bool?(SocketInteractionDataOption option)
        {
            if (option.Value == null)
                return null;

            return (bool)option;
        }
        public static explicit operator int?(SocketInteractionDataOption option)
        {
            if (option.Value == null)
                return null;

            return (int)option;
        }

        public static explicit operator SocketGuildChannel(SocketInteractionDataOption option)
        {
            if (!ulong.TryParse((string)option.Value, out var id))
                return null;

            var guild = option._discord.GetGuild(option._guild);
            return guild?.GetChannel(id);
        }

        public static explicit operator SocketRole(SocketInteractionDataOption option)
        {
            if (!ulong.TryParse((string)option.Value, out var id))
                return null;

            var guild = option._discord.GetGuild(option._guild);
            return guild?.GetRole(id);
        }

        public static explicit operator SocketGuildUser(SocketInteractionDataOption option)
        {
            if (!ulong.TryParse((string)option.Value, out var id))
                return null;

            var guild = option._discord.GetGuild(option._guild);
            return guild?.GetUser(id);
        }

        IReadOnlyCollection<IApplicationCommandInteractionDataOption> IApplicationCommandInteractionDataOption.Options => Options;
    }
}
