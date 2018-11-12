using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GvG_Core_Bot.Main;
using Discord.WebSocket;
using GvG_Core_Bot.Main.Commands.CustomAttributes;

namespace GvG_Core_Bot.Main.Commands
{
    [Name("Factions")]
    public class Factions : ModuleBase<SocketCommandContext>
    {
        [Command("join"), SummaryResx("JoinDesc"), Alias("j")]
        [RequireContext(ContextType.Guild)]
        /*[RequireBotPermission(
            GuildPermission.ManageRoles &
            GuildPermission.SendMessages &
            GuildPermission.ManageMessages &
            GuildPermission.ManageGuild &
            GuildPermission.MentionEveryone &
            GuildPermission.Administrator &
			GuildPermission.CreateInstantInvite)]*/
        public async Task JoinFaction(
            [Remainder, Summary("The faction to join.")] string faction
            )
        {

            if (Context.IsPrivate)
            {
                await ReplyAsync("Please post in a Discord server for this functionality.");
                return;
            }

            faction = FixFactionString(faction);

            var g_user = Context.User as SocketGuildUser;
            var all_roles = Context.Guild.Roles;

            var gvg_player_role = all_roles.First((x) => x.Name == "gvg player");
            var gvg_dead_player_role = all_roles.First((x) => x.Name == "gvg dead player");

            if (g_user.Roles.Any((x) => x.Id == gvg_dead_player_role.Id || x.Id == gvg_player_role.Id))
            {
                await ReplyAsync("You cannot change factions while in a GvG Game.");
                return;
            }

            var role_name = new string[] { "gaia", "guardian", "occult club" };
            var roles = role_name.Select((x) => all_roles.First((y) => y.Name == x));

            if (g_user.Roles.Any((x) => roles.Contains(x)))
            {
                await ReplyAsync("You're already in a faction!");
            } else
            {
                var sel_role = roles.ElementAt(Array.IndexOf(role_name, faction));
                await g_user.AddRoleAsync(sel_role);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Context.Guild.TextChannels.First((x) => x.Name == "general").SendMessageAsync($"{g_user.Mention} has joined the ranks of {sel_role.Mention}!");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        string FixFactionString(string unfixed)
        {
            if (string.IsNullOrWhiteSpace(unfixed)) return "null";
            unfixed = char.ToLower(unfixed[0]) + unfixed.Substring(1);
            if (unfixed.StartsWith("gu")) return "guardian";
            else if (unfixed.StartsWith("ga")) return "gaia";
            else if (unfixed.StartsWith("oc") || unfixed.StartsWith("oc")) return "occult club";
            return "null";
        }


        [Command("quit"), SummaryResx("QuitDesc"), Alias("q")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task QuitFaction()
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
            else if ((Context.User as SocketGuildUser).Roles.Any((x) => x.Name == "gaia" || x.Name == "guardian" || x.Name == "occult club"))
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
