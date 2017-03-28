using Discord;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace GvG_Core_Bot.Main.Commands
{
    static class StringCorrections
    {
        
        public static string FixFactionString(this string unfixed)
        {
            if (string.IsNullOrWhiteSpace(unfixed)) return "null";
            unfixed = char.ToLower(unfixed[0]) + unfixed.Substring(1);
            if (unfixed.StartsWith("gu")) return "guardian";
            else if (unfixed.StartsWith("ga")) return "gaia";
            else if (unfixed.StartsWith("oc") || unfixed.StartsWith("oc")) return "occult club";
            return "null";
        }

        public static string GetFaction(this IGuildUser user)
        {
            return user.Guild.Roles.First((x) =>
                (x.Name == "guardian" || x.Name == "gaia" || x.Name == "occult club") && user.RoleIds.Contains(x.Id)
                )?.Name ?? "civilian";
        }
    }
}
