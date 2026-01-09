using System;

namespace Discord.SlashCommands
{
    /// <summary>
    ///     Defines the current as being a group of slash commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandGroup : Attribute
    {
        /// <summary>
        ///     The name of this slash command.
        /// </summary>
        public string groupName;

        /// <summary>
        ///     The description of this slash command.
        /// </summary>
        public string description;

        /// <summary>
        ///     Tells the <see cref="SlashCommandService"/> that this class/function is a slash command.
        /// </summary>
        /// <param name="groupName">The name of this command group.</param>
        /// <param name="description">The description of this command group.</param>
        public CommandGroup(string groupName, string description = "No description.")
        {
            this.groupName = groupName.ToLower();
            this.description = description;
        }
    }
}
