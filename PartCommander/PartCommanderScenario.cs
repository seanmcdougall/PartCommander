using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartCommander
{
    public class PartCommanderScenario : ScenarioModule
    {

        public static PartCommanderScenario Instance { get; private set; }
        public PartCommanderGameSettings gameSettings { get; private set; }

        public PartCommanderScenario()
        {
            Instance = this;
            gameSettings = new PartCommanderGameSettings();
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

