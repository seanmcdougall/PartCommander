// Stores general settings for Part Commander
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PartCommander
{
    public class PartCommanderGameSettings
    {
        public ConfigNode SettingsNode { get; private set; }
        public Rect windowDefaultRect;
        public Dictionary<Guid, PartCommanderWindow> vesselWindows = new Dictionary<Guid, PartCommanderWindow>();

        public void Load(ConfigNode node)
        {
            Debug.Log("[PC] Loading settings");
            if (node.HasNode("PartCommanderGameSettings"))
            {
                SettingsNode = node.GetNode("PartCommanderGameSettings");
                float windowDefaultX = SettingsNode.GetValueOrDefault("windowDefaultX", (Screen.width - 270f));
                float windowDefaultY = SettingsNode.GetValueOrDefault("windowDefaultY", (Screen.height / 2 - 200f));
                float windowDefaultWidth = SettingsNode.GetValueOrDefault("windowDefaultWidth", 250f);
                float windowDefaultHeight = SettingsNode.GetValueOrDefault("windowDefaultHeight", 400f);
                windowDefaultRect = new Rect(windowDefaultX, windowDefaultY, windowDefaultWidth, windowDefaultHeight);

                if (SettingsNode.HasNode("Vessels"))
                {
                    ConfigNode vessels = SettingsNode.GetNode("Vessels");
                    foreach (ConfigNode v in vessels.nodes)
                    {
                        Guid g = new Guid(v.name);
                        uint u = v.GetValueOrDefault("currentPartId", 0u);

                        Debug.Log("[PC] looking for " + g);

                        foreach (Vessel myVessel in FlightGlobals.Vessels)
                        {
                            Debug.Log("[PC] checking vessel " + myVessel.vesselName + " " + myVessel.id);
                            if (myVessel.id == g)
                            {
                                Debug.Log("[PC] found it!");
                                vesselWindows[g] = new PartCommanderWindow(v.GetValueOrDefault("windowX", windowDefaultX), v.GetValueOrDefault("windowY", windowDefaultY), v.GetValueOrDefault("windowWidth", windowDefaultWidth), v.GetValueOrDefault("windowHeight", windowDefaultHeight));
                                vesselWindows[g].symLock = v.GetValueOrDefault("symLock", true);
                                vesselWindows[g].showPartSelector = false;
                                vesselWindows[g].currentPartId = u;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                float windowDefaultX = (Screen.width - 270f);
                float windowDefaultY = (Screen.height / 2 - 200f);
                float windowDefaultWidth = 250f;
                float windowDefaultHeight = 400f;
                windowDefaultRect = new Rect(windowDefaultX, windowDefaultY, windowDefaultWidth, windowDefaultHeight);
            }
        }

        public void Save(ConfigNode node)
        {
            Debug.Log("[PC] saving settings");
            if (node.HasNode("PartCommanderGameSettings"))
            {
                Debug.Log("[PC] removing existing node");
                SettingsNode.RemoveNode("PartCommanderGameSettings");
            }
            SettingsNode = node.AddNode("PartCommanderGameSettings");
            SettingsNode.AddValue("windowDefaultX", windowDefaultRect.x);
            SettingsNode.AddValue("windowDefaultY", windowDefaultRect.y);
            SettingsNode.AddValue("windowDefaultWidth", windowDefaultRect.width);
            SettingsNode.AddValue("windowDefaultHeight", windowDefaultRect.height);
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
            }
        }
    }
}