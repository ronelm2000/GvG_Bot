using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using GvG_Core_Bot.Main.Positioning;

namespace GvG_Core_Bot.Main.Roles.Gaia
{
    class Kashima : IGameRole
    {
        public List<IGameRole> WhoKnowsYou { get; set; }
        public IUser RolePlayer { get; set; }
        public GameRoleStatus Status { get; set; }
        public IEnumerable<Vector2D> Positions { get; set; }
        public Map GameMap { get; set; }
        public CommandPriority CurrentPriority { get; set; }

		public int PhasePriority => 20;
		public Faction Faction => Faction.Gaia;
		public int HP => 5;

		public event RoleEvent Died;
        public event RoleEvent Revealed;

		event RoleEvent IGameRole.Died
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		event RoleEvent IGameRole.Revealed
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
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
