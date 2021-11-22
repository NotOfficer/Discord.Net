using System;

namespace Discord.SlashCommands
{
    /// <summary>
    /// An Attribute that gives the command parameter a custom name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParameterName : Attribute
    {
        /// <summary>
        ///     The name of this slash command parameter.
        /// </summary>
        public readonly string Name;

        /// <summary>
        ///     Tells the <see cref="SlashCommandService"/> that this parameter has a custom name.
        /// </summary>
        /// <param name="name">The name of this slash command.</param>
        public ParameterName(string name)
        {
            Name = name;
        }
    }
}
