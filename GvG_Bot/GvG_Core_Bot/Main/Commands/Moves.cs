using Discord;
using Discord.Commands;
using GvG_Core_Bot.Main.Commands.CustomAttributes;
using GvG_Core_Bot.Main.Messages;
using GvG_Core_Bot.Main.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GvG_Core_Bot.Main.Commands
{
    [Name("Movement")]
    [SummaryResx("MovementDesc")]
    public class Moves : ModuleBase<SocketCommandContext>
    {
        private GvG_GameService GameService;
        private GvGGame Game;

        public Moves (GvG_GameService serv)
        {
            GameService = serv;
        }

        [Command("patrol"), SummaryResx("PatrolDesc"), Alias("p")]
        public async Task Patrol(
            [SummaryResx("PatrolDesc_RemainderSummary")]
            string patrolString,
            [Remainder, SummaryResx("PatrolDesc_ServerSummary")]
            string serverName = ""
            )
        {
            if (Context.Guild != null && GameService.ListOfGames.ContainsKey(Context.Guild.Id))
            {
                Game = GameService.GetServerInstance(Context.Guild);
            } else {
                Game = GameService.ListOfGames.First((x) => Context.Client.GetGuild(x.Key).Name.Contains(serverName)).Value;
                if (Game == null)
                {
                    await ReplyAsync("You aren't in a GvG Game.");
                    return;
                }
            }
            try
            {
                var translatedMove = Vector2D.Parse(patrolString);
                await ReplyAsync("", false, (await Game.PatrolQueue(await Game.GvG_Player.Guild.GetUserAsync(Context.User.Id), Context, translatedMove)).Build());
            } catch (FormatException e)
            {
                await ReplyAsync("", false, new EmbedBuilder()
                    .WithTitle(ResultMessages.PatrolError)
                    .WithDescription(ResultMessages.PatrolError_ParseError).Build());
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        [Command("display_map"), SummaryResx("DisplayMapDesc"), Alias("dm")]
        public async Task DisplayMap(
            [SummaryResx("DisplayMapDesc_TileSummary")]
            string tileNumber = "")
        {
            await ReplyAsync("", false, (await Game.GetMapStatus(Context.Guild.GetUser(Context.User.Id), Context.Channel, Context.IsPrivate)).Build());
            /*:one::ok::ng::ok::ok::ok::ok::ok::ok:
:two::ok::ok::ok::ok::ok::ok::ok::ok:
:three::ok::ok::ok::ok::ok::ok::ok::ok:
:four::ok::ok::ok::ok::ok::ok::ok::ok:
:five::ok::ok::ok::ok::ok::ok::ok::ok:
:six::ok::ok::ok::ok::ok::ok::ok::ok:
:seven::ok::ok::ok::ok::ok::ok::ok::ok:
:eight::ok::ok::ok::ok::ok::ok::ok::ok:" */
        }
    }
}
