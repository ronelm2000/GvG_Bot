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

namespace GvG_Core_Bot.Main.Commands
{
    public class Die : ModuleBase<SocketCommandContext>
    {
        private readonly Config _config;
        private readonly GvG_GameService _gameService;
        private readonly CancellationTokenSource _exitflag;
        private ResourceManager ResMsg;

        public Die(Config config, GvG_GameService gameService, CancellationTokenSource exitFlag)
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
            if (!_config.owner_ids.Contains(Context.User.Id)) return;

            foreach (var gvg_game in _gameService.ListOfGames.Select((x)=>x.Value))
            {
                await gvg_game.CancelGame(Context.User, true);
            }

            foreach (var chan in Context.Client.Guilds)
            {
                var gvg_chan = chan.TextChannels.FirstOrDefault((x) => x.Name == _config.pub_gvg_chan_name);
                if (gvg_chan != null) await gvg_chan.SendMessageAsync(ResMsg.GetString("BotIsOffline"));
            }
            //await Context.Client.LogoutAsync();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Context.Client.StopAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _exitflag.CancelAfter(5000);
        }
        
    }
}
