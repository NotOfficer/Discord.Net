using Discord.API;
using Discord.API.Rest;
using Discord.Rest;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Model = Discord.API.Gateway.InteractionCreated;

namespace Discord.WebSocket
{
    /// <summary>
    ///     Represents an Interaction recieved over the gateway.
    /// </summary>
    public class SocketInteraction : SocketEntity<ulong>, IDiscordInteraction
    {
        /// <summary>
        ///     The <see cref="DiscordSocketClient"/> this interaction was used in.
        /// </summary>
        public DiscordSocketClient Client => Discord;

        /// <summary>
        ///     The <see cref="SocketGuild"/> this interaction was used in.
        /// </summary>
        public SocketGuild Guild => Discord.GetGuild(GuildId);

        /// <summary>
        ///     The <see cref="SocketTextChannel"/> this interaction was used in.
        /// </summary>
        public SocketTextChannel Channel => Guild.GetTextChannel(ChannelId);

        /// <summary>
        ///     The <see cref="SocketGuildUser"/> who triggered this interaction.
        /// </summary>
        public SocketGuildUser User => Guild.GetUser(UserId);

        /// <summary>
        ///     The type of this interaction.
        /// </summary>
        public InteractionType Type { get; private set; }

        /// <summary>
        ///     The data associated with this interaction.
        /// </summary>
        public SocketInteractionData Data { get; private set; }

        /// <summary>
        ///     The token used to respond to this interaction.
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        ///     The version of this interaction.
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        ///     The creation date of this interaction.
        /// </summary>
        public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);

        /// <summary>
        ///     <see langword="true"/> if the token is valid for replying to, otherwise <see langword="false"/>.
        /// </summary>
        public bool IsValidToken => CheckToken();

        private ulong GuildId { get; set; }
        private ulong ChannelId { get; set; }
        private ulong UserId { get; set; }

        internal SocketInteraction(DiscordSocketClient client, ulong id) : base(client, id) { }

        internal static SocketInteraction Create(DiscordSocketClient client, Model model)
        {
            var entitiy = new SocketInteraction(client, model.Id);
            entitiy.Update(model);
            return entitiy;
        }

        internal void Update(Model model)
        {
            Data = model.Data.IsSpecified
                ? SocketInteractionData.Create(Discord, model.Data.Value, model.GuildId)
                : null;

            GuildId = model.GuildId;
            ChannelId = model.ChannelId;
            Token = model.Token;
            Version = model.Version;
            UserId = model.Member.User.Id;
            Type = model.Type;
        }
        private bool CheckToken()
        {
            // Tokens last for 15 minutes according to https://discord.com/developers/docs/interactions/slash-commands#responding-to-an-interaction
            var elapsed = DateTimeOffset.UtcNow - CreatedAt;
            return elapsed.TotalMinutes < 15d;
        }

        /// <summary>
        /// Responds to an Interaction.
        /// </summary>
        /// <param name="text">The text of the message to be sent.</param>
        /// <param name="isTTS"><see langword="true"/> if the message should be read out by a text-to-speech reader, otherwise <see langword="false"/>.</param>
        /// <param name="embed">A <see cref="Embed"/> to send with this response.</param>
        /// <param name="type">The type of response to this Interaction.</param>
        /// <param name="allowedMentions">The allowed mentions for this response.</param>
        /// <param name="options">The request options for this response.</param>
        /// <returns>
        ///     The <see cref="IMessage"/> sent as the response. If this is the first acknowledgement, it will return null.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Message content is too long, length must be less or equal to <see cref="DiscordConfig.MaxMessageSize"/>.</exception>
        /// <exception cref="InvalidOperationException">The parameters provided were invalid or the token was invalid.</exception>

        public async Task<IMessage> RespondAsync(string text = null, bool isTTS = false, Embed embed = null, InteractionResponseType type = InteractionResponseType.ChannelMessageWithSource, AllowedMentions allowedMentions = null, RequestOptions options = null)
        {
            if (type == InteractionResponseType.Pong)
                throw new InvalidOperationException($"Cannot use {Type} on a send message function");

            if (!IsValidToken)
                throw new InvalidOperationException("Interaction token is no longer valid");

            Preconditions.AtMost(allowedMentions?.RoleIds?.Count ?? 0, 100, nameof(allowedMentions.RoleIds), "A max of 100 role Ids are allowed.");
            Preconditions.AtMost(allowedMentions?.UserIds?.Count ?? 0, 100, nameof(allowedMentions.UserIds), "A max of 100 user Ids are allowed.");

            // check that user flag and user Id list are exclusive, same with role flag and role Id list
            if (allowedMentions?.AllowedTypes != null)
            {
                if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users) &&
                    allowedMentions.UserIds != null && allowedMentions.UserIds.Count > 0)
                {
                    throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", nameof(allowedMentions));
                }

