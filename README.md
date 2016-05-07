# Part Commander
Plugin for Kerbal Space Program (KSP).  Access the right-click action menus from all parts on the current vessel in a single consolidated interface.

Copyright 2016, Sean McDougall

GitHub: https://github.com/seanmcdougall/PartCommander

INSTALLATION

Copy the "PartCommander" folder and all its contents into your KSP/GameData folder.

CHANGELOG

1.1.1 - 2016/05/06
- recompiled for KSP 1.1.2

1.1 - 2016/04/27
- fixed and recompiled for KSP 1.1

1.0.3.0 - 2015/11/20
- recompiled for KSP 1.0.5
- fix a bug where hiding window with keyboard shortcut failed to release control lock

1.0.2.4 - 2015/07/31
- fixes a NRE that was showing up in the logs

1.0.2.3 - 2015/07/18
- bug fixes to hover logic and part highlighting

1.0.2.2 - 2015/07/09
- another hotfix to correct an issue with settings not being saved properly to the persistent config file

1.0.2.1 - 2015/07/08
- hotfix to correct an issue where the part list wouldn't always update when parts were destroyed

1.0.2 - 2015/07/08
- added persistent PartCommander.cfg settings file which gets created under GameData/PartCommander
- created new Settings window to manage this file
- added optional support for blizzy78's Toolbar (http://forum.kerbalspaceprogram.com/threads/60863)
- stock toolbar button can be disabled through the new Settings window
- added hot key support for showing/hiding windows instead of using toolbar buttons.  Default key combo is Mod + P.  
  This can be changed through the Settings window (but be careful not to pick something that KSP already uses).
- you can also disable the hot key altogether through Settings
- added a setting to hide "unactionable" parts... those that just have display fields but no buttons or sliders.
- made the tooltips more visible by setting a solid background colour
- fixed some control locking issues when mousing over a window

1.0.1 - 2015/07/04
- added .version file
- fixed a bug that prevented the parts list from updating properly when using the filtering/sorting options
- made the search field a toggleable option
- added a toggleable category filter
- added tooltips
- made the scrollbar position persistent when moving back and forth between the main listing and a part

1.0.0 - 2015/07/02
- more bug fixes
- first official release

ACKNOWLEDGEMENTS
- Icons made by Freepik (http://www.flaticon.com/authors/freepik) from www.flaticon.com are licensed by CC BY 3.0 (http://creativecommons.org/licenses/by/3.0/)
- TriggerAU for his KSPPluginFramework (https://github.com/TriggerAu/KSPPluginFramework)
