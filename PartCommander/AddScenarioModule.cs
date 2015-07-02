// AddScenarioModule.cs
// Adds the scenario module on startup

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartCommander
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AddScenarioModule : MonoBehaviour
    {
        void Start()
        {
            var game = HighLogic.CurrentGame;

            var psm = game.scenarios.Find(s => s.moduleName == typeof(PCScenario).Name);
            if (psm == null)
            {
                game.AddProtoScenarioModule(typeof(PCScenario), GameScenes.FLIGHT);
            }
            else
            {
                if (psm.targetScenes.All(s => s != GameScenes.FLIGHT))
                {
                    psm.targetScenes.Add(GameScenes.FLIGHT);
                }
            }
        }
    }
}
