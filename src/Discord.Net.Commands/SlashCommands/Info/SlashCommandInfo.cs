using Discord.Commands;
using Discord.Commands.Builders;
using Discord.WebSocket;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord.SlashCommands
{
    public class SlashCommandInfo
    {
        /// <summary>
        ///     Gets the module that the command belongs in.
        /// </summary>
        public SlashModuleInfo Module { get; }
        /// <summary>
        ///     Gets the name of the command.
        /// </summary>
        public string Name { get; }
        /// <summary>
        ///     Gets the name of the command.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// The parameters we are expecting - an extension of SlashCommandOptionBuilder
        /// </summary>
        public List<SlashParameterInfo> Parameters { get; }

        public bool IsGlobal { get; }
        ///// <summary>
        ///// The user method as a delegate. We need to use Delegate because there is an unknown number of parameters
        ///// </summary>
        //public Delegate userMethod;
        ///// <summary>
        ///// The callback that we call to start the delegate.
        ///// </summary>
        //public Func<object[], Task<IResult>> callback;

        public MethodInfo MethodInfo { get; }

        //public SlashCommandInfo(SlashModuleInfo module, string name, string description,List<SlashParameterInfo> parameters , Delegate userMethod , bool isGlobal = false)
        public SlashCommandInfo(SlashModuleInfo module, string name, string description, List<SlashParameterInfo> parameters, MethodInfo methodInfo, bool isGlobal = false)
        {
            Module = module;
            Name = name;
            Description = description;
            Parameters = parameters;
            //this.userMethod = userMethod;
            IsGlobal = isGlobal;
            MethodInfo = methodInfo;
            //this.callback = new Func<object[], Task<IResult>>(async (args) =>
            //{
            //    // Try-catch it and see what we get - error or success
            //    try
            //    {
            //        await Task.Run(() =>
            //        {
            //            userMethod.DynamicInvoke(args);
            //        }).ConfigureAwait(false);
            //    }
            //    catch(Exception e)
            //    {
            //        return ExecuteResult.FromError(e);
            //    }
            //    return ExecuteResult.FromSuccess();

            //});
        }

        /// <summary>
        /// Execute the function based on the interaction data we get.
        /// </summary>
        /// <param name="data">Interaction data from interaction</param>
        /// <param name="interaction">SocketInteraction</param>
        public async Task<IResult> ExecuteAsync(IReadOnlyCollection<SocketInteractionDataOption> data, SocketInteraction interaction)
        {
            // Array of arguments to be passed to the Delegate
            var args = new object[Parameters.Count];

            try
            {
                // For each parameter to try find its coresponding DataOption based on the name.
                // !!! names from `data` will always be lowercase regardless if we defined the command with any
                // number of upercase letters !!!
                for (var i = 0; i < Parameters.Count; i++)
                {
                    var parameter = Parameters[i];

                    if (TryGetInteractionDataOption(data, parameter.Name, out var dataOption))
                    {
                        args[i] = parameter.Parse(dataOption);
                    }
                }
            }
            catch (Exception e)
            {
                return ExecuteResult.FromError(e);
            }

            var instance = ReflectionUtils.CreateObject<ISlashCommandModule>(Module.ModuleType, Module.Service, Module.ServiceProvider);
            instance.SetContext(interaction);

            var delegateMethod = SlashCommandServiceHelper.CreateDelegate(MethodInfo, instance);

            var callback = new Func<object[], Task<IResult>>(async callbackArgs =>
            {
                // Try-catch it and see what we get - error or success
                try
                {
                    await Task.Run(() => { delegateMethod.DynamicInvoke(callbackArgs); }).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    return ExecuteResult.FromError(e);
                }

                return ExecuteResult.FromSuccess();

            });

            return await callback.Invoke(args).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the interaction data from the name of the parameter we want to fill in.
        /// </summary>
        private static bool TryGetInteractionDataOption(IReadOnlyCollection<SocketInteractionDataOption> data, string name, out SocketInteractionDataOption dataOption)
        {
            if (data == null)
            {
                dataOption = null;
                return false;
            }

            foreach (var option in data)
            {
                if (option.Name != name.ToLower())
                    continue;

                dataOption = option;
                return true;
            }

            dataOption = null;
            return false;
        }

        /// <summary>
        ///  Build the command and put it in a state in which we can use to define it to Discord.
        /// </summary>
        public SlashCommandCreationProperties BuildCommand()
        {
            var builder = new SlashCommandBuilder();
            builder.WithName(Name);
            builder.WithDescription(Description);
            builder.Options = new List<SlashCommandOptionBuilder>();

            foreach (var parameter in Parameters)
            {
                builder.AddOptions(parameter);
            }

            return builder.Build();
        }

        /// <summary>
        ///  Build the command AS A SUBCOMMAND and put it in a state in which we can use to define it to Discord.
        /// </summary>
        public SlashCommandOptionBuilder BuildSubCommand()
        {
            var builder = new SlashCommandOptionBuilder();
            builder.WithName(Name);
            builder.WithDescription(Description);
            builder.WithType(ApplicationCommandOptionType.SubCommand);
            builder.Options = new List<SlashCommandOptionBuilder>();

            foreach (var parameter in Parameters)
            {
                builder.AddOption(parameter);
            }

            return builder;
        }
    }
}
