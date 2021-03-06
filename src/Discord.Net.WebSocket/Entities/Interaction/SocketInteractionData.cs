using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Model = Discord.API.ApplicationCommandInteractionData;

namespace Discord.WebSocket
{
    public class SocketInteractionData : SocketEntity<ulong>, IApplicationCommandInteractionData
    {
        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <summary>
        ///     The <see cref="SocketInteractionDataOption"/>'s recieved with this interaction.
        /// </summary>
        public IReadOnlyCollection<SocketInteractionDataOption> Options { get; private set; }

        internal SocketInteractionData(DiscordSocketClient client, ulong id) : base(client, id) { }

        internal static SocketInteractionData Create(DiscordSocketClient client, Model model, ulong guildId)
        {
            var entity = new SocketInteractionData(client, model.Id);
            entity.Update(model, guildId);
            return entity;
        }
        internal void Update(Model model, ulong guildId)
        {
            Name = model.Name;
            Options = model.Options.IsSpecified
                ? model.Options.Value.Select(x => new SocketInteractionDataOption(x, Discord, guildId)).ToImmutableArray().ToReadOnlyCollection()
                : null;
        }

        IReadOnlyCollection<IApplicationCommandInteractionDataOption> IApplicationCommandInteractionData.Options => Options;
    }
}
