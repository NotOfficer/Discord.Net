using System.Collections.Generic;

namespace Discord
{
    /// <summary>
    ///     A class used to create slash commands.
    /// </summary>
    public class SlashCommandCreationProperties
    {
        /// <summary>
        ///     The name of this command.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///    The discription of this command.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     If the command should be defined as a global command.
        /// </summary>
        public bool Global { get; set; }

        /// <summary>
        ///     Gets or sets the options for this command.
        /// </summary>
        public Optional<List<ApplicationCommandOptionProperties>> Options { get; set; }
    }
}
