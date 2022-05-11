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
    /// Internal class that handles the Killer -> Targets (Those killed by killer stats).
    /// </summary>
    internal class KillsPerType
    {
        private Dictionary<RoleType, int> targetTypedKilled;

        /// <summary>
        /// 
        /// </summary>
        public Tuple<RoleType, int> HighestKillRoleCount { get; set; }
        public Tuple<RoleType, int> lowestKillRoleCount { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KillsPerType"/> class.
        /// </summary>
        public KillsPerType()
        {
            totalKilled = 0;
            targetTypedKilled = new Dictionary<RoleType, int>();
        }

        public int totalKilled { get; set; }
        private bool alreadyCalculated { get; set; } = false;
     

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
        /// Calculates the total kills per role by access internal dictionary.
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
        /// total kills for Killer (Role being Scp.. or MTF.. or Chaos).
        /// </summary>
        /// <returns> <see cref="int"/> total kills for the current role. </returns>
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
            return HighestKillRoleCount;
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
        /// Calculates kills for current Killer.
        /// </summary>
        /// <param name="recalculate"> optional param to recalculate if calculations were done. </param>
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

            HighestKillRoleCount = Tuple.Create(highestKillRole, highestKillCount);
            lowestKillRoleCount = Tuple.Create(lowestKillRole, lowestKillCount);

            alreadyCalculated = true;
        }
    }
}