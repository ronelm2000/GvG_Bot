using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GvG_Core_Bot.Main;
using System.Threading;
using System.Resources;
using Microsoft.Extensions.Configuration;
using GvG_Core_Bot.Main.Messages;
using Discord.Rest;

namespace GvG_Core_Bot.Main.Commands
{
    [Name("Server")]
    public class Server : ModuleBase<SocketCommandContext>
    {
        private readonly Config _config;
        private readonly GvG_GameService _gameService;
        private readonly CancellationTokenSource _exitflag;
        private ResourceManager ResMsg;

        public Server(Config config, GvG_GameService gameService, CancellationTokenSource exitFlag)
        {
            _config = config;
            _gameService = gameService;
            _exitflag = exitFlag;
            ResMsg = Main.Messages.ResultMessages.ResourceManager;
        }

        // ~say hello -> hello
        [Command("die"), Summary("Closes the server.")]
        public async Task Say()
        {
            if (!(_config.owner_ids.Contains(Context.User.Id))) return;

            foreach (var gvg_game in _gameService.ListOfGames.Select((x)=>x.Value))
            {
                await gvg_game?.CancelGame(Context.User, true);
            }

            foreach (var chan in Context.Client.Guilds)
            {
                var gvg_chan = chan.TextChannels.FirstOrDefault((x) => x.Name == _config.pub_gvg_chan_name);
                await (gvg_chan?.SendMessageAsync(ResultMessages.BotIsOffline) ?? Task.FromResult<RestUserMessage>(null));
            }
            //await Context.Client.LogoutAsync();

            await Task.WhenAny(
                Context.Client.StopAsync(),
                Task.Delay(5000)
            );

            _exitflag.Cancel();
        }
        
    }
}
