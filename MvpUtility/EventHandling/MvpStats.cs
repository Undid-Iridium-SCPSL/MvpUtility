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

        public Tuple<String, float> firstPlayerEscape;



        internal void roundStarted(float time)
        {
            roundStartTime = time;
            mostKillsPlayer = new Dictionary<String, KillCounterUtility>();
        }

        public void OnEscape(EscapingEventArgs ev)
        {
            firstPlayerEscape = Tuple.Create(ev.Player.Nickname, Time.time - roundStartTime);
        }

        public void OnStart()
        {
            roundStarted(Time.time);
        }

        internal void OnDying(DyingEventArgs ev)
        {

            // Try to add new killer, and then parse their behavior types
            if (ev.Killer == null)
            {
                return;
            }
            if (ev.Killer.Nickname == null)
            {
                return;
            }
            mostKillsPlayer.TryAdd(ev.Killer.Nickname, new KillCounterUtility(ev.Killer, plugin));
            mostKillsPlayer[ev.Killer.Nickname].parseKillerStats(ev.Killer, ev.Target);

            //Either just do this directly or give a queue this data for a thread, so we can offload logic to threads
            //Had most kills, killed most scp's, escaped first, most kills per team Nickname : KillCounter(class)
        }

        internal void OnRoundEnd(RoundEndedEventArgs ev)
        {

            //if(Config for output) then if (value exists) then add to output possibilities then do random on list of possibilities
            //


            Log.Debug($"RoundEnd1 {mostKillsPlayer.Count}", plugin.Config.enableDebug);

            List<String> outputList = new List<String>();




            // This is best I can come up without reflection usage
            if (plugin.Config.roundEndBehaviors.showFirstEscape)
            {
                if (firstPlayerEscape != null)
                {
                    outputList.Add($"<align=center><color=#247BA0> {firstPlayerEscape.Item1} </color> was the first person to escape within {TimeSpan.FromSeconds(firstPlayerEscape.Item2).ToString(@"mm\:ss\:fff")}'s </align> \n");
                }
            }
            Log.Debug("RoundEnd2", plugin.Config.enableDebug);
            // Generates our strings we will use based on config. 
            handlePossibleOutputs(ref outputList);

            List<string> choices = new List<string>();
            Log.Debug("RoundEnd3", plugin.Config.enableDebug);
            if (plugin.Config.roundEndBehaviors.randomOutputs)
            {
                HashSet<int> limitedChoices = new HashSet<int>();

                if (outputList.Count >= 3)
                {
                    while (choices.Count != 3)
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
                    choices = outputList;
                    while (choices.Count != 3)
                    {
                        choices.Add("");
                    }
                }
            }
            else
            {
                for (int pos = 0; pos < outputList.Count && choices.Count != 3; pos++)
                {
                    if (outputList[pos] == null)
                    {
                        break;
                    }
                    choices.Add(outputList[pos]);
                }
            }

            foreach (Player player in Player.List)
            {
                // String.Join(' ', choices.ToArray()) also works but no, why bother to slow it down. It's always 3. Unless that changes. 
                player.ShowHint($"{new string('\n', 15)} " + choices[0] + choices[1] + choices[2], 10);
            }
            firstPlayerEscape = null;
            mostKillsPlayer = null;

        }

        private void handlePossibleOutputs(ref List<string> outputList)
        {
            Log.Debug("handlePossibleOutputs1", plugin.Config.enableDebug);
            List<Tuple<String, RoleType, int>> possibleOutcomes = new List<Tuple<String, RoleType, int>>(){
                Tuple.Create("", RoleType.None, int.MaxValue ), // Worst player
                Tuple.Create("", RoleType.None, int.MinValue ), // Best player (Most kills human on human)
                Tuple.Create("", RoleType.None, int.MinValue ), // Best player (Killer in general, all entities)
                Tuple.Create("", RoleType.None, int.MinValue ), // Best player (Best per Mtf)
                Tuple.Create("", RoleType.None, int.MinValue ), // Best player (Best per Chaos)
                Tuple.Create("", RoleType.None, int.MinValue ), // Best player (Best per ScpTeam)
            };

            Log.Debug("handlePossibleOutputs2", plugin.Config.enableDebug);
            foreach (KeyValuePair<String, KillCounterUtility> killerPairedData in mostKillsPlayer)
            {
                Log.Debug($"killerPairedData {killerPairedData.Key} and {killerPairedData.Value}", plugin.Config.enableDebug);
                if (plugin.Config.roundEndBehaviors.showLeastKillsHuman)
                {
                    // Preallocated position of 0, only way I could think to solve without multi-for loops
                    handleWorstPlayer(ref possibleOutcomes, killerPairedData.Value.getWorstRole(), killerPairedData.Key, 0);
                }
                if (plugin.Config.roundEndBehaviors.showMostKillsHumanOnHuman)
                {
                    handleBestPlayer(ref possibleOutcomes, killerPairedData.Value.getBestHumanToHuman(), killerPairedData.Key, 1);
                }
                if (plugin.Config.roundEndBehaviors.showMostKillsKiller)
                {
                    handleBestPlayer(ref possibleOutcomes, killerPairedData.Value.getBestKiller(), killerPairedData.Key, 2);
                }
                if (plugin.Config.roundEndBehaviors.showMostKillsMtfTeam)
                {
                    handleBestPlayer(ref possibleOutcomes, killerPairedData.Value.getBestKillsPerTeam(Team.MTF), killerPairedData.Key, 3);
                }

                if (plugin.Config.roundEndBehaviors.showMostKillsChaosTeam)
                {
                    handleBestPlayer(ref possibleOutcomes, killerPairedData.Value.getBestKillsPerTeam(Team.CHI), killerPairedData.Key, 4);
                }
                if (plugin.Config.roundEndBehaviors.showMostKillsScpTeam)
                {
                    handleBestPlayer(ref possibleOutcomes, killerPairedData.Value.getBestKillsPerTeam(Team.SCP), killerPairedData.Key, 5);
                }

            }
            Log.Debug("handlePossibleOutputs3", plugin.Config.enableDebug);
            // Alternative is a for loop but the problem is if I do if if, I run same logic, if I do else if, I run into skipping 
            // because the first if, or nth will always be called

            if (plugin.Config.roundEndBehaviors.showLeastKillsHuman)
            {
                if (possibleOutcomes[0].Item3 != int.MaxValue)
                {
                    outputList.Add($"<align=center><color=#F6511D> {possibleOutcomes[0].Item1} </color>" +
                  $" killed {possibleOutcomes[0].Item3} people, how sad. </align> \n");
                }


            }
            if (plugin.Config.roundEndBehaviors.showMostKillsHumanOnHuman)
            {
                if (possibleOutcomes[1].Item3 != int.MaxValue && possibleOutcomes[1].Item3 != int.MinValue)
                {
                    outputList.Add($"<align=center><color=#241623> {possibleOutcomes[1].Item1} </color>" +
               $" killed {possibleOutcomes[1].Item3} person as a human. </align> \n");
                }
            }
            if (plugin.Config.roundEndBehaviors.showMostKillsKiller)
            {
                if (possibleOutcomes[2].Item3 != int.MinValue)
                {
                    outputList.Add($"<align=center><color=#D0CD94> {possibleOutcomes[2].Item1} </color>" +
               $" killed {possibleOutcomes[2].Item3} entities. </align> \n");
                }

            }
            if (plugin.Config.roundEndBehaviors.showMostKillsMtfTeam)
            {
                if (possibleOutcomes[3].Item3 != int.MinValue)
                {
                    outputList.Add($"<align=center><color=#3C787E> {possibleOutcomes[3].Item1} </color>" +
                $" killed {possibleOutcomes[3].Item3} people as {possibleOutcomes[3].Item2} (MTF). </align> \n");
                }

            }

            if (plugin.Config.roundEndBehaviors.showMostKillsChaosTeam)
            {
                if (possibleOutcomes[4].Item3 != int.MinValue)
                {
                    outputList.Add($"<align=center><color=#C7EF00> {possibleOutcomes[4].Item1} </color>" +
                $" killed {possibleOutcomes[4].Item3} people as {possibleOutcomes[4].Item2} (Chaos). </align> \n");
                }
            }

            if (plugin.Config.roundEndBehaviors.showMostKillsScpTeam)
            {
                if (possibleOutcomes[5].Item3 != int.MinValue)
                {
                    outputList.Add($"<align=center><color=#D56F3E> {possibleOutcomes[5].Item1} </color>" +
                $" killed {possibleOutcomes[5].Item3} people as {possibleOutcomes[5].Item2} (SCP). </align> \n");
                }
            }
            Log.Debug("handlePossibleOutputs4", plugin.Config.enableDebug);
        }

        private void handleBestPlayer(ref List<Tuple<string, RoleType, int>> possibleOutcomes, Tuple<RoleType, int> bestRoleCalc, string killerPairedDataName, int outcomePosition)
        {
            if (possibleOutcomes[outcomePosition].Item1.IsEmpty())
            {
                possibleOutcomes[outcomePosition] = (Tuple.Create(killerPairedDataName, bestRoleCalc.Item1, bestRoleCalc.Item2));
            }
            else
            {
                if (bestRoleCalc.Item2 > possibleOutcomes[outcomePosition].Item3)
                {
                    possibleOutcomes[outcomePosition] = (Tuple.Create(killerPairedDataName, bestRoleCalc.Item1, bestRoleCalc.Item2));
                }
            }
        }

        private void handleWorstPlayer(ref List<Tuple<string, RoleType, int>> possibleOutcomes, Tuple<RoleType, int> worstRoleCalc, string killerPairedDataName, int outcomePosition)
        {
            if (possibleOutcomes[outcomePosition].Item1.IsEmpty())
            {
                possibleOutcomes[outcomePosition] = (Tuple.Create(killerPairedDataName, worstRoleCalc.Item1, worstRoleCalc.Item2));
            }
            else
            {
                if (worstRoleCalc.Item2 < possibleOutcomes[outcomePosition].Item3)
                {
                    possibleOutcomes[outcomePosition] = (Tuple.Create(killerPairedDataName, worstRoleCalc.Item1, worstRoleCalc.Item2));
                }
            }
        }
    }
}