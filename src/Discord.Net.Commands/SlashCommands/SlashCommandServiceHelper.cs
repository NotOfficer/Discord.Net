using Discord.WebSocket;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using ParameterInfo = System.Reflection.ParameterInfo;

namespace Discord.SlashCommands
{
    internal static class SlashCommandServiceHelper
    {
        private static readonly Type _slashCommandModuleType = typeof(ISlashCommandModule);
        private static readonly Type _commandGroupType = typeof(CommandGroup);
        private static readonly Type _globalType = typeof(Global);
        private static readonly Type _requiredType = typeof(Required);
        private static readonly Type _descriptionType = typeof(Description);
        private static readonly Type _choiceType = typeof(Choice);
        private static readonly Type _slashCommandType = typeof(SlashCommand);
        private static readonly Type _parameterNameType = typeof(ParameterName);
        private static readonly Type _intType = typeof(int);
        private static readonly Type _intNullableType = typeof(int?);
        private static readonly Type _stringType = typeof(string);
        private static readonly Type _boolType = typeof(int);
        private static readonly Type _boolNullableType = typeof(int?);
        private static readonly Type _guildChannelType = typeof(IGuildChannel);
        private static readonly Type _rolelType = typeof(IRole);
        private static readonly Type _guildUserType = typeof(IGuildUser);

        /// <summary>
        /// Get all of the valid user-defined slash command modules 
        /// </summary>
        public static async Task<IReadOnlyList<TypeInfo>> GetValidModuleClasses(Assembly assembly, SlashCommandService service)
        {
            var result = new List<TypeInfo>();

            foreach (var typeInfo in assembly.DefinedTypes)
            {
                if (!IsValidModuleDefinition(typeInfo))
                    continue;

                // To simplify our lives, we need the modules to be public.
                if (typeInfo.IsPublic || typeInfo.IsNestedPublic)
                    result.Add(typeInfo);
                else
                    await service.Logger.WarningAsync($"Found class {typeInfo.FullName} as a valid SlashCommand Module, but it's not public!");
            }

            return result;
        }

        private static bool IsValidModuleDefinition(Type typeInfo)
        {
            // See if the base type (SlashCommandInfo<T>) implements interface ISlashCommandModule
            return !typeInfo.IsAbstract && !typeInfo.ContainsGenericParameters
                   //&& typeInfo.BaseType.GetInterfaces().Any(n => n == SlashCommandModuleType)
                   && _slashCommandModuleType.IsAssignableFrom(typeInfo)
                   //&& typeInfo.IsAssignableFrom(SlashcommandmoduleType)
                   && !typeInfo.GetCustomAttributes(_commandGroupType).Any();
        }

        /// <summary>
        /// Create an instance of each user-defined module
        /// </summary>
        public static Dictionary<Type, SlashModuleInfo> InstantiateModules(IReadOnlyList<TypeInfo> types, SlashCommandService slashCommandService, IServiceProvider services)
        {
            var result = new Dictionary<Type, SlashModuleInfo>();
            // Here we get all modules thate are NOT sub command groups and instantiate them.
            foreach (var userModuleType in types)
            {
                //var userModuleTypeInfo = userModuleType.GetTypeInfo();

                var moduleInfo = new SlashModuleInfo(slashCommandService, services);
                moduleInfo.SetModuleType(userModuleType);

                // If they want a constructor with different parameters, this is the place to add them.
                //var instance = ReflectionUtils.CreateObject<ISlashCommandModule>(userModuleTypeInfo, slashCommandService, services);
                //object instance = userModuleType.GetConstructor(Type.EmptyTypes).Invoke(null);
                //moduleInfo.SetCommandModule(instance);
                moduleInfo.isGlobal = IsCommandModuleGlobal(userModuleType);

                moduleInfo.SetSubCommandGroups(InstantiateSubModules(userModuleType, moduleInfo, slashCommandService, services));
                result.Add(userModuleType, moduleInfo);
            }

            return result;
        }

        public static List<SlashModuleInfo> InstantiateSubModules(Type rootModule, SlashModuleInfo rootModuleInfo, SlashCommandService slashCommandService, IServiceProvider services)
        {
            // Instantiate all of the nested modules.
            var commandGroups = new List<SlashModuleInfo>();

            foreach (var commandGroupType in rootModule.GetNestedTypes())
            {
                if (!TryGetCommandGroupAttribute(commandGroupType, out var commandGroup))
                    continue;
                //var commandGroupTypeInfo = commandGroupType.GetTypeInfo();

                var groupInfo = new SlashModuleInfo(slashCommandService, services);
                groupInfo.SetModuleType(commandGroupType);

                //var instance = ReflectionUtils.CreateObject<ISlashCommandModule>(commandGroupTypeInfo, slashCommandService, services);
                //object instance = commandGroupType.GetConstructor(Type.EmptyTypes).Invoke(null);
                //groupInfo.SetCommandModule(instance);

                groupInfo.MakeCommandGroup(commandGroup, rootModuleInfo);
                groupInfo.MakePath();
                groupInfo.isGlobal = IsCommandModuleGlobal(commandGroupType);

                groupInfo.SetSubCommandGroups(InstantiateSubModules(commandGroupType, groupInfo, slashCommandService, services));
                commandGroups.Add(groupInfo);
            }

            return commandGroups;
        }

