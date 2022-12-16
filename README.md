# MvpUtility

This plugin basically offers some MVP stats at the end of the round, and you can modify these strings or leave them as blank so defaults will run. This type of text formatting is dependent on http://digitalnativestudios.com/textmeshpro/docs/rich-text/#line-height so if you want it on the bottom you will have to adhere to the richtext rules. Also at this time formatting requires all 3 variables (Name, RoleType, Counter) to be specified. I may if requested make it dynamic where you can reject or rather ignore some of them (such as putting {0} {2} and if I don't see {1} not to put it) but that will be by request. 

![MvpUtility ISSUES](https://img.shields.io/github/issues/Undid-Iridium/MvpUtility)
![MvpUtility FORKS](https://img.shields.io/github/forks/Undid-Iridium/MvpUtility)
![MvpUtility LICENSE](https://img.shields.io/github/license/Undid-Iridium/MvpUtility)


![MvpUtility LATEST](https://img.shields.io/github/v/release/Undid-Iridium/MvpUtility?include_prereleases&style=flat-square)
![MvpUtility LINES](https://img.shields.io/tokei/lines/github/Undid-Iridium/MvpUtility)
![MvpUtility DOWNLOADS](https://img.shields.io/github/downloads/Undid-Iridium/MvpUtility/total?style=flat-square)

TODO:
* Add more configuration for locked values
* Allow ignoring of fields for format string
* Add the ability for fully customizable strings where I list variables that can be called, and then you can build your own combination of stats. 
* (The previous one would be strings that users can reference and I would parse them to generate unique strings)

# Installation

**[EXILED](https://github.com/Exiled-Team/EXILED) must be installed for this to work.**

Current plugin version: V2.0.0

## REQUIREMENTS
* Exiled: V6
* SCP:SL Server: V12
=======


Example configuration
```
mvp_utility:
  is_enabled: true
  # Control over to enable or disable debug information
  enable_debug: true
  # Control over what types to show
  round_end_behaviors:
  # Control over what types to show, whether its first come or random per round
    random_outputs: true
    # Whether to show first player escape, only takes two params, Player, Float ( use {0} {1})
    show_first_escape:
      true: >
        <line-height=75%><voffset=30em><align=center><color=#247BA0> {0} </color> was the first person to escape within {1}'s </align> </voffset>
    # Whether to show who killed the most entities, only takes two params, Player, Int ( use {0} {1})
    show_most_kills_killer:
      true: >
        <line-height=75%><voffset=30em><align=center><color=#F6511D> {0} </color> had {2} kills (Any) </align> </voffset> 
    # Whether to show who killed the most humans as SCP on team, only takes three params
    show_most_kills_scp_team:
      true: >
        <line-height=75%><voffset=30em><align=center><color=#F6511D> {0} </color> had {2} kills as {1} (SCP) </align> </voffset> 
    # Whether to show who killed the most humans as MTF on team, only takes three params
    show_most_kills_mtf_team:
      true: >
        <line-height=75%><voffset=30em><align=center><color=#F6511D> {0} </color> had {2} kills as {1} (MTF) </align> </voffset> 
    # Whether to show who killed the most humans as CHAOS on team, only takes three params
    show_most_kills_chaos_team:
      true: >
        <line-height=75%><voffset=30em><align=center><color=#F6511D> {0} </color> had {2} kills as {1} (Chaos) </align> </voffset> 
    # Whether to show who killed the most humans as human, only takes three params
    show_most_kills_human_on_human:
      true: >
        <line-height=75%><voffset=30em><align=center><color=#F6511D> {0} </color> had {2} kills as {1} (Human) </align> </voffset> 
    # Whether to show who killed the least humans as human, only takes three params
    show_least_kills_human:
      true: >
        <line-height=75%><voffset=30em><align=center><color=#F6511D> {0} </color> only had {2} kills, how sad as {1} (Human)</align> </voffset> 
    # Default output if no one escapes (No params)
    no_escape_string:
      false: ''
    # Whether to force constant updates
    force_constant_update: false
    # Hint limit
    hint_limit: 3
  # How often to check for Scp106
  check_interval: 10
  # Whether to track suicides or not.
  track_suicides: false
  # How long to display hint.
  hint_display_limit: 10
 ```

User example: 

![image](https://user-images.githubusercontent.com/24619207/165345465-150fd649-78c2-412b-a4b0-14f3af220d77.png)

![image](https://user-images.githubusercontent.com/24619207/164874580-e924e4fb-a1f5-42f9-abf1-ee61af3f7bdc.png)

