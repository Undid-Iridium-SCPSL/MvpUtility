// -----------------------------------------------------------------------
// <copyright file="KillsPerType.cs" company="Undid-Iridium">
// Copyright (c) Undid-Iridium. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Features.Roles;
using PlayerRoles;

namespace MvpUtility.EventHandling
{
    using System;
    using System.Collections.Generic;
    using Exiled.API.Features;

    /// <summary>
    /// Internal class that handles the Killer -> Targets (Those killed by killer stats).
    /// </summary>
    internal class KillsPerType
    {
        private Dictionary<RoleTypeId, int> targetTypedKilled;

        /// <summary>
        /// Initializes a new instance of the <see cref="KillsPerType"/> class.
        /// </summary>
        public KillsPerType()
        {
            TotalKilled = 0;
            targetTypedKilled = new Dictionary<RoleTypeId, int>();
        }

        /// <summary>
        /// Gets or sets highest kill for the role.
        /// </summary>
        public Tuple<RoleTypeId, int> HighestKillRoleCount { get; set; }

        /// <summary>
        /// Gets or sets lowest kill for role.
        /// </summary>
        public Tuple<RoleTypeId, int> LowestKillRoleCount { get; set; }

        /// <summary>
        /// Gets or sets total players killed by current player.
        /// </summary>
        public int TotalKilled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether to recalculate lowest, and highest kill count.
        /// </summary>
        private bool AlreadyCalculated { get; set; } = false;

        /// <summary>
        /// Given the target what stats needs to be incremented or created.
        /// </summary>
        /// <param name="target"> Current player killed. </param>
        public void ParseTargetType(Player target)
        {
            if (target == null)
            {
                return;
            }

            if (!targetTypedKilled.TryAddKey(target.Role, 1))
            {
                targetTypedKilled[target.Role] = targetTypedKilled[target.Role] + 1;
            }

            TotalKilled++;
        }

        /// <summary>
        /// Calculates the total kills per role by access internal dictionary.
        /// </summary>
        /// <param name="role"> Current role to check against. </param>
        /// <returns> Counter of total kills for role. </returns>
        public int TotalKillsPerRole(RoleTypeId role)
        {
            targetTypedKilled.TryGetValue(role, out int value);
            return value;
        }

        /// <summary>
        /// Since this is based on a Killer -> Target system, iterate the entire dictionary for all the values for killer to calculate
        /// total kills for Killer (Role being Scp.. or MTF.. or Chaos).
        /// </summary>
        /// <returns> <see cref="int"/> total kills for the current role. </returns>
        public int TotalKillsForMyRole()
        {
            int value = 0;

            foreach (KeyValuePair<RoleTypeId, int> pairedData in targetTypedKilled)
            {
                value += pairedData.Value;
            }

            return value;
        }

        /// <summary>
        /// Calculates the highest kills in a role, also calculates the lowest but only does it once. If constant updates
        /// are wanted then override boolean is needed.
        /// </summary>
        /// <returns> Killer role and kill count of best killer in role. </returns>
        public Tuple<RoleTypeId, int> CalculateHighestKillsInRole()
        {
            KillsPerRoleCalculator();
            return HighestKillRoleCount;
        }

        /// <summary>
        /// Calculates the lowest kills in a role, also calculates the highest but only does it once. If constant updates
        /// are wanted then override boolean is needed.
        /// </summary>
        /// <returns> Lowest killer with kill count. </returns>
        public Tuple<RoleTypeId, int> CalculateLowestKillsInAllRoles()
        {
            KillsPerRoleCalculator();
            return LowestKillRoleCount;
        }

        /// <summary>
        /// Does calculation for both highest and lowest kills but does not return anything.
        /// </summary>
        public void SetHighestKillsInAllRoles()
        {
            KillsPerRoleCalculator();
        }

        /// <summary>
        /// Calculates kills for current Killer.
        /// </summary>
        /// <param name="recalculate"> optional param to recalculate if calculations were done. </param>
        public void KillsPerRoleCalculator(bool recalculate = false)
        {
            // Tuple<RoleTypeId, int> highestRoleKill = new Tuple<RoleTypeId, int>(RoleTypeId.None, 0);
            // No benefit to not calculate both when iterating.

            // Assumes that calculation was just done, why repeat loop. Otherwise, use recalculate variable.
            if (AlreadyCalculated && !recalculate)
            {
                return;
            }

            RoleTypeId highestKillRole = RoleTypeId.None;
            int highestKillCount = int.MinValue;

            RoleTypeId lowestKillRole = RoleTypeId.None;
            int lowestKillCount = int.MaxValue;

            foreach (KeyValuePair<RoleTypeId, int> pair in targetTypedKilled)
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
            LowestKillRoleCount = Tuple.Create(lowestKillRole, lowestKillCount);

            AlreadyCalculated = true;
        }
    }
}