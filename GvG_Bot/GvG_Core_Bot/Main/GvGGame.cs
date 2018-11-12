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
using System.Resources;
using GvG_Core_Bot.Main.Messages;
using GvG_Core_Bot.Main.Positioning;

namespace GvG_Core_Bot.Main
{
    public class GvGGame
    {
        int Day = 0;
        TimeSpan Phase_Interval = new TimeSpan(0, 5, 0);

        // tdl yet another resx for data
        ResourceManager ResMsg = Messages.ResultMessages.ResourceManager;
        ResourceManager DescMsg = Database.Models.RoleDescriptions.ResourceManager;
        readonly static Dictionary<GameStatus, string> StatusNames = new Dictionary<GameStatus, string>()
        {
            [GameStatus.ActionPhase] = "Action Phase",
            [GameStatus.IdlePhase] = "Idle Phase"
        };
 //       string link_rewrite_blue = "http://orig05.deviantart.net/af4d/f/2014/107/a/f/rewrite___magic_circle_1_by_darksaturn93-d7etzpu.png";
        readonly static Color RoleMessageColor = new Color(0xEEFFAA);

        #region Discord Data
        IGuild ServerHost { get; set; }
        public IUser Hoster { get; private set; }
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

        #region User Lists
        List<IGuildUser> GaiaMembers { get; set; } = new List<IGuildUser>();
        List<IGuildUser> OCMembers { get; set; } = new List<IGuildUser>();
        List<IGuildUser> GuardianMembers { get; set; } = new List<IGuildUser>();
        List<IGuildUser> Civilians { get; set; } = new List<IGuildUser>();
        List<IGuildUser> DeadPeople { get; set; } = new List<IGuildUser>();
        #endregion

        Dictionary<IGuildUser, IGameRole> RoleAssignments { get; set; } = new Dictionary<IGuildUser, IGameRole>();
        Dictionary<IGuildUser, FakeReportContext> FakeReports { get; set; } = new Dictionary<IGuildUser, FakeReportContext>();

		ActionQueue Queue = new ActionQueue();

        #region Time Keeping
        public GameStatus Status { get; private set; } = GameStatus.Cancelled;
        private Timer PhaseTimer { get; set; }
        private DateTime TimerStarted { get; set; }
        private DateTime TimerPaused { get; set; }
        private bool IsPaused { get; set; } = false;
        #endregion

        Map GameMap = null;

        private Random Randomizer { get; set; } = new Random();

        private GvG_GameService Parent;

        public GvGGame(GvG_GameService parent, IGuild server)
        {
            ServerHost = server;
            Parent = parent;
        }

        // stated to change.

        public async Task CreateGame (IUser Hoster, IGuild Host, ITextChannel public_GvG, ITextChannel guardian_Channel, ITextChannel gaia_Channel, ITextChannel oC_Channel)
        {
            if (Status > GameStatus.Cancelled && Status < GameStatus.GaiaEnding) throw new AlreadyOngoingGameGvGGameException();

            Day = 0;

            this.Hoster = Hoster;

            Guardian = Host.Roles.First((x) => x.Name == Parent.config.guardian_chan_name);
            Gaia = Host.Roles.First((x) => x.Name == Parent.config.gaia_chan_name);
            Occult_Club = Host.Roles.First((x) => x.Name == Parent.config.oc_chan_name);
            GvG_Player = Host.Roles.First((x) => x.Name == Parent.config.gvg_player_role_name);
            GvG_Dead_Player = Host.Roles.First((x) => x.Name == Parent.config.gvg_dead_player_role_name);

            ServerHost = Host;
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

            await (await Host.GetTextChannelsAsync()).First((x) => x.Name == Parent.config.pub_gvg_chan_name)
                .SendMessageAsync(null, false, new EmbedBuilder()
                    .WithTitle(ResultMessages.CreateGameSuccess)
                    .WithDescription(string.Format(ResultMessages.CreateGameSuccess_Desc, Hoster.Mention))
                    .Build());
        }

