using Discord;
using Discord.Commands;
using GvG_Core_Bot.Main.Commands.CustomAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GvG_Core_Bot.Main.Commands
{
	[Name("Moderation")]
	public class Moderation : ModuleBase<SocketCommandContext>
	{
		[Command("muteall"), SummaryResx("MuteAllDesc"), Alias("ma")]
		[RequireContext(ContextType.Guild)]
		[RequireBotPermission(
			GuildPermission.ManageRoles
			)]
		public async Task MuteAll (
			[Remainder, SummaryResx("MuteAll_RoleDesc")]IRole role)
		{
			// get mute role
			IRole Silenced = Context.Guild.Roles.First(x => x.Name == "silenced");
			foreach (IGuildUser user in Context.Guild.Users.Where(x => x.Roles.Contains(role)))
			{
				if (!user.RoleIds.Contains(Silenced.Id))
				{
					// mute him
					await user.AddRoleAsync(Silenced);
				}
			}
			if (role.IsMentionable)
			{
				await ReplyAsync("Everyone in the " + role.Mention + " role is muted.");
			} else
			{
				await ReplyAsync("All users of the role [" + role.Name + "] have been muted.");
			}
		}

		[Command("unmuteall"), SummaryResx("UnMuteAllDesc"), Alias("uma")]
		[RequireContext(ContextType.Guild)]
		[RequireBotPermission(
	GuildPermission.ManageRoles
	)]
		public async Task UnMuteAll(
	[Remainder, SummaryResx("MuteAll_RoleDesc")]IRole role)
		{
			// get mute role
			IRole Silenced = Context.Guild.Roles.First(x => x.Name == "silenced");
			foreach (IGuildUser user in Context.Guild.Users.Where(x => x.Roles.Contains(role)))
			{
				if (user.RoleIds.Contains(Silenced.Id))
				{                   
					// unmute him
					await user.RemoveRoleAsync(Silenced);
				}
			}
			if (role.IsMentionable)
			{
				await ReplyAsync("Everyone in the " + role.Mention + " role is unmuted.");
			}
			else
			{
				await ReplyAsync("All users of the role [" + role.Name + "] have been unmuted.");
			}
		}
	}
}
