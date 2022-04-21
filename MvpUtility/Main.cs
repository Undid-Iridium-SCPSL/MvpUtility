using Exiled.API.Features;
using HarmonyLib;
using MvpUtility.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerEvents = Exiled.Events.Handlers.Player;
using ServerEvents = Exiled.Events.Handlers.Server;


namespace MvpUtility
{


    public class Main : Plugin<Config>
    {
        private Harmony harmony;

        /// <summary>
        /// Gets a static instance of the <see cref="Plugin"/> class.
        /// </summary>
        public static Main Instance { get; private set; }

        /// <inheritdoc />
        public override string Author => "Undid-Iridium";

        /// <inheritdoc />
        public override string Name => "MvpUtility";

        /// <inheritdoc />
        public override Version RequiredExiledVersion { get; } = new Version(5, 1, 3);

        /// <inheritdoc />
        public override Version Version { get; } = new Version(1, 1, 4);

        /// <summary>
        /// Gets an instance of the <see cref="MvpStats"/> class.
        /// </summary>
        public MvpStats mvpStatsMonitor { get; private set; }

        /// <inheritdoc />
        public override void OnEnabled()
        {
            Instance = this;

            harmony = new Harmony($"com.Undid-Iridium.MvpUtility.{DateTime.UtcNow.Ticks}");
            harmony.PatchAll();

            mvpStatsMonitor = new MvpStats(this);

            PlayerEvents.Escaping += mvpStatsMonitor.OnEscape;
            ServerEvents.RoundStarted += mvpStatsMonitor.OnStart;
            PlayerEvents.Dying += mvpStatsMonitor.OnDying;

            base.OnEnabled();
        }

        /// <inheritdoc />
        public override void OnDisabled()
        {


            harmony.UnpatchAll(harmony.Id);
            harmony = null;


            PlayerEvents.Escaping -= mvpStatsMonitor.OnEscape;
            ServerEvents.RoundStarted -= mvpStatsMonitor.OnStart;

            Instance = null;
            base.OnDisabled();
        }
    }
}
