using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GvG_Core_Bot.Main;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using GvG_Core_Bot.Main.Commands.CustomAttributes;

namespace GvG_Core_Bot.Main.Commands
{
    [Name("Game Prep")]
    public class GamePrep : ModuleBase<SocketCommandContext>
    {
        GvG_GameService GameService { get; set; }

        public GamePrep(IServiceProvider _serv)
        {
            this.GameService = (GvG_GameService)_serv.GetService(typeof(GvG_GameService));
        }

        [Command("join_game"), Summary("Joins the next game of Gaia vs Guardians."), Alias("jg")]
        [RequireBotPermission(ChannelPermission.ManagePermissions | ChannelPermission.ManageChannel)]
        [RequireContext(ContextType.Guild)]
        public async Task SayJoinGame()
        {
            var result = await GameService.GetServerInstance(Context.Guild).Join((Context.User as IGuildUser));
            if (result == JoinResponse.Success)
            {
                if (Context.Channel.Name != "public_gvg") await ReplyAsync("You have sucessfully joined the next game of Gaia vs Guardians; Good luck!");
                await Context.Guild.TextChannels.First((x) => x.Name == "public_gvg").SendMessageAsync($"{Context.User.Mention} has joined for the next GvG!");
            }
            else if (result == JoinResponse.NoGameYet)
            {
                await ReplyAsync("There is no game of GvG yet; you must wait for someone to host the game.");
            }
            else if (result == JoinResponse.AlreadyJoined)
            {
                await ReplyAsync("You've already joined! :shock:");
            }
            else
            {
                await ReplyAsync("Input failed for some reason. Please contact the Bot Owner so he can look into it.");
            }
        }
        
        [Command("create_game"), Summary("Create a game of Gaia vs Guardians."), Alias("cg")]
        [RequireContext(ContextType.Guild)]
        public async Task SayCreateGame()
        {
            GvGGame CurrentGame = GameService.GetServerInstance(Context.Guild);
            if (CurrentGame.Status > GameStatus.GamePreparation)
            {
                await ReplyAsync("The Game has already started!");
                return;
            }
            if (CurrentGame.Status == GameStatus.GamePreparation)
            {
                await ReplyAsync("The Game has already been created!");
                return;
            }

            var pub_chan = Context.Guild.TextChannels.First((x) => x.Name == "public_gvg");
            var gua_chan = Context.Guild.TextChannels.First((x) => x.Name == "guardian");
            var gai_chan = Context.Guild.TextChannels.First((x) => x.Name == "gaia");
            var oc_chan = Context.Guild.TextChannels.First((x) => x.Name == "occult_club");

            await GameService.GetServerInstance(Context.Guild)?
                .CreateGame(Context.User, Context.Guild,
                pub_chan,
                gua_chan,
                gai_chan,
                oc_chan);
        }

        [Command("cancel_game"), SummaryResx("CancelGameDesc"), Alias("cancel", "can")]
        [RequireContext(ContextType.Guild)]
        public async Task SayCancelGame()
        {
            var CurrentGame = GameService.GetServerInstance(Context.Guild);
            if (CurrentGame.Status > GameStatus.ActionPhase)
            {
                await ReplyAsync("The game has already ended!");
            }
            else if (CurrentGame.Status == GameStatus.Cancelled)
            {
                await ReplyAsync("There is no game to cancel!");
            }
            else
            {
                await CurrentGame.CancelGame(Context.User);
            }
        }

        [Command("status"), SummaryResx("StatusDesc"), Alias("s")]
        public async Task SayStatus(
            [Summary("Optional, this will try to show the exact status of the game.")]
            bool showExactGameStatus = false,
            [Summary("Optional, this will try to show the exact status of all games.")]
            bool showBotOwnerStatus = false
            )
        {
            var CurrentGame = GameService.GetServerInstance(Context.Guild);
             EmbedBuilder StatusEmbed = CurrentGame.GetFullStatus(showExactGameStatus);
            await ReplyAsync("", false, StatusEmbed);
            if (showBotOwnerStatus)
            {
                // still in progres
            }
        }

        [Command("start"), Summary("Starts the game.")]
        public async Task SayStart(
            )
        {
            var CurrentGame = GameService.GetServerInstance(Context.Guild);
            var response = await CurrentGame.StartGame(Context.User);
            if (response != null) await ReplyAsync("", false, response);
            await Context.Message.DeleteAsync();
        }
    }
}
