using System;

namespace Discord.SlashCommands
{
    /// <summary>
    ///     Defines the current class or function as a slash command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SlashCommand : Attribute
    {
        /// <summary>
        ///     The name of this slash command.
        /// </summary>
        public readonly string Name;

        /// <summary>
        ///     The description of this slash command.
        /// </summary>
        public readonly string Description;

        /// <summary>
        ///     Tells the <see cref="SlashCommandService"/> that this class/function is a slash command.
        /// </summary>
        /// <param name="name">The name of this slash command.</param>
        /// <param name="description">The description of this slash command.</param>
        public SlashCommand(string name, string description = "No description.")
        {
            Name = name.ToLower();
            Description = description;
        }
    }
}
