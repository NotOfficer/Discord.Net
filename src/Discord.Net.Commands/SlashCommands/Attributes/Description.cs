using System;

namespace Discord.SlashCommands
{
    /// <summary>
    /// An Attribute that gives the command parameter a description.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Description : Attribute
    {
        public const string DefaultDescription = "No description.";
        /// <summary>
        ///     The description of this slash command parameter.
        /// </summary>
        public readonly string Value;

        /// <summary>
        ///     Tells the <see cref="SlashCommandService"/> that this parameter has a description.
        /// </summary>
        /// <param name="value">The value of this slash command description.</param>
        public Description(string value)
        {
            Value = value;
        }
    }

}
