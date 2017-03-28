using Discord;
using GvG_Core_Bot.Main.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace GvG_Core_Bot.Main.Positioning
{
    public class Presence
    {
        public IGameRole Role { get; set; }
        public bool Invisible { get; set; }
        public int Attack { get; set; }
        public int Protect { get; set; }


        public static Presence CannotBeSeenExceptWithEyes(IGameRole role) => new Presence()
        {
            Role = role,
            Invisible = true,
            Attack = 0,
            Protect = 0
        };

        internal Presence Duplicate() => new Presence()
        {
            Role = this.Role,
            Invisible = this.Invisible,
            Attack = this.Attack,
            Protect = this.Protect
        };
    }


}
