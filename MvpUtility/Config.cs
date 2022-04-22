namespace MvpUtility
{
    using Exiled.API.Interfaces;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Config file.
    /// </summary>
    public class Config : IConfig
    {
        /// <inheritdoc />
        public bool IsEnabled { get; set; } = true;

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
        /// Gets or sets how long to display hint.
        /// </summary>
        [Description("How long to display hint.")]
        public float HintDisplayLimit { get; set; } = 10f;

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
            [Description("Whether to show first player escape")]
            public Dictionary<bool, string> ShowFirstEscape { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            /// <summary>
            /// Gets or sets whether to show who killed the most entities.
            /// </summary>
            [Description("Whether to show who killed the most entities")]
            public Dictionary<bool, string> ShowMostKillsKiller { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            /// <summary>
            /// Gets or sets whether to show who killed the most humans as SCP on team.
            /// </summary>
            [Description("Whether to show who killed the most humans as SCP on team")]
            public Dictionary<bool, string> ShowMostKillsScpTeam { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            /// <summary>
            /// Gets or sets whether to show who killed the most humans as MTF on team.
            /// </summary>
            [Description("Whether to show who killed the most humans as MTF on team")]
            public Dictionary<bool, string> ShowMostKillsMtfTeam { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            /// <summary>
            /// Gets or sets whether to show who killed the most humans as CHAOS on team.
            /// </summary>
            [Description("Whether to show who killed the most humans as CHAOS on team")]
            public Dictionary<bool, string> ShowMostKillsChaosTeam { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            /// <summary>
            /// Gets or sets whether to show who killed the most humans as human.
            /// </summary>
            [Description("Whether to show who killed the most humans as human")]
            public Dictionary<bool, string> ShowMostKillsHumanOnHuman { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            /// <summary>
            /// Gets or sets whether to show who killed the least humans as human.
            /// </summary>
            [Description("Whether to show who killed the least humans as human")]
            public Dictionary<bool, string> ShowLeastKillsHuman { get; set; } = new Dictionary<bool, string> { { false, string.Empty } };
        }
    }
}
