// Stores settings for a particular vessel/part window

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartCommander
{
    public class PCWindow
    {
        internal Rect windowRect;
        internal bool resizingWindow = false;
        internal Part currentPart = null;
        internal uint currentPartId;
        internal bool symLock = true;
        internal bool alphaSort = false;
        internal bool showPartSelector = true;
        internal int windowId;
        internal bool togglePartSelector = false;
        internal Vector2 scrollPos = new Vector2(0f, 0f);
        internal Part selectPart = null;
        internal Dictionary<int, PopOutWindow> partWindows;

        public PCWindow(float x, float y, float width, float height)
        {
            windowRect = new Rect(x, y, width, height);
            windowId = GUIUtility.GetControlID(FocusType.Passive);
            partWindows = new Dictionary<int, PopOutWindow>();
        }

        public PCWindow(Rect r)
        {
            windowRect = r;
            windowId = GUIUtility.GetControlID(FocusType.Passive);
            partWindows = new Dictionary<int, PopOutWindow>();
        }

        public PCWindow()
        {
            windowRect = PCScenario.Instance.gameSettings.windowDefaultRect;
            windowId = GUIUtility.GetControlID(FocusType.Passive);
            partWindows = new Dictionary<int, PopOutWindow>();
        }

    }
}