        public static bool TryGetCommandGroupAttribute(Type module, out CommandGroup commandGroup)
        {
            if (!module.IsPublic && !module.IsNestedPublic)
            {
                commandGroup = null;
                return false;
            }

            var commandGroupAttributes = module.GetCustomAttributes(_commandGroupType);
            var groupAttributes = commandGroupAttributes as Attribute[] ?? commandGroupAttributes.ToArray();

            switch (groupAttributes.Length)
            {
                case 0:
                    commandGroup = null;
                    return false;
                case > 1:
                    throw new Exception($"Too many CommandGroup attributes on a single class ({module.FullName}). It can only contain one!");
                default:
                    commandGroup = groupAttributes[0] as CommandGroup;
                    return true;
            }
        }

        public static bool IsCommandModuleGlobal(Type userModuleType)
        {
            // Verify that we only have one [Global] attribute
            var slashCommandAttributes = userModuleType.GetCustomAttributes(_globalType);
            var slashCommandAttributesCount = slashCommandAttributes.Count();

            if (slashCommandAttributesCount > 1)
            {
                throw new Exception("Too many Global attributes on a single method. It can only contain one!");
            }
            // And at least one
            return slashCommandAttributesCount != 0;
        }

        /// <summary>
        /// Prepare all of the commands and register them internally.
        /// </summary>
        public static Dictionary<string, SlashCommandInfo> CreateCommandInfos(IReadOnlyList<TypeInfo> types, Dictionary<Type, SlashModuleInfo> moduleDefs, SlashCommandService slashCommandService)
        {
            // Create the resulting dictionary ahead of time
            var result = new Dictionary<string, SlashCommandInfo>();
            // For each user-defined module ...
            foreach (var userModule in types)
            {
                // Get its associated information. If there isn't any it means something went wrong, but it's not a critical error.
                if (!moduleDefs.TryGetValue(userModule, out var moduleInfo))
                    continue;

                // Create the root-level commands
                var commandInfos = CreateSameLevelCommands(result, userModule, moduleInfo);
                moduleInfo.SetCommands(commandInfos);
                // Then create all of the command groups it has.
                CreateSubCommandInfos(result, moduleInfo.commandGroups, slashCommandService);
            }

            return result;
        }

        public static void CreateSubCommandInfos(Dictionary<string, SlashCommandInfo> result, List<SlashModuleInfo> subCommandGroups, SlashCommandService slashCommandService)
        {
            foreach (var subCommandGroup in subCommandGroups)
            {
                // Create the commands that is on the same hierarchical level as this ...
                var commandInfos = CreateSameLevelCommands(result, subCommandGroup.ModuleType, subCommandGroup);
                subCommandGroup.SetCommands(commandInfos);

                // ... and continue with the lower sub command groups.
                CreateSubCommandInfos(result, subCommandGroup.commandGroups, slashCommandService);
            }
        }

        private static List<SlashCommandInfo> CreateSameLevelCommands(Dictionary<string, SlashCommandInfo> result, Type userModule, SlashModuleInfo moduleInfo)
        {
            var commandMethods = userModule.GetMethods();
            var commandInfos = new List<SlashCommandInfo>();
            foreach (var commandMethod in commandMethods)
            {
                // Get the SlashCommand attribute
                if (!IsValidSlashCommand(commandMethod, out var slashCommand))
                    continue;

                // Create the delegate for the method we want to call once the user interacts with the bot.
                // We use a delegate because of the unknown number and type of parameters we will have.
                //Delegate delegateMethod = CreateDelegate(commandMethod, moduleInfo.userCommandModule);
                var commandInfo = new SlashCommandInfo(
                    moduleInfo,
                    slashCommand.Name,
                    slashCommand.Description,
                    // Generate the parameters. Due to it's complicated way the algorithm has been moved to its own function.
                    ConstructCommandParameters(commandMethod),
                    //userMethod: delegateMethod,
                    commandMethod,
                    IsCommandGlobal(commandMethod)
                );

                result.Add(commandInfo.Module.Path + SlashModuleInfo.PathSeperator + commandInfo.Name, commandInfo);
                commandInfos.Add(commandInfo);
            }

            return commandInfos;
        }

