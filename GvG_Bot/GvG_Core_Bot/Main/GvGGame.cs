using Discord;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Discord.Commands;
using System.Threading;
using GvG_Core_Bot.Main.Roles;
using GvG_Core_Bot.Main.Roles.OccultClub;
using GvG_Core_Bot.Main.Roles.Gaia;
using GvG_Core_Bot.Main.Roles.Guardian;
using GvG_Core_Bot.Main.Roles.Civilian;

namespace GvG_Core_Bot.Main
{
    public class GvGGame
    {
        int Day = 0;
        TimeSpan Phase_Interval = new TimeSpan(0, 0, 30);

        // tdl yet another resx for data
        readonly Dictionary<GameStatus, string> StatusNames = new Dictionary<GameStatus, string>()
        {
            [GameStatus.ActionPhase] = "Action Phase",
            [GameStatus.IdlePhase] = "Idle Phase"
        };
        string link_rewrite_blue = "http://orig05.deviantart.net/af4d/f/2014/107/a/f/rewrite___magic_circle_1_by_darksaturn93-d7etzpu.png";

        #region Discord Data
        IGuild GameHost { get; set; }
        IUser Hoster { get; set; }
        ITextChannel Public_GvG { get; set; }
        ITextChannel Guardian_Channel { get; set; }
        ITextChannel Gaia_Channel { get; set; }
        ITextChannel OC_Channel { get; set; }

        public IRole Guardian { get; private set; }
        public IRole Gaia { get; private set; }
        public IRole Occult_Club { get; private set; }
        public IRole GvG_Player { get; private set; }
        public IRole GvG_Dead_Player { get; private set; }
        #endregion

        List<IGuildUser> GaiaMembers { get; set; } = new List<IGuildUser>();
        List<IGuildUser> OCMembers { get; set; } = new List<IGuildUser>();
        List<IGuildUser> GuardianMembers { get; set; } = new List<IGuildUser>();
        List<IGuildUser> Civilians { get; set; } = new List<IGuildUser>();
        List<IGuildUser> DeadPeople { get; set; } = new List<IGuildUser>();

        Dictionary<IGuildUser, IGameRole> RoleAssignments { get; set; } = new Dictionary<IGuildUser, IGameRole>();

        public GameStatus Status { get; private set; }
        private Timer PhaseTimer { get; set; }
        private Random Randomizer { get; set; } = new Random();

        private IDependencyMap _map;

        public GvGGame(IDependencyMap map, IGuild server)
        {
            GameHost = server;
            _map = map;
        }

        public async Task CreateGame (IUser Hoster, IGuild Host, ITextChannel public_GvG, ITextChannel guardian_Channel, ITextChannel gaia_Channel, ITextChannel oC_Channel)
        {
            Day = 0;

            this.Hoster = Hoster;

            Guardian = Host.Roles.First((x) => x.Name == "guardian");
            Gaia = Host.Roles.First((x) => x.Name == "gaia");
            Occult_Club = Host.Roles.First((x) => x.Name == "occult club");
            GvG_Player = Host.Roles.First((x) => x.Name == "gvg player");
            GvG_Dead_Player = Host.Roles.First((x) => x.Name == "gvg dead player");

            GameHost = Host;
            Public_GvG = public_GvG;
            Gaia_Channel = gaia_Channel;
            Guardian_Channel = guardian_Channel;
            OC_Channel = oC_Channel;

            GaiaMembers.Clear();
            GuardianMembers.Clear();
            OCMembers.Clear();
            Civilians.Clear();
            DeadPeople.Clear();

            RoleAssignments.Clear();

            Status = GameStatus.GamePreparation;

            await (await Host.GetTextChannelsAsync()).First((x) => x.Name == "public_gvg")
                .SendMessageAsync($"{Hoster.Mention} is starting a game of GvG! If anyone wants to join, type %join_game");
        }

        public async Task<JoinResponse> Join (IGuildUser user)
        {
            if (GameHost == null || Status != GameStatus.GamePreparation) return JoinResponse.NoGameYet;
            var all_his_roles = user.RoleIds;
            if (all_his_roles.Contains(Guardian.Id))
            {
                if (GuardianMembers.Contains(user))
                {
                    return JoinResponse.AlreadyJoined;
                }
                else
                {
                    GuardianMembers.Add(user);
                    await user.AddRoleAsync(GvG_Player);
                    return JoinResponse.Success;
                }
            } else if (all_his_roles.Contains(Gaia.Id))
            {
                if (GaiaMembers.Contains(user))
                {
                    return JoinResponse.AlreadyJoined;
                } else
                {
                    GaiaMembers.Add(user);
                    await user.AddRoleAsync(GvG_Player);
                    return JoinResponse.Success;
                }
            } else if (all_his_roles.Contains(Occult_Club.Id))
            {
                if (OCMembers.Contains(user))
                {
                    return JoinResponse.AlreadyJoined;
                } else
                {
                    OCMembers.Add(user);
                    await user.AddRoleAsync(GvG_Player);
                    return JoinResponse.Success;
                }
            } else
            {
                if (Civilians.Contains(user))
                {
                    return JoinResponse.AlreadyJoined;
                } else
                {
                    Civilians.Add(user);
                    await user.AddRoleAsync(GvG_Player);
                    return JoinResponse.Success;
                }
            }
        }

