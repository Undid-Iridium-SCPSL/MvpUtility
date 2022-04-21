using Exiled.API.Features;
using Exiled.Events.EventArgs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MvpUtility.EventHandling
{
    public class MvpStats
    {
        public float roundStartTime;
        private Main plugin;

        /// <summary>
        /// Initializes a new instance of the <see cref="PickupChecker"/> class.
        /// </summary>
        /// <param name="plugin">An instance of the <see cref="Plugin"/> class.</param>
        public MvpStats(Main plugin) => this.plugin = plugin;

        internal Dictionary<String, KillCounterUtility> mostKillsPlayer;

        internal void roundStarted(float time)
        {
            roundStartTime = time;
            mostKillsPlayer = new Dictionary<String, KillCounterUtility>();
        }

        public void OnEscape(EscapingEventArgs ev)
        {
            throw new NotImplementedException();
        }

        public void OnStart()
        {
            roundStarted(Time.time);
        }

        internal void OnDying(DyingEventArgs ev)
        {

            // Try to add new killer, and then parse their behavior types
            if (!mostKillsPlayer.TryAdd(ev.Killer.Nickname, new KillCounterUtility(ev.Killer)))
            {
                mostKillsPlayer[ev.Killer.Nickname].parseKillerStats(ev.Killer);
            }
            //Either just do this directly or give a queue this data for a thread, so we can offload logic to threads
            //Had most kills, killed most scp's, escaped first, most kills per team Nickname : KillCounter(class)
        }
    }
}