                if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles) &&
                    allowedMentions.RoleIds != null && allowedMentions.RoleIds.Count > 0)
                {
                    throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", nameof(allowedMentions));
                }
            }

            var response = new InteractionResponse
            {
                Type = type,
                Data = new InteractionApplicationCommandCallbackData(text)
                {
                    AllowedMentions = allowedMentions?.ToModel(),
                    Embeds = embed != null
                        ? new[] { embed.ToModel() }
                        : Optional<API.Embed[]>.Unspecified,
                    TTS = isTTS
                }
            };

            await Discord.Rest.ApiClient.CreateInteractionResponse(response, Id, Token, options);
            return null;
        }

        /// <summary>
        ///     Sends a followup message for this interaction.
        /// </summary>
        /// <param name="text">The text of the message to be sent</param>
        /// <param name="embed">A <see cref="Embed"/> to send with this response.</param>
        /// <param name="isTTS"><see langword="true"/> if the message should be read out by a text-to-speech reader, otherwise <see langword="false"/>.</param>
        /// <param name="allowedMentions">The allowed mentions for this response.</param>
        /// <param name="options">The request options for this response.</param>
        /// <returns>
        ///     The sent message.
        /// </returns>
        public async Task<IMessage> FollowupAsync(string text = null, Embed embed = null, bool isTTS = false, AllowedMentions allowedMentions = null, RequestOptions options = null)
        {
            if (!IsValidToken)
                throw new InvalidOperationException("Interaction token is no longer valid");

            var args = new CreateWebhookMessageParams(text) { IsTTS = isTTS };
            if (embed != null)
                args.Embeds = new[] { embed.ToModel() };
            if (allowedMentions != null)
                args.AllowedMentions = allowedMentions.ToModel();

            return await InteractionHelper.SendFollowupAsync(Discord.Rest, args, Token, Channel, options);
        }

        /// <summary>
        ///     Sends a followup message for this interaction.
        /// </summary>
        /// <param name="text">The text of the message to be sent</param>
        /// <param name="embeds">A <see cref="Embed"/> to send with this response.</param>
        /// <param name="isTTS"><see langword="true"/> if the message should be read out by a text-to-speech reader, otherwise <see langword="false"/>.</param>
        /// <param name="allowedMentions">The allowed mentions for this response.</param>
        /// <param name="options">The request options for this response.</param>
        /// <returns>
        ///     The sent message.
        /// </returns>
        public async Task<IMessage> FollowupEmbedsAsync(IEnumerable<Embed> embeds, string text = null, bool isTTS = false, AllowedMentions allowedMentions = null, RequestOptions options = null)
        {
            if (!IsValidToken)
                throw new InvalidOperationException("Interaction token is no longer valid");

            var args = new CreateWebhookMessageParams(text)
            {
                IsTTS = isTTS
            };
            if (embeds != null)
                args.Embeds = embeds.Select(x => x.ToModel()).ToArray();
            if (allowedMentions != null)
                args.AllowedMentions = allowedMentions.ToModel();

            return await InteractionHelper.SendFollowupAsync(Discord.Rest, args, Token, Channel, options);
        }

        /// <summary> Sends a message to the channel for this webhook with an attachment. </summary>
        /// <returns> Returns the ID of the created message. </returns>
        public Task<IMessage> FollowupFileAsync(string filePath, string text = null, bool isTTS = false,
            IEnumerable<Embed> embeds = null, string username = null, string avatarUrl = null,
            RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null)
            => FollowupFileAsyncInternal(filePath, text, isTTS, embeds, username, avatarUrl, allowedMentions, options, isSpoiler);
        /// <summary> Sends a message to the channel for this webhook with an attachment. </summary>
        /// <returns> Returns the ID of the created message. </returns>
        public Task<IMessage> FollowupFileAsync(Stream stream, string filename, string text = null, bool isTTS = false,
            IEnumerable<Embed> embeds = null, string username = null, string avatarUrl = null,
            RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null)
            => FollowupFileAsyncInternal(stream, filename, text, isTTS, embeds, username, avatarUrl, allowedMentions, options, isSpoiler);

        private async Task<IMessage> FollowupFileAsyncInternal(string filePath, string text, bool isTTS, IEnumerable<Embed> embeds,
            string username, string avatarUrl, AllowedMentions allowedMentions, RequestOptions options, bool isSpoiler)
        {
            var filename = Path.GetFileName(filePath);
            await using var file = File.OpenRead(filePath);
            return await FollowupFileAsyncInternal(file, filename, text, isTTS, embeds, username, avatarUrl, allowedMentions, options, isSpoiler).ConfigureAwait(false);
        }

        private async Task<IMessage> FollowupFileAsyncInternal(Stream stream, string filename, string text, bool isTTS, IEnumerable<Embed> embeds,
            string username, string avatarUrl, AllowedMentions allowedMentions, RequestOptions options, bool isSpoiler)
        {
            var args = new UploadWebhookFileParams(stream) { Filename = filename, Content = text, IsTTS = isTTS, IsSpoiler = isSpoiler };
            if (username != null)
                args.Username = username;
            if (avatarUrl != null)
                args.AvatarUrl = avatarUrl;
            if (embeds != null)
                args.Embeds = embeds.Select(x => x.ToModel()).ToArray();
            if (allowedMentions != null)
                args.AllowedMentions = allowedMentions.ToModel();

            return await InteractionHelper.SendFollowupFileAsync(Discord.Rest, args, Token, Channel, options);
        }

        /// <returns>
        ///     A task that represents the asynchronous operation of deferring the interaction.
        /// </returns>
        public async Task DeferAsync(int? flags = null, RequestOptions options = null)
        {
            var response = new InteractionResponse
            {
                Type = InteractionResponseType.DeferredChannelMessageWithSource
            };

            if (flags.HasValue)
            {
                response.Data = new InteractionApplicationCommandCallbackData
                {
                    Flags = flags.Value
                };
            }

            await Discord.Rest.ApiClient.CreateInteractionResponse(response, Id, Token, options).ConfigureAwait(false);
        }

        IApplicationCommandInteractionData IDiscordInteraction.Data => Data;
        IGuild IDiscordInteraction.Guild => Guild;
    }
}
