using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvpUtility
{
    public class Config : IConfig
    {
        /// <inheritdoc />
        public bool IsEnabled { get; set; } = true;

        public bool enableDebug { get; set; } = false;

        public class roundEndConfigurations
        {
            [Description("Control over what types to show, whether its first come or random per round")]
            public bool randomOutputs { get; set; } = true;


            [Description("Whether to show first player escape")]
            public Dictionary<bool, string> showFirstEscape { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            [Description("Whether to show who killed the most entities")]
            public Dictionary<bool, string> showMostKillsKiller { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            [Description("Whether to show who killed the most humans as SCP on team")]
            public Dictionary<bool, string> showMostKillsScpTeam { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            [Description("Whether to show who killed the most humans as MTF on team")]
            public Dictionary<bool, string> showMostKillsMtfTeam { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            [Description("Whether to show who killed the most humans as CHAOS on team")]
            public Dictionary<bool, string> showMostKillsChaosTeam { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            [Description("Whether to show who killed the most humans as human")]
            public Dictionary<bool, string> showMostKillsHumanOnHuman { get; set; } = new Dictionary<bool, string> { { true, string.Empty } };

            [Description("Whether to show who killed the least humans as human")]
            public Dictionary<bool, string> showLeastKillsHuman { get; set; } = new Dictionary<bool, string> { { false, string.Empty } };


        }

        [Description("Control over what types to show")]
        public roundEndConfigurations roundEndBehaviors { get; set; } = new roundEndConfigurations();
        public float CheckInterval { get; set; } = 10f;
    }
}
