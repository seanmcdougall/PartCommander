// ModSettings.cs
// Handles saving/loading settings

using System;
using System.Collections.Generic;
using UnityEngine;

namespace PartCommander
{
    public class ModSettings
    {
        private float windowDefaultX = (Screen.width - 270f);
        private float windowDefaultY = (Screen.height / 2 - 200f);
        private float windowDefaultWidth = 250f;
        private float windowDefaultHeight = 400f;

        public ConfigNode SettingsNode { get; private set; }
        public Rect windowDefaultRect;
        public Dictionary<Guid, PCWindow> vesselWindows = new Dictionary<Guid, PCWindow>();

        public bool visibleWindow = false;

        public void Load(ConfigNode node)
        {
            if (node.HasNode("PartCommanderGameSettings"))
            {
                SettingsNode = node.GetNode("PartCommanderGameSettings");
                windowDefaultX = SettingsNode.GetValueOrDefault("windowDefaultX", windowDefaultX);
                windowDefaultY = SettingsNode.GetValueOrDefault("windowDefaultY", windowDefaultY);
                windowDefaultWidth = SettingsNode.GetValueOrDefault("windowDefaultWidth", windowDefaultWidth);
                windowDefaultHeight = SettingsNode.GetValueOrDefault("windowDefaultHeight", windowDefaultHeight);
                windowDefaultRect = new Rect(windowDefaultX, windowDefaultY, windowDefaultWidth, windowDefaultHeight);
                visibleWindow = SettingsNode.GetValueOrDefault("visibleWindow", visibleWindow);

                if (SettingsNode.HasNode("Vessels"))
                {
                    foreach (ConfigNode vesselNode in SettingsNode.GetNode("Vessels").nodes)
                    {
                        Guid vesselId = new Guid(vesselNode.name);
                        uint partId = vesselNode.GetValueOrDefault("currentPartId", 0u);

                        foreach (Vessel v in FlightGlobals.Vessels)
                        {
                            if (v.id == vesselId)
                            {
                                vesselWindows[vesselId] = new PCWindow(vesselNode.GetValueOrDefault("windowX", windowDefaultX), vesselNode.GetValueOrDefault("windowY", windowDefaultY), vesselNode.GetValueOrDefault("windowWidth", windowDefaultWidth), vesselNode.GetValueOrDefault("windowHeight", windowDefaultHeight), false);
                                vesselWindows[vesselId].symLock = vesselNode.GetValueOrDefault("symLock", true);
                                vesselWindows[vesselId].showPartSelector = false;
                                vesselWindows[vesselId].showResources = vesselNode.GetValueOrDefault("showResources", true);
                                vesselWindows[vesselId].showTemp = vesselNode.GetValueOrDefault("showTemp", false);
                                vesselWindows[vesselId].showAero = vesselNode.GetValueOrDefault("showAero", false);
                                vesselWindows[vesselId].currentPartId = partId;

                                if (vesselNode.HasNode("PartWindows"))
                                {
                                    foreach (ConfigNode pwNode in vesselNode.GetNode("PartWindows").nodes)
                                    {
                                        int windowId = int.Parse(pwNode.name);
                                        PCWindow pow = new PCWindow(pwNode.GetValueOrDefault("windowX", windowDefaultX), pwNode.GetValueOrDefault("windowY", windowDefaultY), pwNode.GetValueOrDefault("windowWidth", windowDefaultWidth), pwNode.GetValueOrDefault("windowHeight", windowDefaultHeight), true);
                                        pow.windowId = windowId;
                                        pow.currentPartId = pwNode.GetValueOrDefault("currentPartId", 0u);
                                        pow.symLock = pwNode.GetValueOrDefault("symLock", true);
                                        pow.showResources = pwNode.GetValueOrDefault("showResources", true);
                                        pow.showTemp = pwNode.GetValueOrDefault("showTemp", false);
                                        pow.showAero = pwNode.GetValueOrDefault("showAero", false);
                                        vesselWindows[vesselId].partWindows.Add(windowId, pow);
                                    }
                                }

                                break;
                            }
                        }

                    }
                }
                if (PartCommander.Instance.launcherButton != null)
                {
                    if (visibleWindow)
                    {
                        PartCommander.Instance.launcherButton.SetTrue(true);
                    }
                }
            }
            else
            {
                windowDefaultRect = new Rect(windowDefaultX, windowDefaultY, windowDefaultWidth, windowDefaultHeight);
            }
        }

        public void Save(ConfigNode node)
        {
            if (node.HasNode("PartCommanderGameSettings"))
            {
                SettingsNode.RemoveNode("PartCommanderGameSettings");
            }
            SettingsNode = node.AddNode("PartCommanderGameSettings");
            SettingsNode.AddValue("windowDefaultX", windowDefaultRect.x);
            SettingsNode.AddValue("windowDefaultY", windowDefaultRect.y);
            SettingsNode.AddValue("windowDefaultWidth", windowDefaultRect.width);
            SettingsNode.AddValue("windowDefaultHeight", windowDefaultRect.height);
            SettingsNode.AddValue("visibleWindow", visibleWindow);
            ConfigNode vesselsNode = SettingsNode.AddNode("Vessels");
            foreach (Guid g in vesselWindows.Keys)
            {

                ConfigNode n = vesselsNode.AddNode(g.ToString());
                n.AddValue("windowX", vesselWindows[g].windowRect.x);
                n.AddValue("windowY", vesselWindows[g].windowRect.y);
                n.AddValue("windowWidth", vesselWindows[g].windowRect.width);
                n.AddValue("windowHeight", vesselWindows[g].windowRect.height);
                n.AddValue("currentPartId", vesselWindows[g].currentPartId);
                n.AddValue("symLock", vesselWindows[g].symLock);
                n.AddValue("showTemp", vesselWindows[g].showTemp);
                n.AddValue("showAero", vesselWindows[g].showAero);
                ConfigNode partWindowsNode = n.AddNode("PartWindows");
                foreach (PCWindow pow in vesselWindows[g].partWindows.Values)
                {
                    ConfigNode pn = partWindowsNode.AddNode(pow.windowId.ToString());
                    pn.AddValue("windowX", pow.windowRect.x);
                    pn.AddValue("windowY", pow.windowRect.y);
                    pn.AddValue("windowWidth", pow.windowRect.width);
                    pn.AddValue("windowHeight", pow.windowRect.height);
                    pn.AddValue("currentPartId", pow.currentPartId);
                    pn.AddValue("symLock", pow.symLock);
                    pn.AddValue("showResources", pow.showResources);
                    pn.AddValue("showTemp", pow.showTemp);
                    pn.AddValue("showAero", pow.showAero);
                }

            }
        }
    }
}