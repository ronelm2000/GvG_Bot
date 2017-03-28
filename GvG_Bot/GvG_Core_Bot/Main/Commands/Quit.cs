using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GvG_Core_Bot.Main.Commands
{
    public class Quit : ModuleBase<SocketCommandContext>
    {
        [Command("quit"), Summary("Quits your current faction."), Alias("q")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task Say()
        {
            if (Context.IsPrivate)
            {
                await ReplyAsync("Please post in a Discord server for this functionality.");
                return;
            }
            var Gaia = Context.Guild.Roles.First((x) => x.Name == "gaia");
            var Guardian = Context.Guild.Roles.First((x) => x.Name == "guardian");
            var Occult_Club = Context.Guild.Roles.First((x) => x.Name == "occult club");

            if ((Context.User as SocketGuildUser).Roles.Any((x) => x.Name == "gvg player" || x.Name == "gvg dead player"))
            {
                await ReplyAsync("You cannot change factions while in a GvG Game.");
            }
            else if ((Context.User as SocketGuildUser).Roles.Any((x)=>x.Name=="gaia"||x.Name=="guardian"||x.Name=="occult club"))
            {
                var allRPRoles = new IRole[] { Gaia, Guardian, Occult_Club };
                var Prev_Role = (Context.User as SocketGuildUser).Roles.First((r) => allRPRoles.Contains(r));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                (Context.User as SocketGuildUser).RemoveRolesAsync(new IRole[] { Gaia, Guardian, Occult_Club });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                await Context.Guild.TextChannels.First((x) => x.Name == "general").SendMessageAsync($"{Context.User.Mention} has quit {Prev_Role.Mention}");
            }
        }
    }
}
