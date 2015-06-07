// Stores general settings for Part Commander
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PartCommander
{
    public class PartCommanderGameSettings
    {
        private float windowDefaultX = (Screen.width - 270f);
        private float windowDefaultY = (Screen.height / 2 - 200f);
        private float windowDefaultWidth = 250f;
        private float windowDefaultHeight = 400f;

        public ConfigNode SettingsNode { get; private set; }
        public Rect windowDefaultRect;
        public Dictionary<Guid, PartCommanderWindow> vesselWindows = new Dictionary<Guid, PartCommanderWindow>();
        
        public bool visibleWindow = false;

        public void Load(ConfigNode node)
        {
            if (node.HasNode("PartCommanderGameSettings"))
            {
                Debug.Log("[PC] Loading settings");
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

                        Debug.Log("[PC] looking for " + vesselId);

                        foreach (Vessel v in FlightGlobals.Vessels)
                        {
                            Debug.Log("[PC] checking vessel " + v.vesselName + " " + v.id);
                            if (v.id == vesselId)
                            {
                                Debug.Log("[PC] found it!");
                                vesselWindows[vesselId] = new PartCommanderWindow(vesselNode.GetValueOrDefault("windowX", windowDefaultX), vesselNode.GetValueOrDefault("windowY", windowDefaultY), vesselNode.GetValueOrDefault("windowWidth", windowDefaultWidth), vesselNode.GetValueOrDefault("windowHeight", windowDefaultHeight));
                                vesselWindows[vesselId].symLock = vesselNode.GetValueOrDefault("symLock", true);
                                vesselWindows[vesselId].showPartSelector = false;
                                vesselWindows[vesselId].currentPartId = partId;
                                break;
                            }
                        }
                    }
                }
                if (PartCommander.Instance.launcherButton != null)
                {
                    if (visibleWindow)
                    {
                        Debug.Log("[PC] turning on toolbar");
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
            Debug.Log("[PC] Saving settings");
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
                if (vesselWindows[g].currentPartId != 0u)
                {
                    ConfigNode n = vesselsNode.AddNode(g.ToString());
                    n.AddValue("windowX", vesselWindows[g].windowRect.x);
                    n.AddValue("windowY", vesselWindows[g].windowRect.y);
                    n.AddValue("windowWidth", vesselWindows[g].windowRect.width);
                    n.AddValue("windowHeight", vesselWindows[g].windowRect.height);
                    n.AddValue("currentPartId", vesselWindows[g].currentPartId);
                    n.AddValue("symLock", vesselWindows[g].symLock);
                }
            }
        }
    }
}