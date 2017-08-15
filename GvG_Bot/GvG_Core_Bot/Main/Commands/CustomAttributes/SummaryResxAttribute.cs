using Discord.Commands;
using GvG_Core_Bot.Main.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace GvG_Core_Bot.Main.Commands.CustomAttributes
{
    class SummaryResxAttribute : SummaryAttribute
    {
        public SummaryResxAttribute (string resName) : base (CommandMessages.ResourceManager.GetString(resName))
        {
        }
    }
}
