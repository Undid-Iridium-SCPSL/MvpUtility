namespace MvpUtility.EventHandling
{
    using Exiled.API.Features;
    using Respawning;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// KillCounterUtility handles information regarding a Role to the Roles they kill.
    /// </summary>
    internal class KillCounterUtility
    {
        // Not counting Scientist (I would assume not really a focus, as easy to add as RoleType.Scientist)

        /// <summary>
        /// All the possible NTF's in game by Role (Not including Scientist)
        /// </summary>
        private static List<RoleType> allPossibleNtf = new List<RoleType>() { RoleType.NtfCaptain, RoleType.NtfPrivate, RoleType.NtfSergeant, RoleType.NtfSpecialist, RoleType.FacilityGuard };

        // Not counting D-Boys (I would assume not really a focus, as easy to add as RoleType.DBoy)
        private static List<RoleType> allPossibleChaos = new List<RoleType>() { RoleType.ChaosConscript, RoleType.ChaosMarauder, RoleType.ChaosRepressor, RoleType.ChaosRifleman };

        private static List<RoleType> allPossibleScps = new List<RoleType>() { RoleType.Scp173, RoleType.Scp106, RoleType.Scp049, RoleType.Scp079
            , RoleType.Scp096, RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989, };

        /// <summary>
        /// Gets or sets roles and their targets types.
        /// </summary>
        private Dictionary<RoleType, KillsPerType> killsAsRole;

        /// <summary>
        /// Gets or sets total kills for player regardless of role.
        /// </summary>
        public int TotalKills { get; set; }

        /// <summary>
        /// Gets or sets a new instance of the <see cref="KillCounterUtility"/> class.
        /// </summary>
        private Main MvpPlugin { get; set; }

        public KillCounterUtility(Main instance)
        {
            killsAsRole = new Dictionary<RoleType, KillsPerType>();
            MvpPlugin = instance;
        }

        //Had most kills, killed most scp's, escaped first, most kills per team Nickname : KillCounter(class)

        public void parseKillerStats(Player killer, Player target)
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

        public Tuple<RoleType, int> getKillsPerTeam(Team team)
        {
            return getBestKillsPerTeam(team);
        }

        public Tuple<RoleType, int> getBestKillsPerTeam(Team team)
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
        /// Returns the total amount of entity kills by this player. 
        /// </summary>
        /// <returns> <see cref="Tuple"/></returns>
        internal Tuple<RoleType, int> getBestKiller()
        {
            return Tuple.Create(RoleType.None, TotalKills);
        }

        /// <summary>
        /// Gets the total amount of kills as provided role 
        /// </summary>
        /// <param name="playerRole"> Requested Role </param>
        /// <returns> <see cref="int"/> Total kills for role </returns>
        public int getKillsPerRole(RoleType playerRole)
        {
            if (!killsAsRole.TryGetValue(playerRole, out KillsPerType killPerType))
            {
                return 0;
            }
            return killPerType.totalKillsPerRole(playerRole);
        }


        /// <summary>
        /// For all the roles a player was, calculates which one was the best at killing.
        /// </summary>
        /// <param name="playerRole"></param>
        /// <returns></returns>
        public Tuple<RoleType, int> getBestKillRole()
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
        /// For all the roles a player was, calculates which one was the worst at killing.
        /// </summary>
        /// <param name="playerRole"></param>
        /// <returns></returns>
        public Tuple<RoleType, int> calculateWorstRole()
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
        /// Iterates over every role a person was, and determines the best played one in terms of kills
        /// </summary>
        /// <returns> <see cref="Tuple"/> </returns>
        public Tuple<RoleType, int> getBestRole()
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
        /// Iterates over every role a person was, and determines the best played one in terms of kills
        /// </summary>
        /// <returns> <see cref="Tuple"/> </returns>
        public Tuple<RoleType, int> getBestHumanToHuman()
        {
            Tuple<RoleType, int> currentBestRole = Tuple.Create(RoleType.None, int.MinValue);
            Log.Debug($"What is the human dict {killsAsRole.Count}", MvpPlugin.Config.EnableDebug);
            foreach (KeyValuePair<RoleType, KillsPerType> pairedData in killsAsRole)
            {
                if (!isScp(pairedData.Key))
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


        // Whether a player is an SCP or not by role. 
        private bool isScp(RoleType currentRole)
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



        /// <summary>
        /// Iterates over every role a person was, and determines the worst played one in terms of kills
        /// </summary>
        /// <returns> <see cref="Tuple"/> </returns>
        public Tuple<RoleType, int> getWorstRoleHuman()
        {
            Tuple<RoleType, int> currentWorstRole = Tuple.Create(RoleType.None, int.MaxValue);

            foreach (KeyValuePair<RoleType, KillsPerType> pairedData in killsAsRole)
            {
                if (!isScp(pairedData.Key))
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

    }

    /// <summary>
    /// Internal class that handles the Killer -> Targets (Those killed by killer stats)
    /// </summary>
    internal class KillsPerType
    {
        private Dictionary<RoleType, int> targetTypedKilled;

        public Tuple<RoleType, int> highestKillRoleCount { get; set; }
        public Tuple<RoleType, int> lowestKillRoleCount { get; set; }

        public int totalKilled { get; set; }
        private bool alreadyCalculated { get; set; } = false;
        public KillsPerType()
        {
            totalKilled = 0;
            targetTypedKilled = new Dictionary<RoleType, int>();
        }

        /// <summary>
        /// Given the target what stats needs to be incremented or created. 
        /// </summary>
        /// <param name="target"></param>
        public void parseTargetType(Player target)
        {
            if (target == null)
            {
                return;
            }

            if (!targetTypedKilled.TryAddKey(target.Role, 1))
            {
                targetTypedKilled[target.Role] = targetTypedKilled[target.Role] + 1;
            }

            totalKilled++;
        }


        /// <summary>
        /// Calculates the total kills per role by access internal dictionary
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public int totalKillsPerRole(RoleType role)
        {
            targetTypedKilled.TryGetValue(role, out int value);
            return value;
        }

        /// <summary>
        /// Since this is based on a Killer -> Target system, iterate the entire dictionary for all the values for killer to calculate
        /// total kills for Killer (Role being Scp.. or MTF.. or Chaos)
        /// </summary>
        /// <returns> <see cref="int"/> total kills for the current role </returns>
        public int totalKillsForMyRole()
        {
            int value = 0;

            foreach (KeyValuePair<RoleType, int> pairedData in targetTypedKilled)
            {
                value += pairedData.Value;
            }

            return value;
        }


        /// <summary>
        /// Calculates the highest kills in a role, also calculates the lowest but only does it once. If constant updates
        /// are wanted then override boolean is needed. 
        /// </summary>
        /// <returns></returns>
        public Tuple<RoleType, int> calculateHighestKillsInRole()
        {
            killsPerRoleCalculator();
            return highestKillRoleCount;
        }

        /// <summary>
        /// Calculates the lowest kills in a role, also calculates the highest but only does it once. If constant updates
        /// are wanted then override boolean is needed. 
        /// </summary>
        /// <returns></returns>
        public Tuple<RoleType, int> calculateLowestKillsInAllRoles()
        {
            killsPerRoleCalculator();
            return lowestKillRoleCount;
        }



        /// <summary>
        /// Does calculation for both highest and lowest kills but does not return anything. 
        /// </summary>
        public void setHighestKillsInAllRoles()
        {
            killsPerRoleCalculator();
        }

        /// <summary>
        /// Calculates kills for current Killer
        /// </summary>
        /// <param name="recalculate"> optional param to recalculate if calculations were done </param>
        public void killsPerRoleCalculator(bool recalculate = false)
        {
            // Tuple<RoleType, int> highestRoleKill = new Tuple<RoleType, int>(RoleType.None, 0);
            // No benefit to not calculate both when iterating. 

            //Assumes that calculation was just done, why repeat loop. Otherwise, use recalculate variable.
            if (alreadyCalculated && !recalculate)
            {
                return;
            }
            RoleType highestKillRole = RoleType.None;
            int highestKillCount = int.MinValue;

            RoleType lowestKillRole = RoleType.None;
            int lowestKillCount = int.MaxValue;


            foreach (KeyValuePair<RoleType, int> pair in targetTypedKilled)
            {
                if (pair.Value < lowestKillCount)
                {
                    lowestKillRole = pair.Key;
                    lowestKillCount = pair.Value;
                }
                if (pair.Value > highestKillCount)
                {
                    highestKillRole = pair.Key;
                    highestKillCount = pair.Value;
                }
            }

            highestKillRoleCount = Tuple.Create(highestKillRole, highestKillCount);
            lowestKillRoleCount = Tuple.Create(lowestKillRole, lowestKillCount);


            alreadyCalculated = true;
        }

    }
}