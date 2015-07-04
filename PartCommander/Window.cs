// Window.cs
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
        internal bool popOutWindow = false;
        internal Rect windowRect;
        internal Rect dragRect;
        internal bool resizingWindow = false;
        internal Part currentPart = null;
        internal uint currentPartId;
        internal bool symLock = true;
        internal bool alphaSort = false;
        internal bool search = false;
        internal bool showPartSelector = true;
        internal bool showResources = true;
        internal bool showTemp = false;
        internal bool showAero = false;
        internal int windowId;
        internal bool togglePartSelector = false;
        internal Vector2 oldScrollPos = new Vector2(0f, 0f);
        internal Vector2 scrollPos = new Vector2(0f, 0f);
        internal Part selectPart = null;
        internal Dictionary<int, PCWindow> partWindows;

        public PCWindow(float x, float y, float width, float height, bool popOut)
        {
            windowRect = new Rect(x, y, width, height);
            windowId = GUIUtility.GetControlID(FocusType.Passive);
            partWindows = new Dictionary<int, PCWindow>();
            popOutWindow = popOut;
        }

        public PCWindow(Rect r, bool popOut)
        {
            windowRect = r;
            windowId = GUIUtility.GetControlID(FocusType.Passive);
            partWindows = new Dictionary<int, PCWindow>();
            popOutWindow = popOut;
        }

        public PCWindow(bool popOut)
        {
            windowRect = PCScenario.Instance.gameSettings.windowDefaultRect;
            windowId = GUIUtility.GetControlID(FocusType.Passive);
            partWindows = new Dictionary<int, PCWindow>();
            popOutWindow = popOut;
        }

    }
}