        public async Task CancelGame(IUser user, bool forceClose = false)
        {
            if (user.Id == Hoster.Id || forceClose)
            {
                Status = GameStatus.Cancelled;
                foreach (var mem in GaiaMembers.Concat(GuardianMembers).Concat(OCMembers).Concat(Civilians))
                {
                    try
                    {
                        await mem.RemoveRolesAsync(new IRole[] { GvG_Player, GvG_Dead_Player });
                    } catch (Exception)
                    {
                        // do nothing
                    }
                }
                GaiaMembers.Clear();
                GuardianMembers.Clear();
                OCMembers.Clear();
                Civilians.Clear();
                PhaseTimer.Change(Timeout.Infinite, Timeout.Infinite);
                await (await GameHost.GetTextChannelsAsync()).First((x) => x.Name == "public_gvg").SendMessageAsync("The game has been cancelled by the host.");
            }
        }
        public async Task StartGame(IUser invoker)
        {
            if (invoker.Id != Hoster.Id) return;
            // placeholder for Storymaker.
            var start_embed = new EmbedBuilder().WithTitle("Game Start!");

            await AssignRoles();

            Day++;
            Status = GameStatus.IdlePhase;
            if (PhaseTimer != null) PhaseTimer?.Change(Phase_Interval, Phase_Interval);
            else
            {
                PhaseTimer = new Timer(_ => Move_Phase(), null, Phase_Interval, Phase_Interval);
                PhaseTimer.Change(Phase_Interval, Phase_Interval);
            }

            await Public_GvG.SendMessageAsync("",false,start_embed);
            await Guardian_Channel.SendMessageAsync("", false, start_embed);
            await Gaia_Channel.SendMessageAsync("", false, start_embed);
            await OC_Channel.SendMessageAsync("", false, start_embed);
        }

        #region Assignment
        private async Task AssignRoles ()
        {
            IEnumerable<IGameRole>[] roles = new IEnumerable<IGameRole>[] {
                GenerateOCRoles(OCMembers.Count),
                GenerateGaiaRoles(GaiaMembers.Count),
                GenerateGuardianRoles(GuardianMembers.Count),
                await GenerateCivilianRoles(Civilians.Count)
            };
            var members_ordered = new List<IGuildUser>[]
            {
                OCMembers,
                GaiaMembers,
                GuardianMembers,
                Civilians
            };
            for (int i = 0; i < roles.Length; i++) AddRoleListInOrder(members_ordered[i], roles[i]);
        }
        private void AddRoleListInOrder(List<IGuildUser> members, IEnumerable<IGameRole> roles)
        {
            var array_roles = roles.ToArray();
            for (int i = 0; i < members.Count; i++)
            {
                RoleAssignments.Add(members[i], array_roles[i]);
            }
        }
        

