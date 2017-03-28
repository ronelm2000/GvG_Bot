using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GvG_Core_Bot.Main.Commands
{
    public class TestCommand : ModuleBase
    {
        // ~say hello -> hello
        [Command("test"), Summary("Just a test."), Alias("t")]
        public async Task Say()
        {
            await ReplyAsync("$roll 6");
        }
    }
}