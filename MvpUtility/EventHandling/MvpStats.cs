// -----------------------------------------------------------------------
// <copyright file="MvpStats.cs" company="Undid-Iridium">
// Copyright (c) Undid-Iridium. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace MvpUtility.EventHandling
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using MEC;
    using UnityEngine;

    /// <summary>
    /// Main container for handling parsing of killer type, and target data.
    /// </summary>
    public class MvpStats
    {
        /// <summary>
        /// When the round started.
        /// </summary>
        private float roundStartTime;

        private Main plugin;

        /// <summary>
        /// All the player's by name, and their associated utility to count/parse stats.
        /// </summary>
        private Dictionary<string, KillCounterUtility> listOfPlayersKillStats;

        private CoroutineHandle scp106ValidatorCoroutine;
        private string lastGeneratedHint = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MvpStats"/> class.
        /// </summary>
        /// <param name="plugin">An instance of the <see cref="Main"/> class.</param>
        public MvpStats(Main plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Gets or sets player information on first player to escape.
        /// </summary>
        public Tuple<string, float> FirstPlayerEscape { get; set; } = null;

        /// <summary>
        /// Gets or sets player known 106 player object.
        /// </summary>
        public Player LastKnown106 { get; set; }

        /// <summary>
        /// Delegate function for OnRoundStarted.
        /// </summary>
        public void OnStart()
        {
            if (plugin.Config.ShowOnRoundStart)
            {
                if (lastGeneratedHint is null)
                {
                    lastGeneratedHint = GenerateOutputHintList();
                }

                if (lastGeneratedHint is not null)
                {
                    if (plugin.Config.RoundEndBehaviors.ForceConstantUpdate)
                    {
                        Timing.RunCoroutine(ForceConstantUpdate(lastGeneratedHint, (int)plugin.Config.HintDisplayLimit));
                    }
                    else
                    {
                        Map.ShowHint(lastGeneratedHint, plugin.Config.HintDisplayLimit);
                    }
                }
            }

            RoundStarted(Time.time);
        }

        /// <summary>
        /// Just used to mark who was the first player to escape.
        /// </summary>
        /// <param name="ev"> <see cref="EscapingEventArgs"/> is hooked to grab the first player that escaped. </param>
        public void OnEscape(EscapingEventArgs ev)
        {
            if (FirstPlayerEscape == null)
            {
                FirstPlayerEscape = Tuple.Create(ev.Player.Nickname, Time.time - roundStartTime);
            }
        }

        /// <summary>
        /// Cleanup logic run on Round Start.
        /// </summary>
        /// <param name="time"> Start of the current round. </param>
        internal void RoundStarted(float time)
        {
            lastGeneratedHint = null;
            roundStartTime = time;
            listOfPlayersKillStats = new Dictionary<string, KillCounterUtility>();
            scp106ValidatorCoroutine = Timing.RunCoroutine(CheckLast106());
            FirstPlayerEscape = null;
        }

        /// <summary>
        /// Tracks players dying, right before their death and stores the killer/target information.
        /// </summary>
        /// <param name="ev"> <see cref="DyingEventArgs"/>.</param>
        internal void OnDying(DyingEventArgs ev)
        {
            try
            {
                // Pocket dimension teleport calling this?
                if (ev.Target.Health > 0)
                {
                    return;
                }

                if (ev.Target.Role is Exiled.API.Features.Roles.Scp106Role)
                {
                    LastKnown106 = null;
                    Timing.KillCoroutines(scp106ValidatorCoroutine);
                }

                if (ev.Handler != null)
                {
                    if (ev.Handler.Type is Exiled.API.Enums.DamageType.PocketDimension && LastKnown106 != null)
                    {
                        listOfPlayersKillStats.TryAddKey(LastKnown106.Nickname, new KillCounterUtility(plugin));
                        listOfPlayersKillStats[LastKnown106.Nickname].ParseKillerStats(LastKnown106, ev.Target);
                        return;
                    }
                }

                // Try to add new killer, and then parse their behavior types
                if (ev.Killer == null)
                {
                    return;
                }

                if (ev.Killer.Nickname == null)
                {
                    return;
                }

                // Do not allow suicides (Add config)
                if (ev.Killer == ev.Target && !plugin.Config.TrackSuicides)
                {
                    return;
                }

                listOfPlayersKillStats.TryAddKey(ev.Killer.Nickname, new KillCounterUtility(plugin));
                listOfPlayersKillStats[ev.Killer.Nickname].ParseKillerStats(ev.Killer, ev.Target);
            }
            catch (Exception ex)
            {
                Log.Debug($" It seems we failed on OnDying, here's why {ex}", plugin.Config.EnableDebug);
            }

            // Either just do this directly or give a queue this data for a thread, so we can offload logic to threads
            // Had most kills, killed most scp's, escaped first, most kills per team Nickname : KillCounter(class)
        }

        /// <summary>
        /// Called when round is ending, processes statistical data to determine end-round outputs. <see cref="MvpStats"/>.
        /// </summary>
        /// <param name="ev"> <see cref="RoundEndedEventArgs"/>.</param>
        internal void OnRoundEnd(RoundEndedEventArgs ev)
        {
            string hintToShow = GenerateOutputHintList();

            this.lastGeneratedHint = hintToShow;

            // We still call the hint generation logic because, if we need to use it for round start, it is still cheaper to do it when round ends.
            if(!this.plugin.Config.ShowOnRoundEnd) {
                return;
            }

            // Iterate every player and show the hints.
            if (plugin.Config.RoundEndBehaviors.ForceConstantUpdate)
            {
                Timing.RunCoroutine(ForceConstantUpdate(hintToShow, (int)plugin.Config.HintDisplayLimit));
            }
            else
            {
                foreach (Player player in Player.List)
                {
                    player.ShowHint(hintToShow, plugin.Config.HintDisplayLimit);
                }
            }

            try
            {
                roundStartTime = 0;
                listOfPlayersKillStats = null;
                FirstPlayerEscape = null;
                if (scp106ValidatorCoroutine.IsRunning)
                {
                    Timing.KillCoroutines(scp106ValidatorCoroutine);
                }
            }
            catch (Exception unableToClearFields)
            {
                Log.Debug($"Unable to clear fields for MvpStats {unableToClearFields}", plugin.Config.EnableDebug);
            }
        }

        /// <summary>
        /// Generates the hint string to show on screen.
        /// </summary>
        private string GenerateOutputHintList()
        {
            if(this.FirstPlayerEscape is null && this.listOfPlayersKillStats is null){
                return null;
            }

            // The following segments of code is the best I can come up without reflection usage
            List<string> outputList = new List<string>();

            // Generates our strings we will use based on config.
            HandlePossibleOutputs(ref outputList);

            List<string> choices = new List<string>();

            ushort defaultLimit = plugin.Config.RoundEndBehaviors.HintLimit;

            // If we're going to use random outputs, we need to verify we do no repeat.
            if (plugin.Config.RoundEndBehaviors.RandomOutputs)
            {
                HashSet<int> limitedChoices = new HashSet<int>();

                // If we have more than 3 outputs, we will output a random 3 of them
                if (outputList.Count >= defaultLimit)
                {
                    while (choices.Count != defaultLimit)
                    {
                        int currRandom = UnityEngine.Random.Range(0, outputList.Count);
                        if (limitedChoices.Contains(currRandom))
                        {
                            continue;
                        }

                        choices.Add(outputList[currRandom]);
                        limitedChoices.Add(currRandom);
                    }
                }
                else
                {
                    // If we don't have 3 outputs available then we assign output list and if that happened to have less than 3
                    // we add dummy values
                    choices = outputList;
                    while (choices.Count != defaultLimit)
                    {
                        choices.Add(string.Empty);
                    }
                }
            }
            else
            {
                // Iterate our possibility output list to find the first defaultLimit quantity, if there are.
                for (int pos = 0; pos < outputList.Count && choices.Count != defaultLimit; pos++)
                {
                    choices.Add(outputList[pos]);
                }

                // If in some case we don't have defaultLimit quantity available, dummy ones get added.
                while (choices.Count != defaultLimit)
                {
                    choices.Add(string.Empty);
                }
            }

            StringBuilder outputHint = new StringBuilder();

            for (int pos = 0; pos < choices.Count; pos++)
            {
                outputHint.Append(choices[pos]);
            }

            return outputHint.ToString();

        }

        private IEnumerator<float> ForceConstantUpdate(string hintToShow, int hintDisplayLimit)
        {
            int iterationCounter = 0;
            while (iterationCounter < hintDisplayLimit)
            {
                foreach (Player player in Player.List)
                {
                    player.ShowHint(hintToShow, plugin.Config.HintDisplayLimit);
                }

                yield return Timing.WaitForSeconds(1);
                iterationCounter++;
            }
        }

        /// <summary>
        /// Coroutine to iterate player list every X seconds to find 106 player (DcReplace protection).
        /// </summary>
        /// <returns> IEnumerator for MEC.</returns>
        private IEnumerator<float> CheckLast106()
        {
            while (Round.IsStarted)
            {
                yield return Timing.WaitForSeconds(plugin.Config.CheckInterval);
                foreach (Player player in Player.List)
                {
                    if (player.Role is Exiled.API.Features.Roles.Scp106Role)
                    {
                        LastKnown106 = player;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Generates output strings by iterating each player and their played roles. This is an expensive operation as we iterate 32 players
        /// who may have had 3-5 roles who may have killed 3-5 different rows so in worst case O(N * (K * M)) on average probably being O(32 * (5 * 5))
        /// Being 1 player needs to iterate 5 roles that iterate 5 target roles. 25 in total. Then times 32 = 800 iterations to get the data we need.
        /// </summary>
        /// <param name="outputList"> list to updated with new hintstring results. </param>
        private void HandlePossibleOutputs(ref List<string> outputList)
        {
            if (plugin.Config.RoundEndBehaviors.ShowFirstEscape.TryGetValue(true, out string customString))
            {
                if (FirstPlayerEscape != null)
                {
                    customString = customString ?? string.Empty;
                    if (customString.Equals(string.Empty))
                    {
                        outputList.Add($"<line-height=75%><voffset=30em><align=center><color=#247BA0> {FirstPlayerEscape.Item1} </color> was the first person to escape within {TimeSpan.FromSeconds(FirstPlayerEscape.Item2):mm\\:ss\\:fff}'s </align> </voffset> \n");
                    }
                    else
                    {
                        outputList.Add(string.Format(customString, FirstPlayerEscape.Item1, TimeSpan.FromSeconds(FirstPlayerEscape.Item2).ToString(@"mm\:ss\:fff")));
                    }
                }
                else
                {
                    if (plugin.Config.RoundEndBehaviors.NoEscapeString.TryGetValue(true, out string customDefaultString))
                    {
                        outputList.Add(customDefaultString);
                    }
                    else
                    {
                        outputList.Add($"<line-height=75%><voffset=30em><align=center><color=#6874E8> No one escaped this round </color>  </align> </voffset> \n");
                    }
                }
            }

            List<Tuple<string, RoleType, int>> possibleOutcomes = new List<Tuple<string, RoleType, int>>()
            {
                Tuple.Create(string.Empty, RoleType.None, int.MaxValue), // Worst player
                Tuple.Create(string.Empty, RoleType.None, int.MinValue), // Best player (Most kills human on human)
                Tuple.Create(string.Empty, RoleType.None, int.MinValue), // Best player (Killer in general, all entities)
                Tuple.Create(string.Empty, RoleType.None, int.MinValue), // Best player (Best per Mtf)
                Tuple.Create(string.Empty, RoleType.None, int.MinValue), // Best player (Best per Chaos)
                Tuple.Create(string.Empty, RoleType.None, int.MinValue), // Best player (Best per ScpTeam)
            };

            foreach (KeyValuePair<string, KillCounterUtility> killerPairedData in listOfPlayersKillStats)
            {
                Log.Debug($"What was our killer paired data {killerPairedData.Key}, {killerPairedData.Value}");
                if (plugin.Config.RoundEndBehaviors.ShowLeastKillsHuman.ContainsKey(true))
                {
                    // Preallocated position of 0, only way I could think to solve without multi-for loops (Same for the rest)
                    HandlePlayerBundling(ref possibleOutcomes, killerPairedData.Value.GetWorstRoleHuman(), killerPairedData.Key, 0, true);
                }

                if (plugin.Config.RoundEndBehaviors.ShowMostKillsHumanOnHuman.ContainsKey(true))
                {
                    HandlePlayerBundling(ref possibleOutcomes, killerPairedData.Value.GetBestHumanToHuman(), killerPairedData.Key, 1);
                }

                if (plugin.Config.RoundEndBehaviors.ShowMostKillsKiller.ContainsKey(true))
                {
                    HandlePlayerBundling(ref possibleOutcomes, killerPairedData.Value.GetBestKiller(), killerPairedData.Key, 2);
                }

                if (plugin.Config.RoundEndBehaviors.ShowMostKillsMtfTeam.ContainsKey(true))
                {
                    HandlePlayerBundling(ref possibleOutcomes, killerPairedData.Value.GetBestKillsPerTeam(Team.MTF), killerPairedData.Key, 3);
                }

                if (plugin.Config.RoundEndBehaviors.ShowMostKillsChaosTeam.ContainsKey(true))
                {
                    HandlePlayerBundling(ref possibleOutcomes, killerPairedData.Value.GetBestKillsPerTeam(Team.CHI), killerPairedData.Key, 4);
                }

                if (plugin.Config.RoundEndBehaviors.ShowMostKillsScpTeam.ContainsKey(true))
                {
                    HandlePlayerBundling(ref possibleOutcomes, killerPairedData.Value.GetBestKillsPerTeam(Team.SCP), killerPairedData.Key, 5);
                }
            }

            // Alternative is a for loop but the problem is if I do if if, I run same logic, if I do else if, I run into skipping
            // because the first if, or nth will always be called

            // TODO make return a struct/class that has a success/fail flag instead of this checking crap against something crap.
            if (plugin.Config.RoundEndBehaviors.ShowLeastKillsHuman.ContainsKey(true))
            {
                if (possibleOutcomes[0].Item2 != RoleType.None)
                {
                    customString = plugin.Config.RoundEndBehaviors.ShowLeastKillsHuman[true] ?? string.Empty;
                    GenerateString(ref outputList, possibleOutcomes, customString, $"<line-height=75%><voffset=30em><align=center><color=#F6511D> {possibleOutcomes[0].Item1} </color> had {possibleOutcomes[0].Item3} kills, how sad. </align> </voffset> \n", 0);
                }
            }

            if (plugin.Config.RoundEndBehaviors.ShowMostKillsHumanOnHuman.ContainsKey(true))
            {
                if (possibleOutcomes[1].Item2 != RoleType.None)
                {
                    customString = plugin.Config.RoundEndBehaviors.ShowMostKillsHumanOnHuman[true] ?? string.Empty;
                    GenerateString(ref outputList, possibleOutcomes, customString, $"<line-height=75%><voffset=30em><align=center><color=#241623> {possibleOutcomes[1].Item1} </color>" + $" had {possibleOutcomes[1].Item3} kills as a lonely human. </align> </voffset> \n", 1);
                }
            }

            if (plugin.Config.RoundEndBehaviors.ShowMostKillsKiller.ContainsKey(true))
            {
                if (!possibleOutcomes[2].Item1.Equals(string.Empty) && possibleOutcomes[2].Item3 > 0)
                {
                    customString = plugin.Config.RoundEndBehaviors.ShowMostKillsKiller[true] ?? string.Empty;

                    GenerateString(ref outputList, possibleOutcomes, customString, $"<line-height=75%><voffset=30em><align=center><color=#D0CD94> {possibleOutcomes[2].Item1} </color>" + $" had killed {possibleOutcomes[2].Item3} entities. </align> </voffset> \n", 2);
                }
            }

            if (plugin.Config.RoundEndBehaviors.ShowMostKillsMtfTeam.ContainsKey(true))
            {
                if (possibleOutcomes[3].Item2 != RoleType.None)
                {
                    customString = plugin.Config.RoundEndBehaviors.ShowMostKillsMtfTeam[true] ?? string.Empty;
                    GenerateString(ref outputList, possibleOutcomes, customString, $"<line-height=75%><voffset=30em><align=center><color=#3C787E> {possibleOutcomes[3].Item1} </color>" + $" had {possibleOutcomes[3].Item3} kills as {possibleOutcomes[3].Item2} (MTF). </align> </voffset> \n", 3);
                }
            }

            if (plugin.Config.RoundEndBehaviors.ShowMostKillsChaosTeam.ContainsKey(true))
            {
                if (possibleOutcomes[4].Item2 != RoleType.None)
                {
                    customString = plugin.Config.RoundEndBehaviors.ShowMostKillsChaosTeam[true] ?? string.Empty;
                    GenerateString(ref outputList, possibleOutcomes, customString, $"<line-height=75%><voffset=30em><align=center><color=#C7EF00> {possibleOutcomes[4].Item1} </color>" + $" had {possibleOutcomes[4].Item3} kills as {possibleOutcomes[4].Item2} (Chaos). </align> </voffset> \n", 4);
                }
            }

            if (plugin.Config.RoundEndBehaviors.ShowMostKillsScpTeam.ContainsKey(true))
            {
                if (possibleOutcomes[5].Item2 != RoleType.None)
                {
                    customString = plugin.Config.RoundEndBehaviors.ShowMostKillsScpTeam[true] ?? string.Empty;
                    GenerateString(ref outputList, possibleOutcomes, customString, $"<line-height=75%><voffset=30em><align=center><color=#D56F3E> {possibleOutcomes[5].Item1} </color>" + $" had {possibleOutcomes[5].Item3} kills as {possibleOutcomes[5].Item2} (SCP). </align> </voffset> \n", 5);
                }
            }
        }

        /// <summary>
        /// Generates the string for the hints to display. If config does not have a value set this will default to a specific string per scenario.
        /// </summary>
        /// <param name="outputList"> List of output strings. </param>
        /// <param name="possibleOutcomes"> All current possible outcomes of roles/killers. </param>
        /// <param name="configValue"> Whether to run the default string or config string. </param>
        /// <param name="defaultValue"> Default string set by program. </param>
        /// <param name="pos"> Current position in possible outcomes. </param>
        private void GenerateString(ref List<string> outputList, List<Tuple<string, RoleType, int>> possibleOutcomes, string configValue, string defaultValue, int pos)
        {
            if (configValue.Equals(string.Empty))
            {
                outputList.Add(defaultValue);
            }
            else
            {
                outputList.Add(string.Format(configValue, possibleOutcomes[pos].Item1, possibleOutcomes[pos].Item2, possibleOutcomes[pos].Item3));
            }
        }

        /// <summary>
        /// This logic handles generating the paired data required for outputing to hint for all scenarios.
        /// </summary>
        /// <param name="possibleOutcomes"> List of the outcomes enabled in the config/defaults. </param>
        /// <param name="currentRoleCalc"> Calculation of the player's best role, and kill counter. </param>
        /// <param name="killerPairedDataName"> Name of current player. </param>
        /// <param name="outcomePosition"> What location in the outputList is associated.</param>
        /// <param name="lessThanLogic"> Whether we're checking greater than previous or less than. </param>
        private void HandlePlayerBundling(ref List<Tuple<string, RoleType, int>> possibleOutcomes, Tuple<RoleType, int> currentRoleCalc, string killerPairedDataName, int outcomePosition, bool lessThanLogic = false)
        {
            if (possibleOutcomes[outcomePosition].Item1.IsEmpty())
            {
                possibleOutcomes[outcomePosition] = Tuple.Create(killerPairedDataName, currentRoleCalc.Item1, currentRoleCalc.Item2);
            }
            else
            {
                if (currentRoleCalc.Item2 > possibleOutcomes[outcomePosition].Item3)
                {
                    possibleOutcomes[outcomePosition] = Tuple.Create(killerPairedDataName, currentRoleCalc.Item1, currentRoleCalc.Item2);
                }
                else if (lessThanLogic)
                {
                    possibleOutcomes[outcomePosition] = Tuple.Create(killerPairedDataName, currentRoleCalc.Item1, currentRoleCalc.Item2);
                }
            }
        }
    }
}