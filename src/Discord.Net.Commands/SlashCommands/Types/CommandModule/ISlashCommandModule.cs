namespace Discord.SlashCommands
{
    public interface ISlashCommandModule
    {
        void SetContext(IDiscordInteraction interaction);

        //void BeforeExecute(CommandInfo command);

        //void AfterExecute(CommandInfo command);

        //void OnModuleBuilding(CommandService commandService, ModuleBuilder builder);
    }
}
