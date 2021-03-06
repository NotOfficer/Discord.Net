using Discord.Commands.Builders;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Discord.SlashCommands
{
    public class SlashModuleInfo
    {
        public const string PathSeperator = "//";
        public const string RootModuleName = "TOP";
        public const string RootCommandPrefix = RootModuleName + PathSeperator;

        public SlashModuleInfo(SlashCommandService service, IServiceProvider services)
        {
            Service = service;
            ServiceProvider = services;
        }

        public bool isCommandGroup { get; set; } = false;
        public CommandGroup commandGroupInfo { get; set; }

        public SlashModuleInfo parent { get; set; }
        public List<SlashModuleInfo> commandGroups { get; set; }
        public string Path { get; set; } = RootModuleName;

        public bool isGlobal { get; set; } = false;
        /// <summary>
        ///     Gets the command service associated with this module.
        /// </summary>
        public SlashCommandService Service { get; }

        /// <summary>
        ///     Gets the service rovider associated with this module.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }
        /// <summary>
        ///     Gets a read-only list of commands associated with this module.
        /// </summary>
        public List<SlashCommandInfo> Commands { get; private set; }

        /// <summary>
        /// The user command module defined as the interface ISlashCommandModule
        /// Used to set context.
        /// </summary>
        //public ISlashCommandModule userCommandModule;

        public TypeInfo ModuleType { get; private set; }

        public void SetCommands(List<SlashCommandInfo> commands)
        {
            if (this.Commands == null)
            {
                this.Commands = commands;
            }
        }
        //public void SetCommandModule(ISlashCommandModule userCommandModule)
        //{
        //    if (this.userCommandModule == null)
        //    {
        //        this.userCommandModule = userCommandModule;
        //    }
        //}
        public void SetModuleType(TypeInfo typeInfo)
        {
            ModuleType = typeInfo;
        }

        public void SetModuleType(Type typeInfo)
        {
            ModuleType = typeInfo.GetTypeInfo();
        }

        public void MakeCommandGroup(CommandGroup commandGroupInfo, SlashModuleInfo parent)
        {
            isCommandGroup = true;
            this.commandGroupInfo = commandGroupInfo;
            this.parent = parent;
        }
        public void SetSubCommandGroups(List<SlashModuleInfo> subCommandGroups)
        {
            // this.commandGroups = new List<SlashModuleInfo>(subCommandGroups);
            this.commandGroups = subCommandGroups;
        }

        public void MakePath()
        {
            Path = parent.Path + SlashModuleInfo.PathSeperator + commandGroupInfo.groupName;
        }

        public List<SlashCommandCreationProperties> BuildCommands()
        {
            List<SlashCommandCreationProperties> builtCommands = new List<SlashCommandCreationProperties>();
            foreach (var command in Commands)
            {
                var builtCommand = command.BuildCommand();
                if (isGlobal || command.IsGlobal)
                {
                    builtCommand.Global = true;
                }
                builtCommands.Add(builtCommand);
            }
            foreach(var commandGroup in commandGroups)
            {
                var builtCommand = commandGroup.BuildTopLevelCommandGroup();
                if (isGlobal || commandGroup.isGlobal)
                {
                    builtCommand.Global = true;
                }
                builtCommands.Add(builtCommand);
            }
            return builtCommands;
        }

        public SlashCommandCreationProperties BuildTopLevelCommandGroup()
        {
            SlashCommandBuilder builder = new SlashCommandBuilder();
            builder.WithName(commandGroupInfo.groupName);
            builder.WithDescription(commandGroupInfo.description);
            foreach (var command in Commands)
            {
                builder.AddOption(command.BuildSubCommand());
            }
            foreach (var commandGroup in commandGroups)
            {
                builder.AddOption(commandGroup.BuildNestedCommandGroup());
            }
            return builder.Build();
        }

        private SlashCommandOptionBuilder BuildNestedCommandGroup()
        {
            SlashCommandOptionBuilder builder = new SlashCommandOptionBuilder();
            builder.WithName(commandGroupInfo.groupName);
            builder.WithDescription(commandGroupInfo.description);
            builder.WithType(ApplicationCommandOptionType.SubCommandGroup);
            foreach (var command in Commands)
            {
                builder.AddOption(command.BuildSubCommand());
            }
            foreach (var commandGroup in commandGroups)
            {
                builder.AddOption(commandGroup.BuildNestedCommandGroup());
            }

            return builder;
        }
    }
}
