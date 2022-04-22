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
            public bool showFirstEscape { get; set; } = true;

            [Description("Whether to show who killed the most entities")]
            public bool showMostKillsKiller { get; set; } = true;

            [Description("Whether to show who killed the most humans as SCP on team")]
            public bool showMostKillsScpTeam { get; set; } = true;

            [Description("Whether to show who killed the most humans as MTF on team")]
            public bool showMostKillsMtfTeam { get; set; } = true;

            [Description("Whether to show who killed the most humans as CHAOS on team")]
            public bool showMostKillsChaosTeam { get; set; } = true;

            [Description("Whether to show who killed the most humans as human")]
            public bool showMostKillsHumanOnHuman { get; set; } = true;

            [Description("Whether to show who killed the least humans as human")]
            public bool showLeastKillsHuman { get; set; } = false;


        }

        [Description("Control over what types to show")]
        public roundEndConfigurations roundEndBehaviors { get; set; } = new roundEndConfigurations();
    }
}
