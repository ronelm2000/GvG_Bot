using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GvG_Core_Bot.Main.Commands
{

    public class Role : ModuleBase<SocketCommandContext>
    {
        private GvG_GameService GameService { get; set; }
        public Role (GvG_GameService gvg_serv)
        {
            GameService = gvg_serv;
        }

        [Command("role"), Summary("Gives out your role."), Alias("gr")]
        [RequireBotPermission(
            GuildPermission.ManageRoles &
            GuildPermission.SendMessages &
            GuildPermission.ManageMessages &
            GuildPermission.ManageGuild &
            GuildPermission.MentionEveryone &
            GuildPermission.Administrator)]
        public async Task GetRole (string customCommand = "")
        {
            if (Context.IsPrivate)
            {
                // Get all roles in all games
                foreach (var game in GameService.ListOfGames)
                {
                    if (game.Value.Status >= GameStatus.IdlePhase)
                    {
                        var pm_chan = await Context.User.GetOrCreateDMChannelAsync();
                        await pm_chan.SendMessageAsync("", false, game.Value.FindRole(Context.Client.GetGuild(game.Key).GetUser(Context.User.Id), false));
                    }
                }
            } else if (Context.Guild != null)
            {
                // get game for guild
                var hasGame = GameService.ListOfGames.ContainsKey(Context.Guild.Id);
                if (hasGame)
                {
                    var game = GameService.GetServerInstance(Context.Guild);
                    if (game.Status < GameStatus.IdlePhase)
                    {
                        // stuff
                        await ReplyAsync("The GvG Game hasn't even started yet!");
                        return;
                    }
                    if (customCommand == "all" && game.Hoster == Context.User)
                    {
                        await Context.Channel.SendMessageAsync("", false, game.FindAllRoles());
                    } else {
                        await Context.Channel.SendMessageAsync("", false, game.FindRole(Context.Guild.GetUser(Context.User.Id), true));
                    }
                }
                else
                    await Context.Channel.SendMessageAsync("You're not on a GvG Game.");
            }
        }


    }
}
