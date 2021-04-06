using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Model = Discord.API.ApplicationCommand;

namespace Discord.Rest
{
    /// <summary>
    ///     Represents a Rest-based implementation of the <see cref="IApplicationCommand"/>.
    /// </summary>
    public abstract class RestApplicationCommand : RestEntity<ulong>, IApplicationCommand
    {
        /// <inheritdoc/>
        public ulong ApplicationId { get; private set; }

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public string Description { get; private set; }

        /// <summary>
        ///     The options of this command.
        /// </summary>
        public IReadOnlyCollection<RestApplicationCommandOption> Options { get; private set; }

        /// <summary>
        ///     The type of this rest application command.
        /// </summary>
        public RestApplicationCommandType CommandType { get; internal set; }

        /// <inheritdoc/>
        public DateTimeOffset CreatedAt
            => SnowflakeUtils.FromSnowflake(Id);

        internal RestApplicationCommand(BaseDiscordClient client, ulong id) : base(client, id) { }

        internal static RestApplicationCommand Create(BaseDiscordClient client, Model model, RestApplicationCommandType type, ulong guildId = 0)
        {
            return type switch
            {
                RestApplicationCommandType.GlobalCommand => RestGlobalCommand.Create(client, model),
                RestApplicationCommandType.GuildCommand => RestGuildCommand.Create(client, model, guildId),
                _ => null
            };
        }

        internal virtual void Update(Model model)
        {
            ApplicationId = model.ApplicationId;
            Name = model.Name;
            Description = model.Description;

            Options = model.Options.IsSpecified
                ? model.Options.Value.Select(RestApplicationCommandOption.Create).ToImmutableArray().ToReadOnlyCollection()
                : null;
        }

        IReadOnlyCollection<IApplicationCommandOption> IApplicationCommand.Options => Options;

        public virtual Task DeleteAsync(RequestOptions options = null) => throw new NotImplementedException();
    }
}
