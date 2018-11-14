using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using GvG_Core_Bot.Main.Positioning;

namespace GvG_Core_Bot.Main.Roles.Civilian
{
    class NormalCivilian : IGameRole
    {
        public List<IGameRole> WhoKnowsYou { get; set; }
        public IUser RolePlayer { get; set; }
        public GameRoleStatus Status { get; set; }
        public IEnumerable<Vector2D> Positions { get; set; }
        public Map GameMap { get; set; }
        public CommandPriority CurrentPriority { get; set; }

		public int PhasePriority => 1;
		public Faction Faction => Faction.Guardian;
		public int HP => 5;

		public event RoleEvent Died;
        public event RoleEvent Revealed;

		event RoleEvent IGameRole.Died
		{
            add => Died += value;
            remove => Died -= value;
		}

		event RoleEvent IGameRole.Revealed
		{
            add => Revealed += value;
            remove => Revealed -= value;
		}

		public Task Patrol(Vector2D[] newPos, CommandPriority commandedPrio)
        {
            throw new NotImplementedException();
        }

        public Task Perform_ActionPhase()
        {
            throw new NotImplementedException();
        }

        public Task Perform_IdlePhase()
        {
            throw new NotImplementedException();
        }

        public Task Scout(Vector2D[] newPos, CommandPriority commandedPrio)
        {
            throw new NotImplementedException();
        }

        public void TellPatrolStatus(IEnumerable<Vector2D> enumerable, IEnumerable<Vector2D> destroyed_tiles, IEnumerable<Vector2D> poisoned_tiles)
        {
            throw new NotImplementedException();
        }
    }
}
