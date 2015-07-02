// Scenario.cs
// Used to store persistent settings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartCommander
{
    public class PCScenario : ScenarioModule
    {

        public static PCScenario Instance { get; private set; }
        public ModSettings gameSettings { get; private set; }

        public PCScenario()
        {
            Instance = this;
            gameSettings = new ModSettings();
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            gameSettings.Load(gameNode);
        }

        public override void OnSave(ConfigNode gameNode)
        {
            gameSettings.Save(gameNode);
            base.OnSave(gameNode);
        }

    }
}