        public IEnumerable<IGameRole> GenerateGaiaRoles(int count)
        {
            IEnumerable<IGameRole> result = new IGameRole[] { new Akane(), new ChihayaAndSakuya(), new Kashima(), new Shimako() };
            if (count > 4) for (int i = 0; i < count - 4; i++) result = result.Append(new Summoner());
            // randomly the array can change and you can instead have Chihaya and Sakuya separate.
            return result.OrderBy((x) => Randomizer.Next());
        }
        public IEnumerable<IGameRole> GenerateOCRoles(int count)
        {
            IEnumerable<IGameRole> result = new IGameRole[] { new Kotori(), new Kotarou(), new GilAndPana(), new GilAndPana() };
            if (count > 4) for (int i = 0; i < count - 4; i++) result = result.Append(new GilAndPana());
            // randomly, the array can change and you can instead have gil and pana separate.
            return result.OrderBy((x) => Randomizer.Next());
        }
        public IEnumerable<IGameRole> GenerateGuardianRoles(int count)
        {
            IEnumerable<IGameRole> result = new IGameRole[] { new Touka(), new Shizuru(), new Lucia(), new Imamiya() };
            if (count > 4) for (int i = 0; i < count - 4; i++) result = result.Append(new GilAndPana());
            // randomly, the array can change and you can instead have gil and pana separate.
            return result.OrderBy((x) => Randomizer.Next());
        }
        public async Task<IEnumerable<IGameRole>> GenerateCivilianRoles(int count)
        {
            var everyone = GaiaMembers.Concat(GuardianMembers).Concat(OCMembers).Concat(Civilians);
            var all_ids = new ulong[] { Occult_Club.Id, Guardian.Id, Gaia.Id };
            if (count < 4)
            {
                var random_players = (await GameHost.GetUsersAsync())
                    .Where((x) => !x.IsBot &&
                                  !everyone.Contains(x) &&
                                  !x.RoleIds.Any((y) => all_ids.Contains(y))
                                  );
                var random_numbers = new int[4 - count];
                var total_non_playing = random_players.Count();
                for (int i = 0; i < 4 - count; i++) random_numbers[i] = -1;
                for (int i = 0; i < 4 - count; i++)
                {
                    do
                    {
                        random_numbers[i] = Randomizer.Next(total_non_playing);
                    } while (random_numbers.Count(x => x == random_numbers[i]) > 1);
                    await Join(random_players.ElementAt(random_numbers[i]));
                }
            }

            IEnumerable<IGameRole> result = new IGameRole[] { new Kagari(), new NormalCivilian(), new NormalCivilian(), new NormalCivilian() };
            if (count > 4) for (int i = 0; i < count - 4; i++) result = result.Append(new NormalCivilian());
            return result.OrderBy((x) => Randomizer.Next());
        }
        #endregion

        public void  Move_Phase ()
        {
            Status = (Status == GameStatus.IdlePhase) ? GameStatus.ActionPhase : GameStatus.IdlePhase;
            if (Status == GameStatus.IdlePhase) Day++;

            // placeholder for Storymaker
            var start_embed = new EmbedBuilder().WithTitle($"{StatusNames[Status]} [Day {Day}]").WithThumbnailUrl(link_rewrite_blue);

            Public_GvG.SendMessageAsync("", false, start_embed);
        }

        internal EmbedBuilder GenerateEmbedStatus(bool showExactGameStatus)
        {
            var embed = new EmbedBuilder().WithTitle($"Status [{GameHost.Name}]")
                .WithDescription("As of " + DateTime.Now)
                .WithThumbnailUrl("http://orig05.deviantart.net/af4d/f/2014/107/a/f/rewrite___magic_circle_1_by_darksaturn93-d7etzpu.png")
                .AddField(new EmbedFieldBuilder()
                    .WithName("Status Code")
                    .WithValue(Status)
                    );
            if (Status == GameStatus.GamePreparation)
            {
                embed = embed
                    .WithColor(new Color(0, 0, 1f))
                    .AddField(new EmbedFieldBuilder()
                    .WithName("# of Players who Joined")
                    .WithValue(GaiaMembers.Count + GuardianMembers.Count + OCMembers.Count + Civilians.Count));
            } else if (Status == GameStatus.ActionPhase || Status == GameStatus.IdlePhase)
            {
                var every_player = GaiaMembers.Concat(GuardianMembers).Concat(OCMembers).Concat(Civilians);
                embed = (Status == GameStatus.IdlePhase) ? embed.WithColor(new Color(1f, 1f, 0f)) : embed.WithColor(new Color(0.125f, 0.125f, 1f));
                embed = embed.AddField(new EmbedFieldBuilder()
                                .WithName("Day")
                                .WithValue(Day))
                             .AddField(new EmbedFieldBuilder()
                                .WithName("Alive Players")
                                .WithValue(every_player
                                    .Count((x) => x.RoleIds.Contains(GvG_Player.Id))
                                + " [" + every_player
                                    .Where((x) => x.RoleIds.Contains(GvG_Player.Id))
                                    .Select((x) => x.Nickname)
                                    .Aggregate((x, y) => x + ", " + y) + "]"
                                )
                            ).AddField(new EmbedFieldBuilder()
                                .WithName("Dead Players")
                                .WithValue(every_player
                                    .Count((x) => x.RoleIds.Contains(GvG_Dead_Player.Id))
                                + " [" + every_player
                                    .Where((x) => x.RoleIds.Contains(GvG_Player.Id))
                                    .Select((x) => x.Nickname)
                                    .Aggregate((x, y) => x + ", " + y) + "]"
                                )
                            );
            }
            return embed;
        }

        public EmbedBuilder FindRole (IGuildUser user)
        {

        }

    }

    public enum JoinResponse
    {
        Success,
        Fail,
        AlreadyJoined,
        NoGameYet
    }
    public enum GameStatus
    {
        Cancelled,
        GamePreparation,
        IdlePhase,
        ActionPhase,
        GaiaEnding,
        GuardianEnding,
        FondMemoriesEnding,
        TimeoutEnding
    }
}