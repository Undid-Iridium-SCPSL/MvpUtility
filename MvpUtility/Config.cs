// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Undid-Iridium">
// Copyright (c) Undid-Iridium. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace MvpUtility
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Exiled.API.Interfaces;

    /// <summary>
    /// Config file.
    /// </summary>
    public class Config : IConfig
    {
        /// <inheritdoc />
        public bool IsEnabled { get; set; } = true;

        public bool Debug { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether control over to enable or disable debug information.
        /// </summary>
        [Description("Control over to enable or disable debug information")]
        public bool EnableDebug { get; set; } = false;

        /// <summary>
        /// Gets or sets what types of end round outputs should be shown.
        /// </summary>
        [Description("Control over what types to show")]
        public RoundEndConfigurations RoundEndBehaviors { get; set; } = new RoundEndConfigurations();

        /// <summary>
        /// Gets or sets interval from user settings.
        /// </summary>
        [Description("How often to check for Scp106")]
        public float CheckInterval { get; set; } = 10f;

        /// <summary>
        /// Gets or sets a value indicating whether to track suicides.
        /// </summary>
        [Description("Whether to track suicides or not.")]
        public bool TrackSuicides { get; set; } = false;

        /// <summary>
        /// Gets or sets how long to display hint.
        /// </summary>
        [Description("How long to display hint.")]
        public float HintDisplayLimit { get; set; } = 10f;

        /// <summary>
        /// Gets or sets a value indicating whether whether or not to show the MVP screen the following round start.
        /// </summary>
        [Description("Show the MVP screen next round start")]
        public bool ShowOnRoundStart { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether whether or not to show the MVP screen on round end.
        /// </summary>
        [Description("Show the MVP screen next round end")]
        public bool ShowOnRoundEnd { get; set; } = true;

        /// <summary>
        /// Control over what types to show, whether its first come or random per round.
        /// </summary>
        [Description("Control over what types to show, whether its first come or random per round")]
        public class RoundEndConfigurations
        {
            /// <summary>
            /// Gets or sets a value indicating whether control over what types to show, whether its first come or random per round.
            /// </summary>
            [Description("Control over what types to show, whether its first come or random per round")]
            public bool RandomOutputs { get; set; } = true;

            /// <summary>
            /// Gets or sets whether to show first player escape.
            /// </summary>
            [Description("Whether to show first player escape, only takes two params, Player, Float ( use {0} {1})")]
            public Dictionary<bool, string> ShowFirstEscape { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            /// <summary>
            /// Gets or sets whether to show who killed the most entities.
            /// </summary>
            [Description("Whether to show who killed the most entities, only takes two params, Player, Int ( use {0} {1})")]
            public Dictionary<bool, string> ShowMostKillsKiller { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            /// <summary>
            /// Gets or sets whether to show who killed the most humans as SCP on team.
            /// </summary>
            [Description("Whether to show who killed the most humans as SCP on team, only takes three params")]
            public Dictionary<bool, string> ShowMostKillsScpTeam { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            /// <summary>
            /// Gets or sets whether to show who killed the most humans as MTF on team.
            /// </summary>
            [Description("Whether to show who killed the most humans as MTF on team, only takes three params")]
            public Dictionary<bool, string> ShowMostKillsMtfTeam { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            /// <summary>
            /// Gets or sets whether to show who killed the most humans as CHAOS on team.
            /// </summary>
            [Description("Whether to show who killed the most humans as CHAOS on team, only takes three params")]
            public Dictionary<bool, string> ShowMostKillsChaosTeam { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            /// <summary>
            /// Gets or sets whether to show who killed the most humans as human.
            /// </summary>
            [Description("Whether to show who killed the most humans as human, only takes three params")]
            public Dictionary<bool, string> ShowMostKillsHumanOnHuman { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            /// <summary>
            /// Gets or sets whether to show who killed the least humans as human.
            /// </summary>
            [Description("Whether to show who killed the least humans as human, only takes three params")]
            public Dictionary<bool, string> ShowLeastKillsHuman { get; set; } = new Dictionary<bool, string> { { false, string.Empty } };

            /// <summary>
            /// Gets or sets whether to show what to default to.
            /// </summary>
            [Description("Default output if no one escapes (No params)")]
            public Dictionary<bool, string> NoEscapeString { get; set; } = new Dictionary<bool, string> { { false, string.Empty } };

            /// <summary>
            /// Gets a value indicating whether gets or sets whether to show what to default to.
            /// </summary>
            [Description("Whether to force constant updates")]
            public bool ForceConstantUpdate { get; internal set; } = false;

            /// <summary>
            /// Gets whether to show what to default to.
            /// </summary>
            [Description("Hint limit")]
            public ushort HintLimit { get; internal set; } = 3;
        }
    }
}
