using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using GvG_Core_Bot.Main;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Resources;
using System.Threading;

namespace GvG_Core_Bot
{
    class Program
    {
        static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _comserv;
        private DependencyMap _map;

        private CancellationTokenSource ExitToken;

        // TODO: add to json
        IList<ulong> ownerID = new ulong[] { 285802056038744076 };
        char char_prefix = '%';

        Config _config = null;
        ResourceManager ResMsg = GvG_Core_Bot.Main.Messages.ResultMessages.ResourceManager;

        public async Task Start()
        {
            // Install a Proper Exit Token
            ExitToken = new CancellationTokenSource();
   
            // Install JSON values;
            _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("./config.json"));

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
            _map = new Discord.Commands.DependencyMap();
            _map.Add(_client);
            _map.Add(new GvG_GameService(_map));
            _map.Add(ExitToken);
            _map.Add(_config);

            // Install Commands
            await InstallCommands();

            // Log In
            await _client.LoginAsync(TokenType.Bot, _config.token);
            await _client.StartAsync();

            // Console WriteOut
            Console.WriteLine("Service started.");

            await Task.Delay(-1, ExitToken.Token).ContinueWith(_ => { });
            ExitToken.Dispose();
        }

        private async Task InstallCommands()
        {
            _client.MessageReceived += HandleCommand;
            _client.Ready += Debug_Login;
            await _comserv.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task Debug_Login()
        {
#if DEBUG
            foreach (var chan in _client.Guilds)
            {
                var gvg_chan = chan.TextChannels.FirstOrDefault((x)=>x.Name==_config.pub_gvg_chan_name);
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
            var result = await _comserv.ExecuteAsync(context, argPos, _map);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
            else
            {
                foreach (var ownerID in _config.owner_ids) {
                    var dm_owner = (await _client.GetDMChannelsAsync()).FirstOrDefault((x) => x.Recipient.Id == ownerID);
                    await dm_owner.SendMessageAsync($"<{context.User.Username}> sent: {context.Message}");
                }
            }
        }
    }
}