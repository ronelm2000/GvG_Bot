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
    [Name("Help")]
    public class Help : ModuleBase<SocketCommandContext>
    {
        CommandService serv;
        Config conf;
        public Help (CommandService service, Config config)
        {
            serv = service;
            conf = config;
        }

        // ~say hello -> hello
        [Command("test"), Summary("Just a test."), Alias("t")]
        public async Task TestCom()
        {
            await ReplyAsync("$roll 6");
        }

        [Command("help")]
        [SummaryResx("HelpDesc")]
        [Alias("h")]
        public async Task HelpCommand(
            [Remainder, SummaryResx("HelpDesc_SearchSummary")]
            string searchTerms = "")
        {
            string message = string.Empty;
            //CommandService cs = new CommandService();
            //await cs.AddModulesAsync(Assembly.GetEntryAssembly());
            if (searchTerms == "")
            {
                foreach (var command in serv.Commands)
                {
                    string aliases = (command.Aliases.Count > 1) ? (" [" + command.Aliases.Skip(1).Aggregate((x, y) => x + " / " + y) + "]") : "";
                    message += string.Format("**{0}**{2}: {1}{3}",
                        conf.char_prefix + command.Name,
                        (command.Summary.Length > 104) ?
                            (command.Summary.Substring(0, 100) + ((command.Summary.Length > 100) ? "..." : "")) :
                            command.Summary,
                            aliases,
                        Environment.NewLine);
                }

                await ReplyAsync("", false, new EmbedBuilder()
                    .WithTitle("Total List of Commands")
                    .WithDescription(message));
            } else
            {
                var moduleCommands = serv.Commands.Where(x => x.Module.Name.ToLower() == searchTerms.ToLower());
                if (!moduleCommands.Any()) moduleCommands = serv.Commands.Where(x => x.Aliases.Where(y => y.Contains(searchTerms)).Any());
                if (!moduleCommands.Any()) moduleCommands = serv.Commands.Where(x => x.Summary.Contains(searchTerms));
                if (!moduleCommands.Any()) moduleCommands = serv.Commands.Where(x => x.Parameters.Where(y => y.Summary.Contains(searchTerms)).Any());
                await ReplyAsync("", false, IndexCommands(moduleCommands));
            }
        }

        private EmbedBuilder IndexCommands(IEnumerable<CommandInfo> commands)
        {
            var message = string.Empty;
            var arrayCom = commands.ToArray();
            if (arrayCom.Length == 0) message += "[0 Results]";
            else message += $"[{arrayCom.Length} Result"+((arrayCom.Length==1)?"":"s")+"]" + Environment.NewLine;
            foreach (var command in arrayCom)
            {
                string aliases = (command.Aliases.Count > 1) ? (" [" + command.Aliases.Skip(1).Aggregate((x, y) => x + " / " + y) + "]") : "";
                string summary = (command.Summary.Length > 104 && arrayCom.Length > 2) ?
                        (command.Summary.Substring(0, 100) + ((command.Summary.Length > 100) ? "..." : "")) :
                        command.Summary;
                message += string.Format("**{0}**{2}: {1}{3}",
                    conf.char_prefix+command.Name,
                    summary,
                    aliases,
                    Environment.NewLine);
            }
            return new EmbedBuilder()
                .WithTitle("Command Search")
                .WithDescription(message);
        }
    }
}