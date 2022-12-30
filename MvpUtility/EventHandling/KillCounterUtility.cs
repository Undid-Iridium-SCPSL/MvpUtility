// -----------------------------------------------------------------------
// <copyright file="KillCounterUtility.cs" company="Undid-Iridium">
// Copyright (c) Undid-Iridium. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using PlayerRoles;

namespace MvpUtility.EventHandling
{
    using System;
    using System.Collections.Generic;
    using Exiled.API.Features;
    using Respawning;

    /// <summary>
    /// KillCounterUtility handles information regarding a Role to the Roles they kill.
    /// </summary>
    internal class KillCounterUtility
    {
        // Not counting Scientist (I would assume not really a focus, as easy to add as RoleTypeId.Scientist)

        /// <summary>
        /// All the possible NTF's in game by Role (Not including Scientist).
        /// </summary>
        private static List<RoleTypeId> allPossibleNtf = new() { RoleTypeId.NtfCaptain, RoleTypeId.NtfPrivate, RoleTypeId.NtfSergeant, RoleTypeId.NtfSpecialist, RoleTypeId.FacilityGuard };

        // Not counting D-Boys (I would assume not really a focus, as easy to add as RoleTypeId.DBoy)
        private static List<RoleTypeId> allPossibleChaos = new() { RoleTypeId.ChaosConscript, RoleTypeId.ChaosMarauder, RoleTypeId.ChaosRepressor, RoleTypeId.ChaosRifleman };

        private static List<RoleTypeId> allPossibleScps = new()
        {
            RoleTypeId.Scp173, RoleTypeId.Scp106, RoleTypeId.Scp049, RoleTypeId.Scp079,
            RoleTypeId.Scp096, RoleTypeId.Scp0492, RoleTypeId.Scp939
        };

        /// <summary>
        /// Gets or sets roles and their targets types.
        /// </summary>
        private Dictionary<RoleTypeId, KillsPerType> killsAsRole;

        /// <summary>
        /// Initializes a new instance of the <see cref="KillCounterUtility"/> class.
        /// Sets defaults for the utility to have access to the main plugin for config information.
        /// </summary>
        /// <param name="instance"> Plugin instance. </param>
        public KillCounterUtility(Main instance)
        {
            killsAsRole = new Dictionary<RoleTypeId, KillsPerType>();
            MvpPlugin = instance;
        }

        /// <summary>
        /// Gets or sets total kills for player regardless of role.
        /// </summary>
        public int TotalKills { get; set; }

        /// <summary>
        /// Gets or sets a new instance of the <see cref="KillCounterUtility"/> class.
        /// </summary>
        private Main MvpPlugin { get; set; }

        /// <summary>
        /// Had most kills, killed most scp's, escaped first, most kills per team Nickname : KillCounter(class).
        /// </summary>
        /// <param name="killer"> Killer <see cref="Player"/> who killed the current target (non null/possible environmental is rejected).</param>
        /// <param name="target"> Target <see cref="Player"/> who was killed. </param>
        public void ParseKillerStats(Player killer, Player target)
        {
            if (killer == null)
            {
                return;
            }

            TotalKills++;

            if (target == null)
            {
                return;
            }

            if (!killsAsRole.TryGetValue(killer.Role, out KillsPerType killPerType))
            {
                killPerType = new KillsPerType();
                Log.Debug("We are adding killer to role ");
                killsAsRole.Add(killer.Role, killPerType);
            }

            Log.Debug($"We are parsing type {killer.Role} and target was {target.Role}");
            killPerType.ParseTargetType(target);
        }

        /// <summary>
        /// For all the roles a player was, calculates which one was the best at killing.
        /// </summary>
        /// <returns> <see cref="Tuple{T1, T2}"/> of the best role, and their kill count. </returns>
        public Tuple<RoleTypeId, int> GetBestKillRole()
        {
            RoleTypeId bestRole = RoleTypeId.None;
            int bestKillsPerRoleCounter = int.MinValue;
            foreach (KeyValuePair<RoleTypeId, KillsPerType> pairedData in killsAsRole)
            {
                if (bestKillsPerRoleCounter < pairedData.Value.TotalKilled)
                {
                    bestKillsPerRoleCounter = pairedData.Value.TotalKilled;
                    bestRole = pairedData.Key;
                }
            }

            return Tuple.Create(bestRole, bestKillsPerRoleCounter);
        }

        /// <summary>
        /// Parse data based on killer team.
        /// </summary>
        /// <param name="team"> Current team to check against. </param>
        /// <returns> Tuple of <see cref="Tuple{T1, T2}"/> contains role, and kill count. </returns>
        public Tuple<RoleTypeId, int> GetBestKillsPerTeam(Team team)
        {
            List<RoleTypeId> teamToParse;
            switch (team)
            {
                case Team.SCPs:
                    teamToParse = allPossibleScps;
                    break;
                case Team.FoundationForces:
                case Team.Scientists:
                    teamToParse = allPossibleNtf;
                    break;
                case Team.ChaosInsurgency:
                case Team.ClassD:
                    teamToParse = allPossibleChaos;
                    break;
                default:
                    return Tuple.Create(RoleTypeId.None, int.MinValue);
            }

            RoleTypeId currentBestRole = RoleTypeId.None;
            int bestKillsPerTeam = int.MinValue;
            for (int pos = 0; pos < teamToParse.Count; pos++)
            {
                if (!killsAsRole.TryGetValue(teamToParse[pos], out KillsPerType killPerType))
                {
                    continue;
                }

                if (bestKillsPerTeam < killPerType.TotalKilled)
                {
                    currentBestRole = teamToParse[pos];
                    bestKillsPerTeam = killPerType.TotalKilled;
                }
            }

            return Tuple.Create(currentBestRole, bestKillsPerTeam);
        }

