using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Model = Discord.API.Gateway.ApplicationCommandCreatedUpdatedEvent;

namespace Discord.WebSocket
{
    /// <summary>
    ///     Represends a Websocket-based <see cref="IApplicationCommand"/> recieved over the gateway.
    /// </summary>
    public class SocketApplicationCommand : SocketEntity<ulong>, IApplicationCommand
    {
        /// <inheritdoc/>
        public ulong ApplicationId { get; private set; }

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public string Description { get; private set; }

        /// <summary>
        ///     A collection of <see cref="SocketApplicationCommandOption"/>'s recieved over the gateway.
        /// </summary>
        public IReadOnlyCollection<SocketApplicationCommandOption> Options { get; private set; }

        public DateTimeOffset CreatedAt
            => SnowflakeUtils.FromSnowflake(Id);

        /// <summary>
        ///     The <see cref="SocketGuild"/> where this application was created.
        /// </summary>
        public SocketGuild Guild => Discord.GetGuild(GuildId);
        private ulong GuildId { get; set; }

        internal SocketApplicationCommand(DiscordSocketClient client, ulong id) : base(client, id) { }

        internal static SocketApplicationCommand Create(DiscordSocketClient client, Model model)
        {
            var entity = new SocketApplicationCommand(client, model.Id);
            entity.Update(model);
            return entity;
        }

        internal void Update(Model model)
        {
            ApplicationId = model.ApplicationId;
            Description = model.Description;
            Name = model.Name;
            GuildId = model.GuildId;

            Options = model.Options != null && model.Options.Count != 0
                ? model.Options.Select(SocketApplicationCommandOption.Create).ToImmutableArray()
                : new ImmutableArray<SocketApplicationCommandOption>();
        }

        public Task DeleteAsync(RequestOptions options = null) => throw new NotImplementedException();
        IReadOnlyCollection<IApplicationCommandOption> IApplicationCommand.Options => Options;
    }
}
