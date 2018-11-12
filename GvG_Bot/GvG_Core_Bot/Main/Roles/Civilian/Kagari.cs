using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using GvG_Core_Bot.Main.Positioning;
using System.Linq;

namespace GvG_Core_Bot.Main.Roles.Civilian
{
    class Kagari : IGameRole
    {
        public List<IGameRole> WhoKnowsYou { get; set; }
        public IUser RolePlayer { get; set; }
        public GameRoleStatus Status { get; set; }
        public IEnumerable<Vector2D> Positions { get; set; }
        public Map GameMap { get; set; }
        public CommandPriority CurrentPriority { get; set; }

        public int FondMemories { get; set; }

		public int PhasePriority => throw new NotImplementedException();

		public Faction Faction => throw new NotImplementedException();

		public int HP => throw new NotImplementedException();

		public Kagari ()
        {
        }

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

		public event RoleEvent Died;
        public event RoleEvent Revealed;

        public async Task Patrol(Vector2D[] newPosition, CommandPriority prio)
        {
            if (prio > CurrentPriority)
                Positions = newPosition.Take(1); // take only the first position
            await Task.CompletedTask;
        }
        public async Task Scout(Vector2D[] newPos, CommandPriority prio)
        {
            // do not do anything; Kagari doesn't scout.
            await Task.CompletedTask;
        }

        void TellPatrolStatus(IEnumerable<Vector2D> sucsess_tiles, IEnumerable<Vector2D> destroyed_tiles, IEnumerable<Vector2D> poisoned_tiles)
        {

        }

        public async Task Perform_ActionPhase()
        {
            // perform the scout.
            await GameMap.Place(new Presence[] { Presence.CannotBeSeenExceptWithEyes(this) }, Positions);
        }

        public async Task Perform_IdlePhase()
        {
            if (FondMemories >= 15)
            {
                // declare OC Win
                await Task.CompletedTask;
            }
            //Map.Place(this, Positions);
        }

        void IGameRole.TellPatrolStatus(IEnumerable<Vector2D> enumerable, IEnumerable<Vector2D> destroyed_tiles, IEnumerable<Vector2D> poisoned_tiles)
        {
            throw new NotImplementedException();
        }
    }
}
