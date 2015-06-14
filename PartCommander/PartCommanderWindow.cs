// Stores settings for a particular vessel/part window

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartCommander
{
    public class PartCommanderWindow
    {
        public Rect windowRect;
        public Part currentPart = null;
        public uint currentPartId;
        public bool symLock = true;
        public bool alphaSort = false;
        public bool showPartSelector = true;
        public int windowId;

        public PartCommanderWindow(float x, float y, float width, float height)
        {
            windowRect = new Rect(x, y, width, height);
            windowId = GUIUtility.GetControlID(FocusType.Passive);
        }

        public PartCommanderWindow(Rect r)
        {
            windowRect = r;
            windowId = GUIUtility.GetControlID(FocusType.Passive);
        }

        public PartCommanderWindow()
        {
            windowRect = PartCommanderScenario.Instance.gameSettings.windowDefaultRect;
            windowId = GUIUtility.GetControlID(FocusType.Passive);
        }
    }
}
