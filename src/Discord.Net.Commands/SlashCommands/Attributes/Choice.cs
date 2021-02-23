using System;

namespace Discord.SlashCommands
{
    /// <summary>
    ///     Defines the parameter as a choice.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class Choice : Attribute
    {
        /// <summary>
        ///     The internal value of this choice.
        /// </summary>
        public readonly string StringValue;

        /// <summary>
        ///     The internal value of this choice.
        /// </summary>
        public readonly int? IntValue;

        /// <summary>
        ///     The display value of this choice.
        /// </summary>
        public readonly string Name;

        public Choice(string name, string value)
        {
            Name = name;
            StringValue = value;
        }

        public Choice(string name, int value)
        {
            Name = name;
            IntValue = value;
        }
    }
}