        public async Task<JoinResponse> Join (IGuildUser user)
        {
            if (ServerHost == null || Status != GameStatus.GamePreparation) return JoinResponse.NoGameYet;
            var all_his_roles = user.RoleIds;
            if (all_his_roles.Contains(Guardian.Id)) return await CheckAndJoin(GuardianMembers, user);
            else if (all_his_roles.Contains(Gaia.Id)) return await CheckAndJoin(GaiaMembers, user);
            else if (all_his_roles.Contains(Occult_Club.Id)) return await CheckAndJoin(OCMembers, user);
            else return await CheckAndJoin(Civilians, user);
        }
        private async Task<JoinResponse> CheckAndJoin(List<IGuildUser> Club, IGuildUser user)
        {
            if (Club.Contains(user))
            {
                return JoinResponse.AlreadyJoined;
            }
            else
            {
                Club.Add(user);
                await user.AddRoleAsync(GvG_Player);
                return JoinResponse.Success;
            }
        }

        public async Task CancelGame(IUser user, bool forceClose = false)
        {
            if (Status > GameStatus.Cancelled && (user.Id == Hoster?.Id || forceClose))
            {
                if (Status < GameStatus.GaiaEnding) await (await ServerHost.GetTextChannelsAsync()).First((x) => x.Name == "public_gvg").SendMessageAsync("The game has been cancelled by the host.");
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
                PhaseTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
        public async Task<EmbedBuilder> StartGame(IUser invoker)
        {
            if (invoker.Id != (Hoster?.Id ?? 0)) return new EmbedBuilder()
                    .WithTitle("StartGameError")
                    .WithDescription("ErrorCreateGameFirst");
            // placeholder for Storymaker.
            var start_embed = new EmbedBuilder().WithTitle("Game Start!");

            await AssignRoles();

            Day++;
            Status = GameStatus.IdlePhase;
            if (PhaseTimer != null) PhaseTimer?.Change(Phase_Interval, Phase_Interval);
            else
            {
                PhaseTimer = new Timer(_ => MovePhase(), null, Phase_Interval, Phase_Interval);
                PhaseTimer.Change(Phase_Interval, Phase_Interval);
                TimerStarted = DateTime.Now;
            }

            await Public_GvG.SendMessageAsync("",false,start_embed.Build());
            await Guardian_Channel.SendMessageAsync("", false, start_embed.Build());
            await Gaia_Channel.SendMessageAsync("", false, start_embed.Build());
            await OC_Channel.SendMessageAsync("", false, start_embed.Build());

            return null;
        }

        #region Assignment
        private async Task AssignRoles ()
        {
            IEnumerable<IGameRole>[] roles = new IEnumerable<IGameRole>[] {
                await GenerateOCRoles(OCMembers.Count),
                await GenerateGaiaRoles(GaiaMembers.Count),
                await GenerateGuardianRoles(GuardianMembers.Count),
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
        
        public async Task<IEnumerable<IGameRole>> GenerateGaiaRoles(int count)
        {
            if (count < 4) await ForceJoin(Gaia, GaiaMembers, 4 - count);
            IEnumerable<IGameRole> result = new IGameRole[] { new Akane(), new ChihayaAndSakuya(), new Kashima(), new Shimako() };
            if (count > 4) for (int i = 0; i < count - 4; i++) result = result.Append(new Summoner());
            // randomly the array can change and you can instead have Chihaya and Sakuya separate.
            return result.OrderBy((x) => Randomizer.Next());
        }
        public async Task<IEnumerable<IGameRole>> GenerateOCRoles(int count)
        {
            if (count < 4) await ForceJoin(Occult_Club, OCMembers, 4 - count);
            IEnumerable<IGameRole> result = new IGameRole[] { new Kotori(Queue), new Kotarou(), new GilAndPana(), new GilAndPana() };
            if (count > 4) for (int i = 0; i < count - 4; i++) result = result.Append(new GilAndPana());
            // randomly, the array can change and you can instead have gil and pana separate.
            return result.OrderBy((x) => Randomizer.Next());
        }
        public async Task<IEnumerable<IGameRole>> GenerateGuardianRoles(int count)
        {
            if (count < 4) await ForceJoin(Guardian, GuardianMembers, 4 - count);
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
                var random_players = (await ServerHost.GetUsersAsync())
                    .Where((x) => !x.IsBot &&
                                  !everyone.Contains(x) &&
                                  !x.RoleIds.Any((y) => all_ids.Contains(y))
                                  );
                var random_numbers = new int[4 - count];
                var total_non_playing = random_players.Count();
                var resultMsg = String.Empty;
                for (int i = 0; i < 4 - count; i++)
                {
                    do
                    {
                        random_numbers[i] = Randomizer.Next(total_non_playing);
                    } while (random_numbers.Take(i).Contains(random_numbers[i]));
                    var memToAdd = random_players.ElementAt(random_numbers[i]);
                    Civilians.Add(memToAdd);
                    await memToAdd.AddRoleAsync(GvG_Player);
                    resultMsg += $"Because there are too few players, {memToAdd.Mention} is forced to join the game as a Civilian" + ((i == count - 1) ? "" : Environment.NewLine);
                    
                }
                Public_GvG.SendMessageAsync(resultMsg);
            }
            IEnumerable<IGameRole> result = new IGameRole[] { new Kagari(), new NormalCivilian(), new NormalCivilian(), new NormalCivilian() };
            if (count > 4) for (int i = 0; i < count - 4; i++) result = result.Append(new NormalCivilian());
            return result.OrderBy((x) => Randomizer.Next());
        }
        internal async Task ForceJoin (IRole role, List<IGuildUser> roleMembers, int members)
        {
            var possibleMembers = (await ServerHost.GetUsersAsync()).Where((x) => x.RoleIds.Contains(role.Id) && !roleMembers.Contains(x));
            int[] randomMembers = new int[members];
            string resultMsg = string.Empty;
            for (int i = 0; i < members; i++)
            {
                do
                {
                    randomMembers[i] = Randomizer.Next(possibleMembers.Count());
                } while (randomMembers.Take(i).Contains(randomMembers[i]));
                var memToAdd = possibleMembers.ElementAt(randomMembers[i]);
                roleMembers.Add(memToAdd);
                resultMsg += $"Because there are too few players, {memToAdd.Mention} is forced to join the game as {role?.Mention}" + ((i == members - 1) ? "" : Environment.NewLine);
                await memToAdd.AddRoleAsync(GvG_Player);
                // force join member goes here.
            }

            await Public_GvG.SendMessageAsync(resultMsg);

        }
        #endregion

        #region Time Management
        public void MovePhase ()
        {
            Status = (Status == GameStatus.IdlePhase) ? GameStatus.ActionPhase : GameStatus.IdlePhase;
			if (Status == GameStatus.IdlePhase) Day++;

			// arrange each role by priority
			var roles = RoleAssignments.Select(x => x.Value).OrderByDescending(x => x.PhasePriority);
			foreach (var role in roles)
			{
				if (Status == GameStatus.IdlePhase) role.Perform_ActionPhase();
				else if (Status == GameStatus.ActionPhase) role.Perform_IdlePhase();
			}

            // placeholder for Storymaker
            var start_embed = new EmbedBuilder().WithTitle($"{StatusNames[Status]} [Day {Day}]").WithThumbnailUrl(Links.LinkRewriteBlue1);

            Public_GvG.SendMessageAsync("", false, start_embed.Build());
            TimerStarted = DateTime.Now;
        }
        public EmbedBuilder Pause (SocketCommandContext context)
        {
           var start_embed = new EmbedBuilder();
           if (context.User == Hoster)
            {
                if (IsPaused) return start_embed
                        .WithTitle(ResultMessages.PauseError)
                        .WithDescription(ResultMessages.PauseError_AlreadyPaused);
                PhaseTimer.Change(Timeout.Infinite, Timeout.Infinite);
                TimerPaused = DateTime.Now;
                IsPaused = true;
                return start_embed.WithTitle(ResultMessages.PauseSuccess)
                    .WithDescription(ResultMessages.PauseSuccess_Desc);
            } else
            {
                return start_embed
                    .WithTitle(ResultMessages.PauseError)
                    .WithDescription(string.Format(ResultMessages.PauseError_NotAuthorized, Hoster.Mention));
            }
        }
        public EmbedBuilder Continue (SocketCommandContext context)
        {
            var start_embed = new EmbedBuilder();
            if (context.User == Hoster)
            {
                if (!IsPaused) return start_embed
                        .WithTitle(ResultMessages.ContError)
                        .WithDescription(ResultMessages.ContError_NotPaused);
                PhaseTimer.Change((TimerStarted + Phase_Interval - TimerPaused), Phase_Interval);
                IsPaused = false;
                return start_embed
                    .WithTitle(ResultMessages.ContSuccess)
                    .WithDescription(ResultMessages.ContSuccess_Desc);
            }
            else
            {
                return start_embed
                    .WithTitle(ResultMessages.ContError)
                    .WithDescription(ResultMessages.ContError_NotAuthorized);
            }
        }
        #endregion

        #region Information Management
        internal EmbedBuilder GetFullStatus(bool showExactGameStatus)
        {
            var embed = new EmbedBuilder().WithTitle($"Status [{ServerHost.Name}]")
                .WithDescription("As of " + DateTime.Now)
                .WithThumbnailUrl(Links.LinkRewriteBlue1)
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
                var countDead = every_player.Count((x) => x.RoleIds.Contains(GvG_Dead_Player.Id));
                var listDead = (countDead > 0) ? (" [" + every_player
                                    .Where((x) => x.RoleIds.Contains(GvG_Dead_Player.Id))
                                    .Select((x) => (string.IsNullOrEmpty(x.Nickname)) ? x.Username : x.Nickname)
                                    .Aggregate((x, y) => x + ", " + y) + "]") : "";
                embed = embed.AddField(new EmbedFieldBuilder()
                                .WithName("Day")
                                .WithValue(Day))
                             .AddField(new EmbedFieldBuilder()
                                .WithName("Alive Players")
                                .WithValue(every_player
                                    .Count((x) => x.RoleIds.Contains(GvG_Player.Id))
                                + " [" + every_player
                                    .Where((x) => x.RoleIds.Contains(GvG_Player.Id))
                                    .Select((x) => (string.IsNullOrEmpty(x.Nickname)) ? x.Username : x.Nickname)
                                    .Aggregate((x, y) => x + ", " + y) + "]"
                                )
                            ).AddField(new EmbedFieldBuilder()
                                .WithName("Dead Players")
                                .WithValue(countDead + listDead
                                )
                            );
            }
            return embed;
        }
        public EmbedBuilder FindRole(IGuildUser user, bool IsPublic)
        {
            var FakeReport = (FakeReports.ContainsKey(user)) ? FakeReports[user] : FakeReportContext.Empty;
            var start_embed = new EmbedBuilder().WithColor(RoleMessageColor);
            if (FakeReport == FakeReportContext.Empty || !IsPublic) start_embed = start_embed.WithTitle(RoleAssignments[user].GetType().ToString());//.WithTitle(ResMsg.GetString(RoleAssignments[user] + ""));

            return start_embed;
        }

        public bool IsAPlayer (IUser user)
        {
            return RoleAssignments.Count(x => x.Key.Id == user.Id) > 0;
        }
        public EmbedBuilder FindAllRoles()
        {
            var start_embed = new EmbedBuilder().WithTitle("All Roles");
            foreach (var role in RoleAssignments)
            {
                start_embed.AddField(new EmbedFieldBuilder().WithName(role.Key.Username).WithValue(role.Value.GetType().ToString()));
            }
            return start_embed;
        }
        public async Task<EmbedBuilder> GetMapStatus(IGuildUser user, IMessageChannel contextChannel, bool IsPrivate)
        {
            var msg = ":zero::one::two::three::four::five::six::seven::eight:" + Environment.NewLine;
            var flag = RoleContextType.Public;
            IGameRole role = null;
			var resultEmbed = new EmbedBuilder();
            if (IsAPlayer(user))
            {
                if (contextChannel.Id == (await user.GetOrCreateDMChannelAsync()).Id) flag = RoleContextType.Private;
                if (IsOnClubChannel(contextChannel)) flag = RoleContextType.Channel;
            }
            for (int y = 0; y < GameMap.MaxY; y++) {
                for (int x = 0; x < GameMap.MaxX; x++)
                {
					
                }

            }
			return resultEmbed;
        }
        #endregion

        #region Movement and Actions
        public async Task<EmbedBuilder> PatrolQueue (IGuildUser user, SocketCommandContext Context, IEnumerable<Vector2D> orderedPatrol)
        {
            var start_embed = new EmbedBuilder();
            // we run specific Game related checks before passing to the Role.
            if (!RoleAssignments.ContainsKey(user)) return start_embed
                    .WithTitle(ResultMessages.PatrolError)
                    .WithDescription(Links.LinkRewriteBlue1);
            var role = RoleAssignments[user];
            var prio = (Context.IsPrivate) ? CommandPriority.DM :
                        (Context.Guild == null) ? CommandPriority.Public :
                        (IsOnClubChannel(Context.Channel)) ? CommandPriority.Channel :
                        CommandPriority.Public;
            await RoleAssignments[user].Patrol(orderedPatrol.ToArray(), prio);

            return start_embed;
        }
        public bool IsOnClubChannel (IMessageChannel channel)
        {
            return channel.Id == Guardian_Channel.Id ||
                channel.Id == Gaia_Channel.Id ||
                channel.Id == OC_Channel.Id;
        }
        #endregion

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