using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GvG_Core_Bot.Main.Commands
{
    [Name("Time")]
    public class TimeManip : ModuleBase<SocketCommandContext>
    {
        GvG_GameService GameService { get; set; }

        public TimeManip (GvG_GameService service)
        {
            GameService = service;
        }

        [Command("pause"), Summary("Pauses the current GvG game."), Alias("p")]
        [RequireContext(ContextType.Guild)]
        public async Task PauseGame ()
        {
            GvGGame CurrentGame = GameService.GetServerInstance(Context.Guild);
            if (CurrentGame.Status == GameStatus.ActionPhase || CurrentGame.Status == GameStatus.IdlePhase)
            {
                await ReplyAsync("", false, CurrentGame.Pause(Context));
            } else
            {
                await ReplyAsync("The Game isn't even running.");
            }
        }

        [Command("continue"), Summary("Continues the currently paused GvG game."), Alias("co")]
        [RequireContext(ContextType.Guild)]
        public async Task ContinueGame()
        {
            GvGGame CurrentGame = GameService.GetServerInstance(Context.Guild);
            if (CurrentGame.Status == GameStatus.ActionPhase || CurrentGame.Status == GameStatus.IdlePhase)
            {
                await ReplyAsync("", false, CurrentGame.Continue(Context));
            }
            else
            {
                await ReplyAsync("The Game isn't even running.");
            }
        }
}
}
