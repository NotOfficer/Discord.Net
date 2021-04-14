using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Discord.Commands.Builders
{
    /// <summary>
    ///     A class used to build slash commands.
    /// </summary>
    public class SlashCommandBuilder
    {
        /// <summary>
        ///     Returns the maximun length a commands name allowed by Discord
        /// </summary>
        public const int MaxNameLength = 32;
        /// <summary>
        ///     Returns the minimum length a commands name allowed by Discord
        /// </summary>
        public const int MinNameLength = 1;
        /// <summary>
        ///     Returns the maximum length of a commands description allowed by Discord.
        /// </summary>
        public const int MaxDescriptionLength = 100;
        /// <summary>
        ///     Returns the minimum length of a commands description allowed by Discord.
        /// </summary>
        public const int MinDescriptionLength = 1;
        /// <summary>
        ///     Returns the maximum count of command options allowed by Discord
        /// </summary>
        public const int MaxOptionsCount = 25;

        /// <summary>
        ///     The name of this slash command.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                Preconditions.NotNullOrEmpty(value, nameof(Name));
                Preconditions.AtLeast(value.Length, MinNameLength, nameof(Name));
                Preconditions.AtMost(value.Length, MaxNameLength, nameof(Name));

                // Discord updated the docs, this regex prevents special characters like @!$%(... etc,
                // https://discord.com/developers/docs/interactions/slash-commands#applicationcommand
                if (!Regex.IsMatch(value, @$"^[\w-]{{{MinNameLength},{MaxNameLength}}}$"))
                    throw new ArgumentException("Command names cannot contian any special characters or whitespaces!");

                _name = value;
            }
        }

        /// <summary>
        ///    A 1-100 length description of this slash command
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _description = null;
                    return;
                }

                Preconditions.AtLeast(value.Length, MinDescriptionLength, nameof(Description));
                Preconditions.AtMost(value.Length, MaxDescriptionLength, nameof(Description));

                _description = value;
            }
        }

        public ulong GuildId
        {
            get => _guildId ?? 0;
            set
            {
                if (value == 0)
                {
                    throw new ArgumentException("Guild ID cannot be 0!");
                }

                _guildId = value;

                if (isGlobal)
                    isGlobal = false;
            }
        }
        /// <summary>
        ///     Gets or sets the options for this command.
        /// </summary>
        public List<SlashCommandOptionBuilder> Options
        {
            get => _options;
            set
            {
                if (value != null && value.Count > MaxOptionsCount)
                    throw new ArgumentException($"Option count must be less than or equal to {MaxOptionsCount}.", nameof(Options));

                _options = value;
            }
        }

        private ulong? _guildId { get; set; }
        private string _name { get; set; }
        private string _description { get; set; }
        private List<SlashCommandOptionBuilder> _options { get; set; }

        internal bool isGlobal { get; set; }

        public SlashCommandCreationProperties Build()
        {
            var props = new SlashCommandCreationProperties
            {
                Name = Name,
                Description = Description,
            };

            if (Options != null && Options.Count != 0)
            {
                var options = new List<ApplicationCommandOptionProperties>();

                Options.ForEach(x => options.Add(x.Build()));

                props.Options = options;
            }

            return props;

        }

        /// <summary>
        ///     Makes this command a global application command .
        /// </summary>
        /// <returns>The current builder.</returns>
        public SlashCommandBuilder MakeGlobal()
        {
            isGlobal = true;
            return this;
        }

        /// <summary>
        ///     Makes this command a guild specific command.
        /// </summary>
        /// <param name="guildId">The Id of the target guild.</param>
        /// <returns>The current builder.</returns>
        public SlashCommandBuilder ForGuild(ulong guildId)
        {
            GuildId = guildId;
            return this;
        }

        public SlashCommandBuilder WithName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        ///     Sets the description of the current command.
        /// </summary>
        /// <param name="description">The description of this command.</param>
        /// <returns>The current builder.</returns>
        public SlashCommandBuilder WithDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        ///     Adds an option to the current slash command.
        /// </summary>
        /// <param name="name">The name of the option to add.</param>
        /// <param name="type">The type of this option.</param>
        /// <param name="description">The description of this option.</param>
        /// <param name="required">If this option is required for this command.</param>
        /// <param name="isDefault">If this option is the default option.</param>
        /// <param name="options">The options of the option to add.</param>
        /// <param name="choices">The choices of this option.</param>
        /// <returns>The current builder.</returns>
        public SlashCommandBuilder AddOption(string name, ApplicationCommandOptionType type,
           string description, bool required = true, bool isDefault = false, List<SlashCommandOptionBuilder> options = null, params ApplicationCommandOptionChoiceProperties[] choices)
        {
            // Make sure the name matches the requirements from discord
            Preconditions.NotNullOrEmpty(name, nameof(name));
            Preconditions.AtLeast(name.Length, MinNameLength, nameof(name));
            Preconditions.AtMost(name.Length, MaxNameLength, nameof(name));

            // Discord updated the docs, this regex prevents special characters like @!$%( and s p a c e s.. etc,
            // https://discord.com/developers/docs/interactions/slash-commands#applicationcommand
            if (!Regex.IsMatch(name, @$"^[\w-]{{{MinNameLength},{MaxNameLength}}}$"))
                throw new ArgumentException("Command name cannot contian any special characters or whitespaces!", nameof(name));

            // same with description
            Preconditions.NotNullOrEmpty(description, nameof(description));
            Preconditions.AtLeast(description.Length, 3, nameof(description));
            Preconditions.AtMost(description.Length, MaxDescriptionLength, nameof(description));

            // make sure theres only one option with default set to true
            if (isDefault && Options != null && Options.Any(x => x.Default))
                throw new ArgumentException("There can only be one command option with default set to true!", nameof(isDefault));

            var option = new SlashCommandOptionBuilder
            {
                Name = name,
                Type = type, // might break
                Description = description,
                Required = required,
                Default = isDefault,
                Options = options,
                Choices = choices != null && choices.Length != 0 ? new List<ApplicationCommandOptionChoiceProperties>(choices) : null
            };

            return AddOption(option);
        }

        /// <summary>
        ///     Adds an option to the current slash command.
        /// </summary>
        /// <param name="name">The name of the option to add.</param>
        /// <param name="type">The type of this option.</param>
        /// <param name="description">The description of this option.</param>
        /// <param name="required">If this option is required for this command.</param>
        /// <param name="isDefault">If this option is the default option.</param>
        /// <param name="choices">The choices of this option.</param>
        /// <returns>The current builder.</returns>
        public SlashCommandBuilder AddOption(string name, ApplicationCommandOptionType type,
            string description, bool required = true, bool isDefault = false, params ApplicationCommandOptionChoiceProperties[] choices)
            => AddOption(name, type, description, required, isDefault, null, choices);

        /// <summary>
        ///     Adds an option to the current slash command.
        /// </summary>
        /// <param name="name">The name of the option to add.</param>
        /// <param name="type">The type of this option.</param>
        /// <param name="description">The sescription of this option.</param>
        /// <returns>The current builder.</returns>
        public SlashCommandBuilder AddOption(string name, ApplicationCommandOptionType type, string description)
            => AddOption(name, type, description, options: null, choices: null);

        /// <summary>
        ///     Adds an option to this slash command.
        /// </summary>
        /// <param name="option">The option to add.</param>
        /// <returns>The current builder.</returns>
        public SlashCommandBuilder AddOption(SlashCommandOptionBuilder option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option), "Option cannot be null");

            Options ??= new List<SlashCommandOptionBuilder>();

            if (Options.Count >= MaxOptionsCount)
                throw new ArgumentOutOfRangeException(nameof(Options), $"Cannot have more than {MaxOptionsCount} options!");

            Options.Add(option);
            return this;
        }
        /// <summary>
        ///     Adds a collection of options to the current slash command.
        /// </summary>
        /// <param name="options">The collection of options to add.</param>
        /// <returns>The current builder.</returns>
        public SlashCommandBuilder AddOptions(params SlashCommandOptionBuilder[] options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options), "Options cannot be null!");

            if (options.Length == 0)
                throw new ArgumentException("Options cannot be empty!", nameof(options));

            Options ??= new List<SlashCommandOptionBuilder>();

            if (Options.Count + options.Length > MaxOptionsCount)
                throw new ArgumentOutOfRangeException(nameof(options), $"Cannot have more than {MaxOptionsCount} options!");

            Options.AddRange(options);
            return this;
        }
    }

    /// <summary>
    ///     Represents a class used to build options for the <see cref="SlashCommandBuilder"/>.
    /// </summary>
    public class SlashCommandOptionBuilder
    {
        /// <summary>
        ///     Returns the maximun length a commands choice name allowed by Discord
        /// </summary>
        public const int MaxChoiceNameLength = 100;
        /// <summary>
        ///     Returns the minimum length a commands choice name allowed by Discord
        /// </summary>
        public const int MinChoiceNameLength = 1;
        /// <summary>
        ///     The maximum number of choices allowed by Discord.
        /// </summary>
        public const int MaxChoiceCount = 25;

        private string _name;
        private string _description;

        /// <summary>
        ///     The name of this option.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                Preconditions.NotNullOrEmpty(value, nameof(Name));
                Preconditions.AtLeast(value.Length, SlashCommandBuilder.MinNameLength, nameof(Name));
                Preconditions.AtMost(value.Length, SlashCommandBuilder.MaxNameLength, nameof(Name));

                // Discord updated the docs, this regex prevents special characters like @!$%(... etc,
                // https://discord.com/developers/docs/interactions/slash-commands#applicationcommandoption
                if (!Regex.IsMatch(value, @$"^[\w-]{{{SlashCommandBuilder.MinNameLength},{SlashCommandBuilder.MaxNameLength}}}$"))
                    throw new ArgumentException("Command choice names cannot contian any special characters or whitespaces!");

                _name = value;
            }
        }

        /// <summary>
        ///     The description of this option.
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _description = null;
                    return;
                }

                Preconditions.AtLeast(value.Length, SlashCommandBuilder.MinDescriptionLength, nameof(Description));
                Preconditions.AtMost(value.Length, SlashCommandBuilder.MaxDescriptionLength, nameof(Description));

                _description = value;
            }
        }

        /// <summary>
        ///     The type of this option.
        /// </summary>
        public ApplicationCommandOptionType Type { get; set; }

        /// <summary>
        ///     The first required option for the user to complete. only one option can be default.
        /// </summary>
        public bool Default { get; set; }

        /// <summary>
        ///     <see langword="true"/> if this option is required for this command, otherwise <see langword="false"/>.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        ///     choices for string and int types for the user to pick from.
        /// </summary>
        public List<ApplicationCommandOptionChoiceProperties> Choices { get; set; }

        /// <summary>
        ///     If the option is a subcommand or subcommand group type, this nested options will be the parameters.
        /// </summary>
        public List<SlashCommandOptionBuilder> Options { get; set; }

        /// <summary>
        ///     Builds the current option.
        /// </summary>
        /// <returns>The build version of this option</returns>
        public ApplicationCommandOptionProperties Build()
        {
            var isSubType = Type is ApplicationCommandOptionType.SubCommand or ApplicationCommandOptionType.SubCommandGroup;

            if (Type == ApplicationCommandOptionType.SubCommandGroup && (Options == null || !Options.Any()))
                throw new ArgumentException("SubCommandGroups must have at least one option", nameof(Options));

            if (!isSubType && Options != null && Options.Any())
                throw new ArgumentException(nameof(Options), $"Cannot have options on {Type} type");

            return new ApplicationCommandOptionProperties
            {
                Name = Name,
                Description = Description,
                Default = Default,
                Required = Required,
                Type = Type,
                Options = Options != null ? new List<ApplicationCommandOptionProperties>(Options.Select(x => x.Build())) : null,
                Choices = Choices
            };
        }

        /// <summary>
        ///     Adds a sub
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public SlashCommandOptionBuilder AddOption(SlashCommandOptionBuilder option)
        {
            Options ??= new List<SlashCommandOptionBuilder>();

            if (Options.Count >= SlashCommandBuilder.MaxOptionsCount)
                throw new ArgumentOutOfRangeException(nameof(Choices), $"There can only be {SlashCommandBuilder.MaxOptionsCount} options per sub command group!");

            if (option == null)
                throw new ArgumentNullException(nameof(option), "Option cannot be null");

            Options.Add(option);
            return this;
        }

        public SlashCommandOptionBuilder AddChoice(string name, int value)
        {
            Choices ??= new List<ApplicationCommandOptionChoiceProperties>();

            if (Choices.Count >= MaxChoiceCount)
                throw new ArgumentOutOfRangeException(nameof(Choices), $"Cannot add more than {MaxChoiceCount} choices!");

            Choices.Add(new ApplicationCommandOptionChoiceProperties
            {
                Name = name,
                Value = value
            });

            return this;
        }
        public SlashCommandOptionBuilder AddChoice(string name, string value)
        {
            Choices ??= new List<ApplicationCommandOptionChoiceProperties>();

            if (Choices.Count >= MaxChoiceCount)
                throw new ArgumentOutOfRangeException(nameof(Choices), $"Cannot add more than {MaxChoiceCount} choices!");

            Choices.Add(new ApplicationCommandOptionChoiceProperties
            {
                Name = name,
                Value = value
            });

            return this;
        }

        public SlashCommandOptionBuilder WithName(string name)
        {
            Name = name;
            return this;
        }

        public SlashCommandOptionBuilder WithDescription(string description)
        {
            Description = description;
            return this;
        }

        public SlashCommandOptionBuilder WithRequired(bool value)
        {
            Required = value;
            return this;
        }

        public SlashCommandOptionBuilder WithDefault(bool value)
        {
            Default = value;
            return this;
        }
        public SlashCommandOptionBuilder WithType(ApplicationCommandOptionType type)
        {
            Type = type;
            return this;
        }
    }
}
