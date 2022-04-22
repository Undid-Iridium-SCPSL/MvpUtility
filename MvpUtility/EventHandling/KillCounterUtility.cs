using Exiled.API.Features;
using Respawning;
using System;
using System.Collections.Generic;

namespace MvpUtility.EventHandling
{
    internal class KillCounterUtility
    {



        //public int killsAgainstHumans { get => killsAgainstHumans; set => killsAgainstHumans = value; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PickupChecker"/> class.
        /// </summary>
        /// <param name="plugin">An instance of the <see cref="Plugin"/> class.</param>
        Main plugin { get; set; }

        public int totalKills { get; set; }

        private Dictionary<RoleType, KillsPerType> killsAsRole;

        public static List<RoleType> allPossibleNtf = new List<RoleType>() { RoleType.NtfCaptain, RoleType.NtfPrivate, RoleType.NtfSergeant, RoleType.NtfSpecialist };

        public static List<RoleType> allPossibleChaos = new List<RoleType>() { RoleType.ChaosConscript, RoleType.ChaosMarauder, RoleType.ChaosRepressor, RoleType.ChaosRifleman };

        public static List<RoleType> allPossibleScps = new List<RoleType>() {  RoleType.Scp173, RoleType.Scp106, RoleType.Scp049, RoleType.Scp079
            , RoleType.Scp096, RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989 };

        public KillCounterUtility(Main instance)
        {

            killsAsRole = new Dictionary<RoleType, KillsPerType>();
            plugin = instance;
        }


        //Had most kills, killed most scp's, escaped first, most kills per team Nickname : KillCounter(class)


        public void parseKillerStats(Player killer, Player target)
        {
            if (killer == null)
            {
                return;
            }

            totalKills++;

            if (target == null)
            {

                return;
            }

            if (!killsAsRole.TryGetValue(killer.Role, out KillsPerType killPerType))
            {
                killPerType = new KillsPerType();
                Log.Debug("We are adding killer to role ", plugin.Config.enableDebug);
                killsAsRole.Add(killer.Role, killPerType);
            }
            Log.Debug($"We are parsing type {killer.Role}", plugin.Config.enableDebug);
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
                Log.Debug($"We are parsing team with {teamToParse[pos]} and this was output {killPerType.totalKillsForMyRole()} ", plugin.Config.enableDebug);
                if (bestKillsPerTeam < killPerType.totalKillsForMyRole())
                {
                    currentBestRole = teamToParse[pos];
                    bestKillsPerTeam = killPerType.totalKillsForMyRole();
                }
            }
            return Tuple.Create(currentBestRole, bestKillsPerTeam);

        }

        internal Tuple<RoleType, int> getBestKiller()
        {
            return Tuple.Create(RoleType.None, totalKills);
        }

        public int getKillsPerRole(RoleType playerRole)
        {
            if (!killsAsRole.TryGetValue(playerRole, out KillsPerType killPerType))
            {
                return 0;
            }
            return killPerType.totalKillsPerRole(playerRole);
        }

        public Tuple<RoleType, int> getBestKillRole(RoleType playerRole)
        {
            if (!killsAsRole.TryGetValue(playerRole, out KillsPerType killPerType))
            {
                return null;
            }
            return killPerType.calculateHighestKillsInRole();
        }

        public Tuple<RoleType, int> calculateWorstRole(RoleType playerRole)
        {
            if (!killsAsRole.TryGetValue(playerRole, out KillsPerType killPerType))
            {
                return null;
            }

            return killPerType.calculateLowestKillsInAllRoles();
        }


        /// <summary>
        /// Iterates over every role a person was, and determines the best played one in terms of kills
        /// </summary>
        /// <returns> <see cref="Tuple"/> </returns>
        public Tuple<RoleType, int> getBestRole()
        {
            Tuple<RoleType, int> currentBestRole = Tuple.Create(RoleType.None, int.MaxValue);
            foreach (KeyValuePair<RoleType, KillsPerType> pairedData in killsAsRole)
            {
                Tuple<RoleType, int> currentBestRoleCalc = pairedData.Value.calculateHighestKillsInRole();
                if (currentBestRoleCalc.Item2 < currentBestRole.Item2)
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
            Tuple<RoleType, int> currentBestRole = Tuple.Create(RoleType.None, int.MaxValue);
            Log.Debug($"What is the human dict {killsAsRole.Count}", plugin.Config.enableDebug);
            foreach (KeyValuePair<RoleType, KillsPerType> pairedData in killsAsRole)
            {
                if (!isScp(pairedData.Key))
                {
                    Tuple<RoleType, int> currentBestRoleCalc = pairedData.Value.calculateHighestKillsInRole();
                    if (currentBestRoleCalc.Item2 < currentBestRole.Item2)
                    {
                        currentBestRole = currentBestRoleCalc;
                    }
                }

            }
            return currentBestRole;
        }

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
        public Tuple<RoleType, int> getWorstRole()
        {
            Tuple<RoleType, int> currentWorstRole = Tuple.Create(RoleType.None, int.MaxValue);

            foreach (KeyValuePair<RoleType, KillsPerType> pairedData in killsAsRole)
            {
                Tuple<RoleType, int> currentWorstRoleCalc = pairedData.Value.calculateLowestKillsInAllRoles();
                if (currentWorstRoleCalc.Item2 < currentWorstRole.Item2)
                {
                    currentWorstRole = currentWorstRoleCalc;
                }
            }
            return currentWorstRole;
        }

    }

    internal class KillsPerType
    {
        private Dictionary<RoleType, int> targetTypedKilled;

        public Tuple<RoleType, int> highestKillRoleCount { get; set; }
        public Tuple<RoleType, int> lowestKillRoleCount { get; set; }

        private bool alreadyCalculated { get; set; } = false;
        public KillsPerType()
        {
            targetTypedKilled = new Dictionary<RoleType, int>();
        }

        public void parseTargetType(Player target)
        {
            if (target == null)
            {
                return;
            }

            if (!targetTypedKilled.TryAdd(target.Role, 1))
            {
                targetTypedKilled[target.Role] = targetTypedKilled[target.Role] + 1;
            }
        }

        public int totalKillsPerRole(RoleType role)
        {
            targetTypedKilled.TryGetValue(role, out int value);
            return value;
        }
        public int totalKillsForMyRole()
        {
            int value = 0;

            foreach (KeyValuePair<RoleType, int> pairedData in targetTypedKilled)
            {
                value += pairedData.Value;
            }

            return value;
        }

        public Tuple<RoleType, int> calculateHighestKillsInRole()
        {
            killsPerRoleCalculator();
            return highestKillRoleCount;
        }

        public Tuple<RoleType, int> calculateLowestKillsInAllRoles()
        {
            killsPerRoleCalculator();
            return lowestKillRoleCount;
        }

        public void setHighestKillsInAllRoles()
        {
            killsPerRoleCalculator();
        }

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