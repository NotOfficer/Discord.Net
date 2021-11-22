using System;

namespace Discord
{
    /// <summary> Defines the available permissions for a channel. </summary>
    [Flags]
    public enum GuildPermission : ulong
    {
        // General
        /// <summary>
        ///     Allows creation of instant invites.
        /// </summary>
        CreateInstantInvite = 0x00000001,
        /// <summary>
        ///     Allows kicking members.
        /// </summary>
        /// <remarks>
        ///     This permission requires the owner account to use two-factor
        ///     authentication when used on a guild that has server-wide 2FA enabled.
        /// </remarks>
        KickMembers         = 0x00000002,
        /// <summary>
        ///     Allows banning members.
        /// </summary>
        /// <remarks>
        ///     This permission requires the owner account to use two-factor
        ///     authentication when used on a guild that has server-wide 2FA enabled.
        /// </remarks>
        BanMembers          = 0x00000004,
        /// <summary>
        ///     Allows all permissions and bypasses channel permission overwrites.
        /// </summary>
        /// <remarks>
        ///     This permission requires the owner account to use two-factor
        ///     authentication when used on a guild that has server-wide 2FA enabled.
        /// </remarks>
        Administrator       = 0x00000008,
        /// <summary>
        ///     Allows management and editing of channels.
        /// </summary>
        /// <remarks>
        ///     This permission requires the owner account to use two-factor
        ///     authentication when used on a guild that has server-wide 2FA enabled.
        /// </remarks>
        ManageChannels      = 0x00000010,
        /// <summary>
        ///     Allows management and editing of the guild.
        /// </summary>
        /// <remarks>
        ///     This permission requires the owner account to use two-factor
        ///     authentication when used on a guild that has server-wide 2FA enabled.
        /// </remarks>
        ManageGuild         = 0x00000020,
        /// <summary>
        ///     Allows for viewing of guild insights
        /// </summary>
        ViewGuildInsights   = 0x00080000,

        // Text
        /// <summary>
        ///     Allows for the addition of reactions to messages.
        /// </summary>
        AddReactions        = 0x00000040,
        /// <summary>
        ///     Allows for viewing of audit logs.
        /// </summary>
        ViewAuditLog        = 0x00000080,
        [Obsolete("Use ViewChannel instead.")]
        ReadMessages        = ViewChannel,
        /// <summary>
        ///     Allows guild members to view a channel, which includes reading messages in text channels.
        /// </summary>
        ViewChannel         = 0x00000400,
        /// <summary>
        ///     Allows for sending messages in a channel.
        /// </summary>
        SendMessages        = 0x00000800,
        /// <summary>
        ///     Allows for sending of text-to-speech messages.
        /// </summary>
        SendTTSMessages     = 0x00001000,
        /// <summary>
        ///     Allows for deletion of other users messages.
        /// </summary>
        /// <remarks>
        ///     This permission requires the owner account to use two-factor
        ///     authentication when used on a guild that has server-wide 2FA enabled.
        /// </remarks>
        ManageMessages      = 0x00002000,
        /// <summary>
        ///     Allows links sent by users with this permission will be auto-embedded.
        /// </summary>
        EmbedLinks          = 0x00004000,
        /// <summary>
        ///     Allows for uploading images and files.
        /// </summary>
        AttachFiles         = 0x00008000,
        /// <summary>
        ///     Allows for reading of message history.
        /// </summary>
        ReadMessageHistory  = 0x00010000,
        /// <summary>
        ///     Allows for using the @everyone tag to notify all users in a channel, and the @here tag to notify all
        ///     online users in a channel.
        /// </summary>
        MentionEveryone     = 0x00020000,
        /// <summary>
        ///     Allows the usage of custom emojis from other servers.
        /// </summary>
        UseExternalEmojis   = 0x00040000,
        /// <summary>
        ///     Allows the usage of slash commands.
        /// </summary>
        UseSlashCommands    = 0x80000000,

        // Voice
        /// <summary>
        ///     Allows for joining of a voice channel.
        /// </summary>
        Connect             = 0x00100000,
        /// <summary>
        ///     Allows for speaking in a voice channel.
        /// </summary>
        Speak               = 0x00200000,
        /// <summary>
        ///     Allows for muting members in a voice channel.
        /// </summary>
        MuteMembers         = 0x00400000,
        /// <summary>
        ///     Allows for deafening of members in a voice channel.
        /// </summary>
        DeafenMembers       = 0x00800000,
        /// <summary>
        ///     Allows for moving of members between voice channels.
        /// </summary>
        MoveMembers         = 0x01000000,
        /// <summary>
        ///     Allows for using voice-activity-detection in a voice channel.
        /// </summary>
        UseVAD              = 0x02000000,
        /// <summary>
        ///     Allows for using priority speaker in a voice channel
        /// </summary>
        PrioritySpeaker     = 0x00000100,
        /// <summary>
        ///     Allows video streaming in a voice channel.
        /// </summary>
        Stream              = 0x00000200,

        // General 2
        /// <summary>
        ///     Allows for modification of own nickname.
        /// </summary>
        ChangeNickname      = 0x04000000,
        /// <summary>
        ///     Allows for modification of other users nicknames.
        /// </summary>
        ManageNicknames     = 0x08000000,
        /// <summary>
        ///     Allows management and editing of roles.
        /// </summary>
        /// <remarks>
        ///     This permission requires the owner account to use two-factor
        ///     authentication when used on a guild that has server-wide 2FA enabled.
        /// </remarks>
        ManageRoles         = 0x10000000,
        /// <summary>
        ///     Allows management and editing of webhooks.
        /// </summary>
        /// <remarks>
        ///     This permission requires the owner account to use two-factor
        ///     authentication when used on a guild that has server-wide 2FA enabled.
        /// </remarks>
        ManageWebhooks      = 0x20000000,
        /// <summary>
        ///     Allows management and editing of emojis.
        /// </summary>
        /// <remarks>
        ///     This permission requires the owner account to use two-factor
        ///     authentication when used on a guild that has server-wide 2FA enabled.
        /// </remarks>
        ManageEmojis        = 0x40000000
    }
}
