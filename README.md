# MvpUtility

I need to go shopping. However, this is a plugin to show MVP stats at the end of the round. It's barebones but it was what I could accomplish today thus far. Thanks, any bugs should be expected but hopefully none. 

![MvpUtility ISSUES](https://img.shields.io/github/issues/Undid-Iridium/MvpUtility)
![MvpUtility FORKS](https://img.shields.io/github/forks/Undid-Iridium/MvpUtility)
![MvpUtility LICENSE](https://img.shields.io/github/license/Undid-Iridium/MvpUtility)


![MvpUtility LATEST](https://img.shields.io/github/v/release/Undid-Iridium/MvpUtility?include_prereleases&style=flat-square)
![MvpUtility LINES](https://img.shields.io/tokei/lines/github/Undid-Iridium/MvpUtility)
![MvpUtility DOWNLOADS](https://img.shields.io/github/downloads/Undid-Iridium/MvpUtility/total?style=flat-square)



# Installation

**[EXILED](https://github.com/Exiled-Team/EXILED) must be installed for this to work.**

Current plugin version: V1.0.0

## REQUIREMENTS
* Exiled: V5.1.3
* SCP:SL Server: V11.2


Example configuration
```
mvp_utility:
  is_enabled: true
  enable_debug: false
  # Control over what types to show
  round_end_behaviors:
  # Control over what types to show, whether its first come or random per round
    random_outputs: true
    # Whether to show first player escape
    show_first_escape: true
    # Whether to show who killed the most entities
    show_most_kills_killer: true
    # Whether to show who killed the most humans as SCP on team
    show_most_kills_scp_team: true
    # Whether to show who killed the most humans as MTF on team
    show_most_kills_mtf_team: true
    # Whether to show who killed the most humans as CHAOS on team
    show_most_kills_chaos_team: true
    # Whether to show who killed the most humans as human
    show_most_kills_human_on_human: true
    # Whether to show who killed the least humans as human
    show_least_kills_human: false
 ```
 
![paintdotnet_V4jZ6KywZM](https://user-images.githubusercontent.com/24619207/164623340-95b71ddf-d494-4e21-860d-3a010a35264e.png)
