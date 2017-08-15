using System;
using System.Collections.Generic;
using System.Text;
using GvG_Core_Bot.Main.Roles;
using System.Threading.Tasks;
using System.Linq;
using Discord;

namespace GvG_Core_Bot.Main.Positioning
{
    public class Map
    {
        public List<Presence>[,] GameMap { get; private set; }
        public List<Vector2D> DestroyedTiles { get; private set; } = new List<Vector2D>();
        public List<Vector2D> PoisonedTiles { get; private set; } = new List<Vector2D>();

        public Map (int x, int y)
        {
            GameMap = new List<Presence>[x, y];
        }

        public int MaxY { get; internal set; }
        public int MaxX { get; internal set; }

        public async Task Place(IEnumerable<Presence> presence, IEnumerable<Vector2D> positions)
        {
            //var dm_chan = await presence.First().Role.RolePlayer.CreateDMChannelAsync();

            // Formats:
            // 1 Presence => 1 Position
            // 1 Presence => 5 Positions
            // 2 Presence => 2 Positions
            // 2 Presence => 3 Positions => 0->0
            // 3 Presence => 5 Positions 
            // Basically, the last Presence is for multiple Positions always

            // set to inform the user if he tried to go to the destroyed/poisoned tiles.
            //var pos_destroyed = new List<Vector2D>();
            //var pos_poisoned = new List<Vector2D>();
            for (int i = 0; i < presence.Count() || i < positions.Count(); i++)
            {
                var pos = positions.ElementAt(i);
                GameMap[pos.X, pos.Y] = GameMap[pos.X, pos.Y] ?? new List<Presence>();
                if (!((DestroyedTiles.Contains(pos)) || PoisonedTiles.Contains(pos)))
                    GameMap[pos.X, pos.Y].Add(presence.ElementAt(i));
                if (i + 1 == presence.Count())
                {
                    for (int j = i; j < positions.Count(); j++)
                    {
                        var pos2 = positions.ElementAt(j);
                        if (!((DestroyedTiles.Contains(pos)) || PoisonedTiles.Contains(pos)))
                            GameMap[pos2.X, pos2.Y].Add(presence.ElementAt(j).Duplicate());
                    }
                }
            }

            var destroyed_tiles = positions.Where(x => DestroyedTiles.Contains(x));
            var poisoned_tiles = positions.Where(x => PoisonedTiles.Contains(x));

            // Placeholders
            presence.First().Role.TellPatrolStatus(positions.Except(destroyed_tiles).Except(poisoned_tiles), destroyed_tiles, poisoned_tiles);
            //dm_chan.SendMessageAsync("", false, new EmbedBuilder().WithTitle("Patrol"));
            
        }
    }
    
}