        /// <summary>
        /// Gets the total amount of kills as provided role.
        /// </summary>
        /// <param name="playerRole"> Requested Role. </param>
        /// <returns> <see cref="int"/> Total kills for role. </returns>
        public int GetKillsPerRole(RoleTypeId playerRole)
        {
            if (!killsAsRole.TryGetValue(playerRole, out KillsPerType killPerType))
            {
                return 0;
            }

            return killPerType.TotalKillsPerRole(playerRole);
        }

        /// <summary>
        /// For all the roles a player was, calculates which one was the worst at killing.
        /// </summary>
        /// <returns> <see cref="Tuple{T1, T2}"/> of the best role, and their kill count. </returns>
        public Tuple<RoleTypeId, int> CalculateWorstRole()
        {
            RoleTypeId worstRole = RoleTypeId.None;
            int worstKillsPerRole = int.MaxValue;
            foreach (KeyValuePair<RoleTypeId, KillsPerType> pairedData in killsAsRole)
            {
                if (worstKillsPerRole > pairedData.Value.TotalKilled)
                {
                    worstKillsPerRole = pairedData.Value.TotalKilled;
                    worstRole = pairedData.Key;
                }
            }

            return Tuple.Create(worstRole, worstKillsPerRole);
        }

        /// <summary>
        /// Iterates over every role a person was, and determines the best played one in terms of kills.
        /// </summary>
        /// <returns> <see cref="Tuple"/>. </returns>
        public Tuple<RoleTypeId, int> GetBestRole()
        {
            Tuple<RoleTypeId, int> currentBestRole = Tuple.Create(RoleTypeId.None, int.MinValue);
            foreach (KeyValuePair<RoleTypeId, KillsPerType> pairedData in killsAsRole)
            {
                Tuple<RoleTypeId, int> currentBestRoleCalc = Tuple.Create(pairedData.Key, pairedData.Value.TotalKilled);
                if (currentBestRoleCalc.Item2 > currentBestRole.Item2)
                {
                    currentBestRole = currentBestRoleCalc;
                }
            }

            return currentBestRole;
        }

        /// <summary>
        /// Iterates over every role a person was, and determines the best played one in terms of kills.
        /// </summary>
        /// <returns> <see cref="Tuple"/>. </returns>
        public Tuple<RoleTypeId, int> GetBestHumanToHuman()
        {
            Tuple<RoleTypeId, int> currentBestRole = Tuple.Create(RoleTypeId.None, int.MinValue);
            Log.Debug($"What is the human dict {killsAsRole.Count}");
            foreach (KeyValuePair<RoleTypeId, KillsPerType> pairedData in killsAsRole)
            {
                if (!IsScp(pairedData.Key))
                {
                    Tuple<RoleTypeId, int> currentBestRoleCalc = Tuple.Create(pairedData.Key, pairedData.Value.TotalKilled);
                    if (currentBestRoleCalc.Item2 > currentBestRole.Item2)
                    {
                        currentBestRole = currentBestRoleCalc;
                    }
                }
            }

            return currentBestRole;
        }

        /// <summary>
        /// Iterates over every role a person was, and determines the worst played one in terms of kills.
        /// </summary>
        /// <returns> <see cref="Tuple"/>. </returns>
        public Tuple<RoleTypeId, int> GetWorstRoleHuman()
        {
            Tuple<RoleTypeId, int> currentWorstRole = Tuple.Create(RoleTypeId.None, int.MaxValue);

            foreach (KeyValuePair<RoleTypeId, KillsPerType> pairedData in killsAsRole)
            {
                if (!IsScp(pairedData.Key))
                {
                    if (pairedData.Value.TotalKilled < currentWorstRole.Item2)
                    {
                        currentWorstRole = Tuple.Create(pairedData.Key, pairedData.Value.TotalKilled);
                    }
                }
            }

            Log.Debug($"What was our current worst role {currentWorstRole.Item1} and {currentWorstRole.Item2}");
            return currentWorstRole;
        }

        /// <summary>
        /// Returns the total amount of entity kills by this player.
        /// </summary>
        /// <returns> <see cref="Tuple"/>.</returns>
        internal Tuple<RoleTypeId, int> GetBestKiller()
        {
            return Tuple.Create(RoleTypeId.None, TotalKills);
        }

        // Whether a player is an SCP or not by role.
        private bool IsScp(RoleTypeId currentRole)
        {
            switch (currentRole)
            {
                case RoleTypeId.Scp049:
                case RoleTypeId.Scp0492:
                case RoleTypeId.Scp079:
                case RoleTypeId.Scp096:
                case RoleTypeId.Scp106:
                case RoleTypeId.Scp173:
                case RoleTypeId.Scp939:
                    return true;
                default:
                    return false;
            }
        }
    }
}