        /// <summary>
        /// Determines wheater a method can be clasified as a slash command
        /// </summary>
        private static bool IsValidSlashCommand(MemberInfo method, out SlashCommand slashCommand)
        {
            // Verify that we only have one [SlashCommand(...)] attribute
            var slashCommandAttributes = method.GetCustomAttributes(_slashCommandType);
            var commandAttributes = slashCommandAttributes as Attribute[] ?? slashCommandAttributes.ToArray();

            switch (commandAttributes.Length)
            {
                case > 1:
                    throw new Exception("Too many SlashCommand attributes on a single method. It can only contain one!");
                // And at least one
                case 0:
                    slashCommand = null;
                    return false;
                default:
                    // And return the first (and only) attribute
                    slashCommand = commandAttributes[0] as SlashCommand;
                    return true;
            }
        }

        /// <summary>
        /// Determins if the method has a [Global] Attribute.
        /// </summary>
        private static bool IsCommandGlobal(MemberInfo method)
        {
            // Verify that we only have one [Global] attribute
            var slashCommandAttributes = method.GetCustomAttributes(_globalType);
            var slashCommandAttributesCount = slashCommandAttributes.Count();

            if (slashCommandAttributesCount > 1)
            {
                throw new Exception("Too many Global attributes on a single method. It can only contain one!");
            }
            // And at least one
            return slashCommandAttributesCount != 0;
        }

        /// <summary>
        /// Process the parameters of this method, including all the attributes.
        /// </summary>
        private static List<SlashParameterInfo> ConstructCommandParameters(MethodInfo method)
        {
            // Prepare the final list of parameters
            var finalParameters = new List<SlashParameterInfo>();

            // For each mehod parameter ...
            // ex: ... MyCommand(string abc, int myInt)
            // `abc` and `myInt` are parameters
            foreach (var methodParameter in method.GetParameters())
            {
                var newParameter = new SlashParameterInfo();

                // Test for the [ParameterName] Attribute. If we have it, then use that as the name,
                // if not just use the parameter name as the option name.
                var customNameAttributes = methodParameter.GetCustomAttributes(_parameterNameType);
                var nameAttributes = customNameAttributes as Attribute[] ?? customNameAttributes.ToArray();
                newParameter.Name = nameAttributes.Length switch
                {
                    0 => methodParameter.Name,
                    > 1 => throw new Exception($"Too many ParameterName attributes on a single parameter ({method.Name} -> {methodParameter.Name}). It can only contain one!"),
                    _ => ((ParameterName)nameAttributes[0]).Name
                };

                // Get to see if it has a Description Attribute.
                // If it has
                // 0 -> then use the default description
                // 1 -> Use the value from that attribute
                // 2+ -> Throw an error. This shouldn't normaly happen, but we check for sake of sanity
                var descriptionAttributes = methodParameter.GetCustomAttributes(_descriptionType);
                var descriptions = descriptionAttributes as Attribute[] ?? descriptionAttributes.ToArray();
                newParameter.Description = descriptions.Length switch
                {
                    0 => Description.DefaultDescription,
                    > 1 => throw new Exception($"Too many Description attributes on a single parameter ({method.Name} -> {methodParameter.Name}). It can only contain one!"),
                    _ => ((Description)descriptions[0]).Value
                };

                // Set the Type of the parameter.
                // In the case of int and int? it returns the same type - INTEGER.
                // Same with bool and bool?.
                newParameter.Type = TypeFromMethodParameter(methodParameter);

                // If we have a nullble type (int? or bool?) mark it as such.
                newParameter.Nullable = GetNullableStatus(methodParameter);

                // Test for the [Required] Attribute
                var requiredAttributes = methodParameter.GetCustomAttributes(_requiredType);
                var requiredCount = requiredAttributes.Count();
                newParameter.Required = requiredCount switch
                {
                    1 => true,
                    > 1 => throw new Exception($"Too many Required attributes on a single parameter ({method.Name} -> {methodParameter.Name}). It can only contain one!"),
                    _ => newParameter.Required
                };

                // Test for the [Choice] Attribute
                // A parameter cna have multiple Choice attributes, and for each we're going to add it's key-value pair.
                foreach (var attribute in methodParameter.GetCustomAttributes(_choiceType))
                {
                    var choice = (Choice)attribute;

                    // If the parameter expects a string but the value of the choice is of type int, then throw an error.
                    if (newParameter.Type == ApplicationCommandOptionType.String)
                    {
                        if (string.IsNullOrEmpty(choice.StringValue))
                            throw new Exception($"Parameter ({method.Name} -> {methodParameter.Name}) is of type string, but choice is of type int!");

                        newParameter.AddChoice(choice.Name, choice.StringValue);
                    }

                    // If the parameter expects a int but the value of the choice is of type string, then throw an error.
                    if (newParameter.Type != ApplicationCommandOptionType.Integer)
                        continue;

                    if (choice.IntValue == null)
                        throw new Exception($"Parameter ({method.Name} -> {methodParameter.Name}) is of type int, but choice is of type string!");

                    newParameter.AddChoice(choice.Name, (int)choice.IntValue);
                }

                finalParameters.Add(newParameter);
            }
            return finalParameters;
        }

