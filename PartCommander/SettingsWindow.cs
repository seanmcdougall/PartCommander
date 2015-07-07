using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartCommander
{
    internal class SettingsWindow
    {
        internal bool showWindow;
        internal Rect windowRect;
        internal Rect dragRect;
        internal int windowId;
        internal ModStyle modStyle;

        internal SettingsWindow(ModStyle m)
        {
            modStyle = m;
            showWindow = false;
            windowRect = new Rect((Screen.width - 200) / 2, (Screen.height - 400) / 2, 200, 400);
            windowId = GUIUtility.GetControlID(FocusType.Passive);
        }

        internal void draw()
        {
            if (showWindow)
            {
                windowRect = GUILayout.Window(windowId, windowRect, drawWindow, "");
            }
        }

        internal void drawWindow(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Settings", modStyle.guiStyles["titleLabel"]);
            GUILayout.EndVertical();
            if (Event.current.type == EventType.Repaint)
            {
                dragRect = GUILayoutUtility.GetLastRect();
            }
            if (GUI.Button(new Rect(windowRect.width - 18, 3f, 15f, 15f), new GUIContent("", "Close"), modStyle.guiStyles["closeButton"]))
            {
                showWindow = false;
            }
            GUI.DragWindow(dragRect);
        }
    }
}
