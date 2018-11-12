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
        List<IGameRole> WhoKnowsYou { get; }
        IUser RolePlayer { get; set; }
        GameRoleStatus Status { get; set; }
        IEnumerable<Vector2D> Positions { get; set; }
        Map GameMap { get; set; }
        CommandPriority CurrentPriority { get; set; }
		int PhasePriority { get; }
		Faction Faction { get; }
		int HP { get; }

		event RoleEvent Died;
		event RoleEvent Revealed;

		Task Patrol(Vector2D[] newPos, CommandPriority commandedPrio);
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

	public enum Faction
	{
		Gaia,
		Guardian,
		OC,
		Civilian
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
        public static readonly char[] Separators = { ',', '\\', '-', '~' };
        public static readonly char[] XYSeparators = { 'x', 'X' };
        public static IEnumerable<Vector2D> Parse(string str)
        {
            for (int i = 0; i < str.Length && i >= 0; i = str.IndexOfAny(Separators,i))
            {
                var substr = String.Empty;
                if (i > 0) i++;
                if (str.IndexOfAny(Separators, i) == -1) substr = str.Substring(i);
                else substr = str.Substring(i, str.IndexOfAny(Separators, i) - i);
                yield return new Vector2D(
                    int.Parse(substr.Substring(0, substr.IndexOfAny(XYSeparators))),
                    int.Parse(substr.Substring(substr.IndexOfAny(XYSeparators) + 1))
                    );
            }
        }
    }
}
