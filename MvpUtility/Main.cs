// -----------------------------------------------------------------------
// <copyright file="Main.cs" company="Undid-Iridium">
// Copyright (c) Undid-Iridium. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace MvpUtility
{
    using System;
    using Exiled.API.Features;
    using HarmonyLib;
    using MvpUtility.EventHandling;
    using PlayerEvents = Exiled.Events.Handlers.Player;
    using ServerEvents = Exiled.Events.Handlers.Server;

    /// <summary>
    /// Main plugin instance for MvpPlugin.
    /// </summary>
    public class Main : Plugin<Config>
    {
        private Harmony harmony;

        /// <summary>
        /// Gets a static instance of the <see cref="Main"/> class.
        /// </summary>
        public static Main Instance { get; private set; }

        /// <inheritdoc />
        public override string Author => "Undid-Iridium";

        /// <inheritdoc />
        public override string Name => "MvpUtility";

        /// <inheritdoc />
        public override Version RequiredExiledVersion { get; } = new Version(6, 0, 0);

        /// <inheritdoc />
        public override Version Version { get; } = new Version(2, 0, 0);

        /// <summary>
        /// Gets an instance of the <see cref="MvpStats"/> class.
        /// </summary>
        public MvpStats MvpStatsMonitor { get; private set; }

        /// <inheritdoc />
        public override void OnEnabled()
        {
            Instance = this;

            harmony = new Harmony($"com.Undid-Iridium.MvpUtility.{DateTime.UtcNow.Ticks}");
            harmony.PatchAll();

            MvpStatsMonitor = new MvpStats(this);

            PlayerEvents.Escaping += MvpStatsMonitor.OnEscape;
            PlayerEvents.Dying += MvpStatsMonitor.OnDying;
            ServerEvents.RoundStarted += MvpStatsMonitor.OnStart;

            ServerEvents.RoundEnded += MvpStatsMonitor.OnRoundEnd;

            base.OnEnabled();
        }

        /// <inheritdoc />
        public override void OnDisabled()
        {
            harmony.UnpatchAll(harmony.Id);
            harmony = null;

            PlayerEvents.Escaping -= MvpStatsMonitor.OnEscape;
            ServerEvents.RoundStarted -= MvpStatsMonitor.OnStart;
            PlayerEvents.Dying -= MvpStatsMonitor.OnDying;
            ServerEvents.RoundEnded -= MvpStatsMonitor.OnRoundEnd;

            MvpStatsMonitor = null;
            Instance = null;
            base.OnDisabled();
        }
    }
}
