// -----------------------------------------------------------------------
// <copyright file="KillCounterUtility.cs" company="Undid-Iridium">
// Copyright (c) Undid-Iridium. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

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
        // Not counting Scientist (I would assume not really a focus, as easy to add as RoleType.Scientist)

        /// <summary>
        /// All the possible NTF's in game by Role (Not including Scientist).
        /// </summary>
        private static List<RoleType> allPossibleNtf = new List<RoleType>() { RoleType.NtfCaptain, RoleType.NtfPrivate, RoleType.NtfSergeant, RoleType.NtfSpecialist, RoleType.FacilityGuard };

        // Not counting D-Boys (I would assume not really a focus, as easy to add as RoleType.DBoy)
        private static List<RoleType> allPossibleChaos = new List<RoleType>() { RoleType.ChaosConscript, RoleType.ChaosMarauder, RoleType.ChaosRepressor, RoleType.ChaosRifleman };

        private static List<RoleType> allPossibleScps = new List<RoleType>()
        {
            RoleType.Scp173, RoleType.Scp106, RoleType.Scp049, RoleType.Scp079,
            RoleType.Scp096, RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989,
        };

        /// <summary>
        /// Gets or sets roles and their targets types.
        /// </summary>
        private Dictionary<RoleType, KillsPerType> killsAsRole;

        /// <summary>
        /// Initializes a new instance of the <see cref="KillCounterUtility"/> class.
        /// Sets defaults for the utility to have access to the main plugin for config information.
        /// </summary>
        /// <param name="instance"> Plugin instance. </param>
        public KillCounterUtility(Main instance)
        {
            killsAsRole = new Dictionary<RoleType, KillsPerType>();
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
                Log.Debug("We are adding killer to role ", MvpPlugin.Config.EnableDebug);
                killsAsRole.Add(killer.Role, killPerType);
            }

            Log.Debug($"We are parsing type {killer.Role} and target was {target.Role}", MvpPlugin.Config.EnableDebug);
            killPerType.parseTargetType(target);
        }

        /// <summary>
        /// For all the roles a player was, calculates which one was the best at killing.
        /// </summary>
        /// <returns> <see cref="Tuple{T1, T2}"/> of the best role, and their kill count. </returns>
        public Tuple<RoleType, int> GetBestKillRole()
        {
            RoleType bestRole = RoleType.None;
            int bestKillsPerRoleCounter = int.MinValue;
            foreach (KeyValuePair<RoleType, KillsPerType> pairedData in killsAsRole)
            {
                if (bestKillsPerRoleCounter < pairedData.Value.totalKilled)
                {
                    bestKillsPerRoleCounter = pairedData.Value.totalKilled;
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
        public Tuple<RoleType, int> GetBestKillsPerTeam(Team team)
        {
            List<RoleType> teamToParse;
            switch (team)
            {
                case Team.SCP:
                    teamToParse = allPossibleScps;
                    break;
                case Team.MTF:
                    teamToParse = allPossibleNtf;
                    break;
                case Team.CHI:
                    teamToParse = allPossibleChaos;
                    break;
                default:
                    return Tuple.Create(RoleType.None, int.MinValue);
            }

            RoleType currentBestRole = RoleType.None;
            int bestKillsPerTeam = int.MinValue;
            for (int pos = 0; pos < teamToParse.Count; pos++)
            {
                if (!killsAsRole.TryGetValue(teamToParse[pos], out KillsPerType killPerType))
                {
                    continue;
                }

                if (bestKillsPerTeam < killPerType.totalKilled)
                {
                    currentBestRole = teamToParse[pos];
                    bestKillsPerTeam = killPerType.totalKilled;
                }
            }

            return Tuple.Create(currentBestRole, bestKillsPerTeam);
        }

        /// <summary>
        /// Gets the total amount of kills as provided role.
        /// </summary>
        /// <param name="playerRole"> Requested Role. </param>
        /// <returns> <see cref="int"/> Total kills for role. </returns>
        public int GetKillsPerRole(RoleType playerRole)
        {
            if (!killsAsRole.TryGetValue(playerRole, out KillsPerType killPerType))
            {
                return 0;
            }

            return killPerType.totalKillsPerRole(playerRole);
        }

        /// <summary>
        /// For all the roles a player was, calculates which one was the worst at killing.
        /// </summary>
        /// <returns> <see cref="Tuple{T1, T2}"/> of the best role, and their kill count. </returns>
        public Tuple<RoleType, int> CalculateWorstRole()
        {
            RoleType worstRole = RoleType.None;
            int worstKillsPerRole = int.MaxValue;
            foreach (KeyValuePair<RoleType, KillsPerType> pairedData in killsAsRole)
            {
                if (worstKillsPerRole > pairedData.Value.totalKilled)
                {
                    worstKillsPerRole = pairedData.Value.totalKilled;
                    worstRole = pairedData.Key;
                }
            }

            return Tuple.Create(worstRole, worstKillsPerRole);
        }

        /// <summary>
        /// Iterates over every role a person was, and determines the best played one in terms of kills.
        /// </summary>
        /// <returns> <see cref="Tuple"/>. </returns>
        public Tuple<RoleType, int> GetBestRole()
        {
            Tuple<RoleType, int> currentBestRole = Tuple.Create(RoleType.None, int.MinValue);
            foreach (KeyValuePair<RoleType, KillsPerType> pairedData in killsAsRole)
            {
                Tuple<RoleType, int> currentBestRoleCalc = Tuple.Create(pairedData.Key, pairedData.Value.totalKilled);
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
        public Tuple<RoleType, int> GetBestHumanToHuman()
        {
            Tuple<RoleType, int> currentBestRole = Tuple.Create(RoleType.None, int.MinValue);
            Log.Debug($"What is the human dict {killsAsRole.Count}", MvpPlugin.Config.EnableDebug);
            foreach (KeyValuePair<RoleType, KillsPerType> pairedData in killsAsRole)
            {
                if (!IsScp(pairedData.Key))
                {
                    Tuple<RoleType, int> currentBestRoleCalc = Tuple.Create(pairedData.Key, pairedData.Value.totalKilled);
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
        public Tuple<RoleType, int> GetWorstRoleHuman()
        {
            Tuple<RoleType, int> currentWorstRole = Tuple.Create(RoleType.None, int.MaxValue);

            foreach (KeyValuePair<RoleType, KillsPerType> pairedData in killsAsRole)
            {
                if (!IsScp(pairedData.Key))
                {
                    if (pairedData.Value.totalKilled < currentWorstRole.Item2)
                    {
                        currentWorstRole = Tuple.Create(pairedData.Key, pairedData.Value.totalKilled);
                    }
                }
            }

            Log.Debug($"What was our current worst role {currentWorstRole.Item1} and {currentWorstRole.Item2}", MvpPlugin.Config.EnableDebug);
            return currentWorstRole;
        }

        /// <summary>
        /// Returns the total amount of entity kills by this player.
        /// </summary>
        /// <returns> <see cref="Tuple"/>.</returns>
        internal Tuple<RoleType, int> GetBestKiller()
        {
            return Tuple.Create(RoleType.None, TotalKills);
        }

        // Whether a player is an SCP or not by role.
        private bool IsScp(RoleType currentRole)
        {
            switch (currentRole)
            {
                case RoleType.Scp049:
                case RoleType.Scp0492:
                case RoleType.Scp079:
                case RoleType.Scp096:
                case RoleType.Scp106:
                case RoleType.Scp173:
                case RoleType.Scp93953:
                case RoleType.Scp93989:
                    return true;
                default:
                    return false;
            }
        }
    }
}