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
        internal bool resizingWindow;
        internal Rect windowRect;
        internal Rect dragRect;
        internal Vector2 scrollPos = new Vector2(0f, 0f);
        internal int windowId;
        internal ModStyle modStyle;
        internal Settings settings;
        internal bool settingHotKey = false;

        internal SettingsWindow(ModStyle m, Settings s)
        {
            modStyle = m;
            settings = s;
            showWindow = false;
            windowRect = new Rect((Screen.width - 250) / 2, (Screen.height - 300) / 2, 250, 300);
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
            GUI.skin = modStyle.skin;
            GUILayout.BeginVertical();
            GUILayout.Label("Settings", modStyle.guiStyles["titleLabel"]);
            GUILayout.EndVertical();
            if (Event.current.type == EventType.Repaint)
            {
                dragRect = GUILayoutUtility.GetLastRect();
            }
            GUILayout.BeginVertical();
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            bool newHideUnAct = GUILayout.Toggle(settings.hideUnAct, "Hide unactionable parts");
            if (newHideUnAct != settings.hideUnAct)
            {
                PartCommander.Instance.updateParts = true;
                settings.hideUnAct = newHideUnAct;
            }

            GUILayout.Space(5f);

            settings.useStockToolbar = GUILayout.Toggle(settings.useStockToolbar, "Use stock toolbar");

            GUILayout.Space(5f);

            settings.enableHotKey = GUILayout.Toggle(settings.enableHotKey, "Enable hot key");

            GUILayout.Space(5f);
            

            if (settingHotKey)
            {
                GUILayout.Label("Type a new hot key...");
                if (Event.current.isKey)
                {
                    settings.hotKey = Event.current.keyCode;
                    settingHotKey = false;
                }
            }
            else
            {
                if (settings.enableHotKey)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Mod + ");
                    if (GUILayout.Button(new GUIContent(settings.hotKey.ToString(), "Click to set new hot key")))
                    {
                        settingHotKey = true;
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();
            GUILayout.Space(25f);
            GUILayout.EndVertical();
            
            if (GUI.Button(new Rect(windowRect.width - 18, 3f, 15f, 15f), new GUIContent("", "Close"), modStyle.guiStyles["closeButton"]))
            {
                showWindow = false;
            }
            // Create resize button in bottom right corner
            if (GUI.RepeatButton(new Rect(windowRect.width - 23, windowRect.height - 23, 20, 20), "", modStyle.guiStyles["resizeButton"]))
            {
                resizingWindow = true;
            }
            GUI.DragWindow(dragRect);

        }

        internal void resizeWindow()
        {
            if (Input.GetMouseButtonUp(0))
            {
                resizingWindow = false;
            }

            if (resizingWindow)
            {
                windowRect.width = Input.mousePosition.x - windowRect.x + 10;
                windowRect.width = Mathf.Clamp(windowRect.width, modStyle.minWidth, Screen.width);
                windowRect.height = (Screen.height - Input.mousePosition.y) - windowRect.y + 10;
                windowRect.height = Mathf.Clamp(windowRect.height, modStyle.minHeight, Screen.height);
            }
        }
    }
}
