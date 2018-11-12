using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using GvG_Core_Bot.Main;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Resources;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace GvG_Core_Bot
{
    class Program
    {
        static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _comserv;
        //private DependencyMap _map;

        private CancellationTokenSource ExitToken;

        // TODO: add to json
        IList<ulong> ownerID = new ulong[] { 285802056038744076 };
        char char_prefix = '%';

        IConfiguration _config = null;
        Config __config = null;
        IServiceProvider _serv = null;

        ResourceManager ResMsg = GvG_Core_Bot.Main.Messages.ResultMessages.ResourceManager;

        public async Task Start()
        {
            // Install a Proper Exit Token
            ExitToken = new CancellationTokenSource();
            // Install JSON values;
            _config = BuildConfig();

            // Install Services
            _client = new DiscordSocketClient(new DiscordSocketConfig()
             {
#if DEBUG
                LogLevel = LogSeverity.Debug
#else            
                LogLevel = LogSeverity.Info
#endif
             });
            _client.Log += (message) =>
            {
                Console.WriteLine($"{message.Source}: {message.ToString()}");
                return Task.CompletedTask;
            };

            _comserv = new CommandService();
            _comserv.Log += LogAsync;
            __config = new Config();
            _config.Bind(__config);
            _serv = ConfigureServices();

            /*
            _map = new Discord.Commands.DependencyMap();
            _map.Add(_client);
            _map.Add(new GvG_GameService(_map));
            _map.Add(ExitToken);
            _map.Add(_config);
            */

            // Install Commands
            await InstallCommands();

            // Log In
            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            // Console WriteOut
            Console.WriteLine("Service started.");

            await Task.Delay(-1, ExitToken.Token).ContinueWith(_ => { });
            ExitToken.Dispose();
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            if (logMessage.Exception is CommandException cmdException)
            {
                Console.WriteLine($"Exception occurred: {cmdException}");
            }
            if (logMessage.Exception != null)
            {
                Console.WriteLine($"Exception occurred: {logMessage.Exception}");
            }
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                 // Base
                .AddOptions()
                .AddSingleton(_client)
                .AddSingleton(_comserv)
                .AddSingleton<GvG_GameService>()
                .AddSingleton(ExitToken)
                //.AddSingleton<CommandHandlingService>()
                // Logging
                .AddLogging()
                //.AddSingleton<LogService>()
                // Extra
                .AddSingleton(_config)
                .AddSingleton(__config)
                // Add additional services here...
                .BuildServiceProvider();
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
        }

        private async Task InstallCommands()
        {
            _client.MessageReceived += HandleCommand;
            _client.Ready += Debug_Login;
            await _comserv.AddModulesAsync(Assembly.GetEntryAssembly(), ConfigureServices());
        }

        private async Task Debug_Login()
        {
#if DEBUG
            foreach (var chan in _client.Guilds)
            {
                var gvg_chan = chan.TextChannels.FirstOrDefault((x)=>x.Name==_config["pub_gvg_chan_name"]);
                if (gvg_chan != null) await gvg_chan.SendMessageAsync(ResMsg.GetString("BotIsOnline"));
            }
#endif
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix

            if (!(message.HasCharPrefix(char_prefix, ref argPos) || 
                    message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;

            // Create a Command Context
            var context = new SocketCommandContext(_client, message);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed succesfully)
            var result = await _comserv.ExecuteAsync(context, argPos, _serv);
            if (!result.IsSuccess)
                // I need to improve this part by logging in the actual Exception used.
                // How do I do that?
                await context.Channel.SendMessageAsync(result.ErrorReason);
            else
            {
                foreach (var ownerID in __config.owner_ids) {
                    var dm_owner = (await _client.GetDMChannelsAsync()).FirstOrDefault((x) => x.Recipient.Id == ownerID);
                    await dm_owner.SendMessageAsync($"<{context.User.Username}> sent: {context.Message}");
                }
            }
        }
    }
}