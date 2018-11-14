using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using GvG_Core_Bot.Main.Positioning;
using System.Linq;

namespace GvG_Core_Bot.Main.Roles.OccultClub
{
    class Kotori : IGameRole
    {
		public List<IGameRole> WhoKnowsYou { get; set; } = new List<IGameRole>();
        public IUser RolePlayer { get ; set ; }
        public GameRoleStatus Status { get ; set ; }
        public IEnumerable<Vector2D> Positions { get ; set ; }
        public Map GameMap { get ; set ; }
        public CommandPriority CurrentPriority { get ; set ; }

		public int PhasePriority => 10;
		public Faction Faction => Faction.OC;
		public int HP { get; private set; } = 05;

		public event RoleEvent Died;
        public event RoleEvent Revealed;

		public int FamiliarCount = 0;

		private ActionQueue Queue;

		public Kotori (ActionQueue actionQ)
		{
			Queue = actionQ;
		}

        public async Task Patrol(Vector2D[] newPos, CommandPriority commandedPrio)
        {
			Positions = newPos;
        }

        public async Task Perform_ActionPhase()
        {
			// If there is a Position, you are to perform Kagari Protection
			if (Positions?.Count() > 0)
			{
				var actualPos = Positions.ElementAt(0);
				
				// Before anything, Kotori will establish a new Presence in that area, to set up a Barrier.
				var newPresence = new Presence()
				{
					Attack = 0,
					Protect = 10,
					Invisible = true,
					Role = this
				};
				GameMap.GameMap[actualPos.X, actualPos.Y].Add(newPresence);


				Queue += new Action(() =>
				{
					if (GameMap.PoisonedTiles.Contains(actualPos))Status = GameRoleStatus.Poisoned;
					if (GameMap.GameMap[actualPos.X, actualPos.Y].Find(x => x.Role.Faction == Faction.Guardian) != null &&
						GameMap.GameMap[actualPos.X, actualPos.Y].Find(x => x.Role.Faction == Faction.Gaia) != null)
					{
						// Kotori's Scouting ability
						var allOtherRoles = GameMap.GameMap[actualPos.X, actualPos.Y].Select(x => x.Role).Except(new IGameRole[] { this });
						foreach (var role in allOtherRoles)
						{
							role.WhoKnowsYou.Add(this);
						}
					}
					if (Status == GameRoleStatus.Poisoned) HP -= GameMap.RNG.Next(1);
					if (HP < 1) Died(new RoleContext());
				});
			}
        }

        public async Task Perform_IdlePhase()
        {
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
