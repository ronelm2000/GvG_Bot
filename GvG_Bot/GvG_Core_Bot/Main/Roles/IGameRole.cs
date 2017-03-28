using Discord;
using GvG_Core_Bot.Main.Positioning;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static GvG_Core_Bot.Main.Roles.RoleContext;

namespace GvG_Core_Bot.Main.Roles
{
    public interface IGameRole
    {  
        List<IGameRole> WhoKnowsYou { get; set; }
        IUser RolePlayer { get; set; }
        GameRoleStatus Status { get; set; }
        IEnumerable<Vector2D> Positions { get; set; }
        Map GameMap { get; set; }
        CommandPriority CurrentPriority { get; set; }

        event RoleEvent Died;
        event RoleEvent Revealed;

        Task Patrol(Vector2D[] newPos, CommandPriority commandedPrio);
        Task Scout(Vector2D[] newPos, CommandPriority commandedPrio);
        Task Perform_ActionPhase();
        Task Perform_IdlePhase();
        void TellPatrolStatus(IEnumerable<Vector2D> enumerable, IEnumerable<Vector2D> destroyed_tiles, IEnumerable<Vector2D> poisoned_tiles);
    }

    public enum GameRoleStatus
    {
        Normal,
        Protected,
        Poisoned,
        Dead,
        Jailed,
        Sakura_4,
        Sakura_3,
        Sakura_2,
        Sakura_1,
        Invisible,
        CivilianImmunity,
        CivilianImmunity_Gaia,
        CivilianImmunity_Guardian
    }

    public enum CommandPriority
    {
        Public,
        Channel,
        DM,
        Order
    }

    public struct Vector2D
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Vector2D(int x, int y)
        {
            X = x;
            Y = y;
        }


        static Vector2D Zero {  get => new Vector2D(0,0); }
        static Vector2D Up { get => new Vector2D(0, 1); }
        static Vector2D Down { get => new Vector2D(0, -1); }
        static Vector2D Left { get => new Vector2D(-1, 0); }
        static Vector2D Right { get => new Vector2D(1, 0); }

        public static Vector2D operator + (Vector2D one, Vector2D two) => new Vector2D(one.X+one.Y,two.X+two.Y);
    }
}
