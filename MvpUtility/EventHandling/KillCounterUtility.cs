using Exiled.API.Features;
using System;
using System.Collections.Generic;

namespace MvpUtility.EventHandling
{
    internal class KillCounterUtility
    {
        private Player killer;


        //public int killsAgainstHumans { get => killsAgainstHumans; set => killsAgainstHumans = value; }

        public int totalKills { get => totalKills; set => totalKills = value; }

        private Dictionary<RoleType, killsPerType> killsAsRole;



        public KillCounterUtility(Player killer)
        {
            this.killer = killer;
            killsAsRole = new Dictionary<RoleType, killsPerType>();
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

            if (!killsAsRole.TryGetValue(killer.Role, out killsPerType killPerType))
            {
                killPerType = new killsPerType();
            }
            killPerType.parseTargetType(target);

        }

        public int getKillsPerRole(RoleType playerRole)
        {
            if (!killsAsRole.TryGetValue(killer.Role, out killsPerType killPerType))
            {
                return 0;
            }
            return killPerType.totalKillsPerRole(playerRole);
        }

        public Tuple<RoleType, int> getBestKillRole(RoleType playerRole)
        {
            if (!killsAsRole.TryGetValue(killer.Role, out killsPerType killPerType))
            {
                return null;
            }
            return killPerType.calculateHighestKillsInAllRoles();
        }

        public Tuple<RoleType, int> getWorstKillRole(RoleType playerRole)
        {
            if (!killsAsRole.TryGetValue(killer.Role, out killsPerType killPerType))
            {
                return null;
            }

            return killPerType.calculateLowestKillsInAllRoles();
        }

    }

    internal class killsPerType
    {
        private Dictionary<RoleType, int> targetTypedKilled;

        public Tuple<RoleType, int> highestKillRoleCount { get => highestKillRoleCount; set => highestKillRoleCount = value; }
        public Tuple<RoleType, int> lowestKillRoleCount { get => lowestKillRoleCount; set => lowestKillRoleCount = value; }
        public killsPerType()
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
            return targetTypedKilled[role];
        }

        public Tuple<RoleType, int> calculateHighestKillsInAllRoles()
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

        public void killsPerRoleCalculator()
        {
            // Tuple<RoleType, int> highestRoleKill = new Tuple<RoleType, int>(RoleType.None, 0);
            // No benefit to not calculate both when iterating. 

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
            highestKillRoleCount = Tuple.Create(lowestKillRole, lowestKillCount);
        }

    }
}