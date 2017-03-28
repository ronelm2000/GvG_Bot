using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using GvG_Core_Bot.Main.Positioning;

namespace GvG_Core_Bot.Main.Roles.Guardian
{
    class Touka : IGameRole
    {
        public List<IGameRole> WhoKnowsYou { get; set; }
        public IUser RolePlayer { get; set; }
        public GameRoleStatus Status { get; set; }
        public IEnumerable<Vector2D> Positions { get; set; }
        public Map GameMap { get; set; }
        public CommandPriority CurrentPriority { get; set; }

        public event RoleContext.RoleEvent Died;
        public event RoleContext.RoleEvent Revealed;

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