        /// <summary>
        /// Get the type of command option from a method parameter info.
        /// </summary>
        private static ApplicationCommandOptionType TypeFromMethodParameter(ParameterInfo methodParameter)
        {
            if (methodParameter.ParameterType == _intType || methodParameter.ParameterType == _intNullableType)
                return ApplicationCommandOptionType.Integer;
            if (methodParameter.ParameterType == _stringType)
                return ApplicationCommandOptionType.String;
            if (methodParameter.ParameterType == _boolType || methodParameter.ParameterType == _boolNullableType)
                return ApplicationCommandOptionType.Boolean;
            if (methodParameter.ParameterType.IsAssignableFrom(_guildChannelType))
                return ApplicationCommandOptionType.Channel;
            if (methodParameter.ParameterType.IsAssignableFrom(_rolelType))
                return ApplicationCommandOptionType.Role;
            if (methodParameter.ParameterType.IsAssignableFrom(_guildUserType))
                return ApplicationCommandOptionType.User;

            throw new Exception($"Got parameter type other than int, string, bool, guild, role, or user. {methodParameter.Name}");
        }

        /// <summary>
        /// Gets whater the parameter can be set as null, in the case that parameter type usually does not allow null.
        /// More specifically tests to see if it is a type of 'int?' or 'bool?',
        /// </summary>
        private static bool GetNullableStatus(ParameterInfo methodParameter)
        {
            return methodParameter.ParameterType == _intNullableType || methodParameter.ParameterType == _boolNullableType;
        }

        /// <summary>
        /// Creae a delegate from methodInfo. Taken from
        /// https://stackoverflow.com/a/40579063/8455128
        /// </summary>
        public static Delegate CreateDelegate(MethodInfo methodInfo, object target)
        {
            var isAction = methodInfo.ReturnType == typeof(void);
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);
            Func<Type[], Type> getType;

            if (isAction)
            {
                getType = Expression.GetActionType;
            }
            else
            {
                getType = Expression.GetFuncType;
                types = types.Concat(new[] { methodInfo.ReturnType });
            }

            return methodInfo.IsStatic
                ? Delegate.CreateDelegate(getType(types.ToArray()), methodInfo)
                : Delegate.CreateDelegate(getType(types.ToArray()), target, methodInfo.Name);
        }

        public static async Task RegisterCommands(DiscordSocketClient socketClient, Dictionary<Type, SlashModuleInfo> rootModuleInfos, IEnumerable<ulong> guildIds/*, CommandRegistrationOptions options*/)
        {
            // TODO: see how we should handle if user wants to register two commands with the same name, one global and one not.
            // Build the commands
            var builtCommands = BuildCommands(rootModuleInfos);

            var builtGlobalCommands = new List<SlashCommandCreationProperties>();
            var builtGuildCommands = new List<SlashCommandCreationProperties>();

            // And now register them. Globally if the 'Global' flag is set.
            // If not then just register them as guild commands on all of the guilds given to us.
            foreach (var builtCommand in builtCommands)
            {
                if (builtCommand.Global)
                {
                    builtGlobalCommands.Add(builtCommand);
                }
                else
                {
                    builtGuildCommands.Add(builtCommand);
                }
            }

            await socketClient.Rest.CreateGlobalCommands(builtGlobalCommands).ConfigureAwait(false);

            if (builtGuildCommands.Count != 0 && guildIds != null)
            {
                foreach (var guildId in guildIds)
                {
                    await socketClient.Rest.CreateGuildCommands(builtGuildCommands, guildId).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Build and return all of the commands this assembly contians.
        /// </summary>
        public static List<SlashCommandCreationProperties> BuildCommands(Dictionary<Type, SlashModuleInfo> rootModuleInfos)
        {
            var builtCommands = new List<SlashCommandCreationProperties>();

            foreach (var rootModuleInfo in rootModuleInfos.Values)
            {
                builtCommands.AddRange(rootModuleInfo.BuildCommands());
            }

            return builtCommands;
        }
    }
}
