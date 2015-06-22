// 
//     Part Commander
// 
//     Copyright (C) 2015 Sean McDougall
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HighlightingSystem;

namespace PartCommander
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class PartCommander : MonoBehaviour
    {
        internal ApplicationLauncherButton launcherButton = null;

        private int fontSize = 12;
        private GUISkin skin;
        private GUIStyle resizeButtonStyle;
        private GUIStyle symLockButtonStyle;
        private GUIStyle azButtonStyle;
        private GUIStyle closeButtonStyle;
        private GUIStyle popoutButtonStyle;
        private GUIStyle centeredLabelStyle;

        private List<Part> activeParts = new List<Part>();
        private string partFilter = "";

        private PartCommanderWindow currentWindow;

        private bool visibleUI = true;
        
        private bool controlsLocked = false;
        private string controlsLockID = "PartCommander_LockID";

        private bool popOff = false;

        public static PartCommander Instance { get; private set; }
        public PartCommander()
        {
            Instance = this;
        }

        // ------------------------------- Unity Events --------------------------------
        public void Awake()
        {
            skin = SetupSkin();

            // Hook into events for Application Launcher
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
            GameEvents.onGameSceneLoadRequested.Add(onSceneChange);
        }

        public void Start()
        {
            // Add hooks for showing/hiding on F2
            GameEvents.onShowUI.Add(showUI);
            GameEvents.onHideUI.Add(hideUI);

            // Load Application Launcher
            if (launcherButton == null)
            {
                OnGUIApplicationLauncherReady();
            }
        }

        public void Update()
        {
            // Only proceed if a vessel is active and physics have stablized
            if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.HoldPhysics == false)
            {
                // Check to see if we already have a saved window, if not then create a new one
                if (!PartCommanderScenario.Instance.gameSettings.vesselWindows.ContainsKey(FlightGlobals.ActiveVessel.id))
                {
                    Debug.Log("[PC] creating window for " + FlightGlobals.ActiveVessel.vesselName + " " + FlightGlobals.ActiveVessel.id);
                    PartCommanderScenario.Instance.gameSettings.vesselWindows.Add(FlightGlobals.ActiveVessel.id, new PartCommanderWindow());
                }
                // Load the saved window
                currentWindow = PartCommanderScenario.Instance.gameSettings.vesselWindows[FlightGlobals.ActiveVessel.id];

                // If we don't have a selected part but we do have an id, then resurrect it
                if (currentWindow.currentPart == null && currentWindow.currentPartId != 0u)
                {
                    Debug.Log("[PC] resurrecting current part from flight ID " + currentWindow.currentPartId);
                    foreach (Part p in FlightGlobals.ActiveVessel.Parts)
                    {
                        Debug.Log("[PC] checking " + p.partInfo.title + " " + p.flightID);
                        if (p.flightID == currentWindow.currentPartId)
                        {
                            Debug.Log("[PC] found it!");
                            currentWindow.currentPart = p;
                            break;
                        }
                    }
                    // If we still don't have a part, then the id must be invalid or the part is gone.  Clear it out.
                    if (currentWindow.currentPart == null)
                    {
                        currentWindow.currentPartId = 0u;
                    }
                }

                if (popOff)
                {
                    if (currentWindow.currentPart != null)
                    {
                        PopOffWindow pow = new PopOffWindow(0f, 100f, currentWindow.windowRect.width, currentWindow.windowRect.height);
                        pow.currentPart = currentWindow.currentPart;
                        pow.currentPartId = currentWindow.currentPartId;
                        currentWindow.partWindows.Add(pow.windowId, pow);
                    }
                    popOff = false;
                }

                // The part selector button was clicked in the gui
                if (currentWindow.togglePartSelector)
                {
                    Debug.Log("[PC] part selector toggled");
                    // toggle part selector
                    currentWindow.showPartSelector = !currentWindow.showPartSelector;
                    if (currentWindow.showPartSelector)
                    {
                        // Showing part selector now... clear out any selected part info.
                        if (currentWindow.currentPart != null)
                        {
                            GameEvents.onPartActionUIDismiss.Fire(currentWindow.currentPart);
                        }
                        currentWindow.currentPart = null;
                        currentWindow.currentPartId = 0u;
                    }
                    else
                    {
                        // Show now have a selected part, but make sure it's not null and turn the part selector back on if it is.
                        if (currentWindow.currentPart == null)
                        {
                            currentWindow.showPartSelector = true;
                        }
                    }
                    currentWindow.togglePartSelector = false;
                }

                // Make sure the selected part still exists and is part of the active vessel, otherwise clear it out and reenable the part selector.
                if (currentWindow.currentPart == null)
                {
                    currentWindow.showPartSelector = true;
                    currentWindow.currentPartId = 0u;
                }
                else
                {
                    if (currentWindow.currentPart.vessel != FlightGlobals.ActiveVessel)
                    {
                        Debug.Log("[PC] clearing out active part");
                        currentWindow.currentPart = null;
                        currentWindow.currentPartId = 0u;
                        currentWindow.showPartSelector = true;
                    }
                }

                resizeWindow();
                windowHover();
                getActiveParts();

                // If there's only one available part on the vessel, select it automatically.
                if (currentWindow.showPartSelector && activeParts.Count == 1 && partFilter == "")
                {
                    currentWindow.selectPart = activeParts.First();
                }

                // A part was selected in the gui
                if (currentWindow.selectPart != null)
                {
                    if (currentWindow.selectPart.vessel == FlightGlobals.ActiveVessel)
                    {
                        GameEvents.onPartActionUICreate.Fire(currentWindow.selectPart);
                        currentWindow.currentPart = currentWindow.selectPart;
                        currentWindow.currentPartId = currentWindow.selectPart.flightID;
                        currentWindow.showPartSelector = false;
                        partFilter = "";
                    }
                    currentWindow.selectPart = null;
                }



            }
        }

        public void OnGUI()
        {
            // Make sure we have something to show

            if (visibleUI)
            {
                if (FlightGlobals.ActiveVessel != null)
                {
                    if (currentWindow != null)
                    {
                        if (PartCommanderScenario.Instance != null)
                        {
                            if (PartCommanderScenario.Instance.gameSettings.visibleWindow)
                            {
                                GUI.skin = skin;
                                currentWindow.windowRect = GUILayout.Window(currentWindow.windowId, currentWindow.windowRect, mainWindow, FlightGlobals.ActiveVessel.vesselName);
                                // Set the default location/size for new windows to be the same as this one
                                PartCommanderScenario.Instance.gameSettings.windowDefaultRect = currentWindow.windowRect;

                                foreach (PopOffWindow pow in currentWindow.partWindows.Values)
                                {
                                    pow.windowRect = GUILayout.Window(pow.windowId, pow.windowRect, partWindow, "");
                                }
                            }
                        }

                    }
                }
            }
        }

        public void onSceneChange(GameScenes scene)
        {
            Debug.Log("[PC] onSceneChange");
            removeLauncherButton();
        }

        protected void OnDestroy()
        {
            Debug.Log("[PC] onDestroy");
            PartCommanderScenario.Instance.gameSettings.visibleWindow = false;
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIApplicationLauncherReady);
            GameEvents.onGameSceneLoadRequested.Remove(onSceneChange);
            removeLauncherButton();

            if (InputLockManager.lockStack.ContainsKey(controlsLockID))
                Debug.Log("[PC] remove control lock");
            InputLockManager.RemoveControlLock(controlsLockID);
        }

        // -------------------------------------- Skin/Textures ------------------------------------------
        private Texture2D GetImage(String path, int width, int height)
        {
            Texture2D img = new Texture2D(width, height, TextureFormat.ARGB32, false);
            img = GameDatabase.Instance.GetTexture(path, false);
            return img;
        }

        private GUIStyle GetToggleButtonStyle(string styleName,int width,int height)
        {
            GUIStyle myStyle = new GUIStyle();
            Texture2D styleOff = GetImage("PartCommander/textures/" + styleName + "_off", width, height);
            Texture2D styleOn = GetImage("PartCommander/textures/" + styleName + "_on", width, height);

            myStyle.name = styleName+"Button";
            myStyle.padding = new RectOffset() { left = 0, right = 0, top = 0, bottom = 0 };
            myStyle.border = new RectOffset() { left = 0, right = 0, top = 0, bottom = 0 };
            myStyle.margin = new RectOffset() { left = 0, right = 0, top = 2, bottom = 2 };
            myStyle.normal.background = styleOff;
            myStyle.onNormal.background = styleOn;
            myStyle.hover.background = styleOn;
            myStyle.active.background = styleOn;
            myStyle.fixedWidth = width;
            myStyle.fixedHeight = height;
            return myStyle;
        }

        private GUISkin SetupSkin()
        {
            Debug.Log("[PC] SetupSkin");
            // Setup skin
            GUISkin skin = GameObject.Instantiate(HighLogic.Skin) as GUISkin;

            skin.button.padding = new RectOffset() { left = 1, right = 1, top = 3, bottom = 2 };
            skin.button.wordWrap = true;
            skin.button.fontSize = fontSize;

            skin.label.padding.top = 0;
            skin.label.fontSize = fontSize;

            centeredLabelStyle = skin.label;
            centeredLabelStyle.alignment = TextAnchor.MiddleCenter;

            skin.verticalScrollbar.fixedWidth = 10f;

            skin.window.onNormal.textColor = skin.window.normal.textColor = XKCDColors.Green_Yellow;
            skin.window.onHover.textColor = skin.window.hover.textColor = XKCDColors.YellowishOrange;
            skin.window.onFocused.textColor = skin.window.focused.textColor = Color.red;
            skin.window.onActive.textColor = skin.window.active.textColor = Color.blue;
            skin.window.padding.left = skin.window.padding.right = skin.window.padding.bottom = 2;
            skin.window.fontSize = (fontSize + 2);

            resizeButtonStyle = GetToggleButtonStyle("resize", 20, 20);
            symLockButtonStyle = GetToggleButtonStyle("symlock", 20, 20);
            azButtonStyle = GetToggleButtonStyle("az", 20, 20);
            closeButtonStyle = GetToggleButtonStyle("close", 10, 10);
            popoutButtonStyle = GetToggleButtonStyle("popout", 20, 20);
            
            return (skin);
        }

        // ------------------------------------------ Application Launcher / UI ---------------------------------------
        private void OnGUIApplicationLauncherReady()
        {
            Debug.Log("[PC] OnGUIApplicationLauncherReady");
            if (launcherButton == null)
            {
                Debug.Log("[PC] AddModApplication");
                launcherButton = ApplicationLauncher.Instance.AddModApplication(showWindow, hideWindow, null, null, null, null, ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW, GetImage("PartCommander/textures/toolbar",38,38));
            }
        }

        public void showUI() // triggered on F2
        {
            Debug.Log("[PC] showUI");
            visibleUI = true;
        }

        public void hideUI() // triggered on F2
        {
            Debug.Log("[PC] hideUI");
            visibleUI = false;
        }

        public void showWindow()  // triggered by toolbar
        {
            Debug.Log("[PC] showWindow");
            PartCommanderScenario.Instance.gameSettings.visibleWindow = true;
        }

        public void hideWindow() // triggered by toolbar
        {
            Debug.Log("[PC] hideWindow");
            PartCommanderScenario.Instance.gameSettings.visibleWindow = false;
        }

        private void resizeWindow()
        {
            if (Input.GetMouseButtonUp(0))
            {
                currentWindow.resizingWindow = false;
            }

            if (currentWindow.resizingWindow)
            {
                Debug.Log("[PC] resizing");
                currentWindow.windowRect.width = Input.mousePosition.x - currentWindow.windowRect.x + 10;
                currentWindow.windowRect.width = currentWindow.windowRect.width < 150 ? 150 : currentWindow.windowRect.width;
                currentWindow.windowRect.height = (Screen.height - Input.mousePosition.y) - currentWindow.windowRect.y + 10;
                currentWindow.windowRect.height = currentWindow.windowRect.height < 200 ? 200 : currentWindow.windowRect.height;
            }

            foreach (PopOffWindow pow in currentWindow.partWindows.Values)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    pow.resizingWindow = false;
                }

                if (pow.resizingWindow)
                {
                    Debug.Log("[PC] resizing part window");
                    pow.windowRect.width = Input.mousePosition.x - pow.windowRect.x + 10;
                    pow.windowRect.width = pow.windowRect.width < 150 ? 150 : pow.windowRect.width;
                    pow.windowRect.height = (Screen.height - Input.mousePosition.y) - pow.windowRect.y + 10;
                    pow.windowRect.height = pow.windowRect.height < 200 ? 200 : pow.windowRect.height;
                }
            }
        }

        private void windowHover()
        {
            // Lock camera controls when over window
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;

            if (controlsLocked)
            {
                if (visibleUI && PartCommanderScenario.Instance.gameSettings.visibleWindow && currentWindow.windowRect.Contains(mousePos))
                {
                    if (currentWindow.showPartSelector == false && currentWindow.currentPart != null)
                    {
                        setHighlighting(currentWindow.currentPart, currentWindow.symLock, true);
                    }
                }
                else
                {
                    Debug.Log("[PC] remove control lock");
                    InputLockManager.RemoveControlLock(controlsLockID);
                    controlsLocked = false;
                    clearHighlighting(activeParts,currentWindow.symLock);
                }
            }
            else
            {
                if (visibleUI && PartCommanderScenario.Instance.gameSettings.visibleWindow && currentWindow.windowRect.Contains(mousePos))
                {
                    Debug.Log("[PC] set control lock");
                    InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, controlsLockID);
                    controlsLocked = true;

                    if (currentWindow.showPartSelector == false && currentWindow.currentPart != null)
                    {
                        setHighlighting(currentWindow.currentPart, currentWindow.symLock, true);
                    }
                }
            }
        }

        public void removeLauncherButton()
        {
            Debug.Log("[PC] removeLauncherButton");
            if (launcherButton != null)
            {
                Debug.Log("[PC] removeModApplication");
                ApplicationLauncher.Instance.RemoveModApplication(launcherButton);
            }
        }



        // ----------------------------------- Main Window Logic ---------------------------
        public void mainWindow(int id)
        {
            int optionsCount = 0;

            GUILayout.BeginVertical();

            // Part selector button
            if (currentWindow.currentPart != null)
            {
                string partSelectorLabel = (currentWindow.symLock && currentWindow.currentPart.symmetryCounterparts.Count() > 0) ? currentWindow.currentPart.partInfo.title + " (x" + (currentWindow.currentPart.symmetryCounterparts.Count() + 1) + ")" : currentWindow.currentPart.partInfo.title;
                if (GUILayout.Button(partSelectorLabel))
                {
                    currentWindow.togglePartSelector = true;
                }    
            }
            
            // Main area
            currentWindow.scrollPos = GUILayout.BeginScrollView(currentWindow.scrollPos);

            if (currentWindow.showPartSelector)
            {
                optionsCount = showParts();
            }
            else
            {
                optionsCount = showOptions(currentWindow.currentPart, currentWindow.symLock);
            }

            if (optionsCount == 0)
            {
                GUILayout.Label("Nothing to display.");
            }

            GUILayout.EndScrollView();
            
            GUILayout.Space(5f);
            if (currentWindow.currentPart == null)
            {
                GUILayout.BeginHorizontal();
                partFilter = GUILayout.TextField(partFilter);
                GUILayout.EndHorizontal();
                GUILayout.Space(5f);
            }
            
            GUILayout.BeginHorizontal();
            showSettings(currentWindow);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            // Create resize button in bottom right corner
            if (GUI.RepeatButton(new Rect(currentWindow.windowRect.width - 23, currentWindow.windowRect.height - 23, 20, 20), "", resizeButtonStyle))
            {
                currentWindow.resizingWindow = true;
            }

            // Make window draggable by title
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        public void partWindow(int id)
        {
            PopOffWindow currentPOW = currentWindow.partWindows[id];
            if (currentPOW.currentPart == null)
            {
                currentWindow.partWindows.Remove(id);
                return;
            }

            string partWindowTitle = (currentPOW.symLock && currentPOW.currentPart.symmetryCounterparts.Count() > 0) ? currentPOW.currentPart.partInfo.title + " (x" + (currentPOW.currentPart.symmetryCounterparts.Count() + 1) + ")" : currentPOW.currentPart.partInfo.title;

            int optionsCount = 0;

            GUILayout.BeginVertical();
            GUILayout.Label(partWindowTitle,centeredLabelStyle);

            // Main area
            currentPOW.scrollPos = GUILayout.BeginScrollView(currentPOW.scrollPos);

            optionsCount = showOptions(currentPOW.currentPart, currentPOW.symLock);

            if (optionsCount == 0)
            {
                GUILayout.Label("Nothing to display.");
            }

            GUILayout.EndScrollView();

            GUILayout.Space(5f);

            GUILayout.BeginHorizontal();
            showPartSettings(currentPOW);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            // Create resize button in bottom right corner
            if (GUI.RepeatButton(new Rect(currentPOW.windowRect.width - 23, currentPOW.windowRect.height - 23, 20, 20), "", resizeButtonStyle))
            {
                currentPOW.resizingWindow = true;
            }

            // Create close button in upper right corner
            if (GUI.Button(new Rect(currentPOW.windowRect.width - 13, 3f, 10f, 10f), "", closeButtonStyle))
            {
                currentWindow.partWindows.Remove(currentPOW.windowId);
            }

            // Make window draggable by title
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        // ----------------------------------- Part Selector -------------------------------
        private void getActiveParts()
        {
            // Build list of active parts
            activeParts.Clear();
            List<Part> hiddenParts = new List<Part>();
            foreach (Part p in FlightGlobals.ActiveVessel.Parts)
            {
                bool includePart = false;
                if (!hiddenParts.Contains(p))
                {
                    // Hide other members of the symmetry
                    if (currentWindow.symLock)
                    {
                        foreach (Part symPart in p.symmetryCounterparts)
                        {
                            hiddenParts.Add(symPart);
                        }
                    }
                    foreach (PartModule pm in p.Modules)
                    {
                        if (includePart)
                        {
                            // Part was already included, so break out
                            break;
                        }
                        else
                        {
                            if (pm.Fields != null || pm.Events != null)
                            {
                                foreach (BaseField f in pm.Fields)
                                {
                                    if (f.guiActive && f.guiName != "")
                                    {
                                        includePart = true;
                                        break;
                                    }
                                }
                                if (!includePart)
                                {
                                    foreach (BaseEvent e in pm.Events)
                                    {
                                        if (e.guiActive && e.active)
                                        {
                                            includePart = true;
                                            break;
                                        }
                                    }
                                }
                                if (includePart)
                                {
                                    activeParts.Add(p);
                                }
                            }
                        }
                    }
                }
            }
            if (partFilter != "")
            {
                activeParts = activeParts.FindAll(partMatch);
            }
            if (currentWindow.alphaSort)
            {
                activeParts = activeParts.OrderBy(o => o.partInfo.title).ToList();
            }
        }

        private bool partMatch(Part p)
        {
            if (p.partInfo.title.Contains(partFilter, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int showParts()
        {
            GUILayout.Space(10f);

            foreach (Part p in activeParts)
            {
                string partTitle = (currentWindow.symLock && p.symmetryCounterparts.Count() > 0) ? p.partInfo.title + " (x" + (p.symmetryCounterparts.Count() + 1) + ")" : p.partInfo.title;
                if (GUILayout.Button(partTitle))
                {
                    currentWindow.selectPart = p;
                }
                if (Event.current.type == EventType.Repaint)
                {
                    if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        setHighlighting(p, currentWindow.symLock, true);
                    }
                    else
                    {
                        if (controlsLocked)
                        {
                            setHighlighting(p, currentWindow.symLock, false);
                        }
                    }
                }
            }

            return activeParts.Count();
        }

        // ----------------------------------- Selected Part Logic -------------------------

        private int showOptions(Part p, bool symLock)
        {
            int optionsCount = 0;
            string multiEngineMode = getEngineMode(p);
            optionsCount += showFields(p, symLock, multiEngineMode);
            optionsCount += showEvents(p, symLock, multiEngineMode);
            optionsCount += showResources(p);
            showTemperatureInfo(p);
            return (optionsCount);
        }

        // Routines for displaying/setting KSPFields
        private int showFields(Part p, bool symLock, string multiEngineMode)
        {
            int fieldCount = 0;
            foreach (PartModule pm in p.Modules)
            {
                if (pm.Fields != null)
                {
                    if (checkEngineMode(multiEngineMode, pm))
                    {
                        foreach (BaseField f in pm.Fields)
                        {
                            if (f.guiActive && f.guiName != "")
                            {
                                fieldCount++;

                                if (f.uiControlFlight.GetType().ToString() == "UI_Toggle")
                                {
                                    showToggleField(p, symLock, pm, f, multiEngineMode);
                                }
                                else if (f.uiControlFlight.GetType().ToString() == "UI_FloatRange")
                                {
                                    showSliderField(p, symLock, pm, f, multiEngineMode);

                                }
                                else
                                {
                                    GUILayout.Label(f.GuiString(f.host));
                                }
                            }
                        }
                    }
                }
            }
            return fieldCount;
        }

        private void showSliderField(Part p, bool symLock, PartModule pm, BaseField f, string multiEngineMode)
        {
            UI_FloatRange fr = (UI_FloatRange)f.uiControlFlight;
            GUILayout.Label(f.GuiString(f.host));
            float curVal = (float)f.GetValue(f.host);
            curVal = Mathf.Clamp(curVal, fr.minValue, fr.maxValue);
            curVal = GUILayout.HorizontalSlider(curVal, fr.minValue, fr.maxValue);
            GUILayout.Space(10f);
            curVal = Mathf.CeilToInt(curVal / fr.stepIncrement) * fr.stepIncrement;
            setPartModuleFieldValue(p, symLock, pm, f, multiEngineMode, curVal);
        }

        private void showToggleField(Part p, bool symLock, PartModule pm, BaseField f, string multiEngineMode)
        {
            UI_Toggle t = (UI_Toggle)f.uiControlFlight;
            bool curVal = (bool)f.GetValue(f.host);
            string curText = curVal ? t.enabledText : t.disabledText;

            if (GUILayout.Button(f.guiName + ": " + curText))
            {
                curVal = !curVal;
                setPartModuleFieldValue(p, symLock, pm, f, multiEngineMode, curVal);
            }

        }

        private void setPartModuleFieldValue<T>(Part p, bool symLock, PartModule pm, BaseField f, string multiEngineMode, T curVal)
        {
            f.SetValue(curVal, f.host);
            if (symLock)
            {
                foreach (Part symPart in p.symmetryCounterparts)
                {
                    foreach (PartModule symPM in symPart.Modules)
                    {
                        if (symPM.GetType() == pm.GetType())
                        {
                            if (checkEngineMode(multiEngineMode, symPM))
                            {
                                foreach (BaseField symF in symPM.Fields)
                                {
                                    if (symF.guiActive && f.name == symF.name)
                                    {
                                        symF.SetValue(curVal, symF.host);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Routines for displaying KSPEvents
        private int showEvents(Part p, bool symLock, string multiEngineMode)
        {
            int eventCount = 0;
            foreach (PartModule pm in p.Modules)
            {
                if (pm.Events != null)
                {
                    if (checkEngineMode(multiEngineMode, pm))
                    {
                        foreach (BaseEvent e in pm.Events)
                        {
                            if (e.active && e.guiActive)
                            {
                                eventCount++;
                                showEvent(p, symLock, pm, e, multiEngineMode);
                            }
                        }
                    }
                }
            }
            return eventCount;
        }

        private void showEvent(Part p, bool symLock, PartModule pm, BaseEvent e, string multiEngineMode)
        {
            if (GUILayout.Button(e.guiName))
            {
                e.Invoke();
                if (symLock)
                {
                    foreach (Part symPart in p.symmetryCounterparts)
                    {
                        foreach (PartModule symPM in symPart.Modules)
                        {
                            if (symPM.GetType() == pm.GetType())
                            {
                                if (checkEngineMode(multiEngineMode, symPM))
                                {
                                    foreach (BaseEvent symE in symPM.Events)
                                    {
                                        if (symE.active && symE.guiActive && e.id == symE.id)
                                        {
                                            symE.Invoke();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Display Resources
        private int showResources(Part p)
        {
            int resourceCount = 0;
            foreach (PartResource pr in p.Resources)
            {
                if (pr.isActiveAndEnabled)
                {
                    GUILayout.Label(pr.resourceName + ": " + string.Format("{0:N2}", Math.Round(pr.amount, 2)) + " / " + string.Format("{0:N2}", pr.maxAmount));
                    resourceCount++;
                }
            }
            return resourceCount;
        }

        // Display Temperature Info
        private void showTemperatureInfo(Part p)
        {
            if (PhysicsGlobals.ThermalDataDisplay)
            {
                GUILayout.Label("Thermal Mass: " + string.Format("{0:N2}", Math.Round(p.thermalMass, 2)));
                GUILayout.Label("Temp: " + string.Format("{0:N2}", Math.Round(p.temperature, 2)) + " / " + string.Format("{0:N2}", Math.Round(p.maxTemp)));
                GUILayout.Label("Temp Ext: " + string.Format("{0:N2}", Math.Round(p.externalTemperature, 2)));
                GUILayout.Label("Cond Flux: " + string.Format("{0:N2}", Math.Round(p.thermalConductionFlux, 2)));
                GUILayout.Label("Conv Flux: " + string.Format("{0:N2}", Math.Round(p.thermalConvectionFlux, 2)));
                GUILayout.Label("Rad Flux: " + string.Format("{0:N2}", Math.Round(p.thermalRadiationFlux, 2)));
                GUILayout.Label("Int Flux: " + string.Format("{0:N2}", Math.Round(p.thermalInternalFlux, 2)));
            }
            else
            {
                GUILayout.Label("Temp: " + string.Format("{0:N2}", Math.Round(p.temperature, 2)) + " / " + string.Format("{0:N2}", Math.Round(p.maxTemp)));
            }
        }

        // Display settings
        private void showSettings(PartCommanderWindow w)
        {
            bool oldSymLock = w.symLock;
            w.symLock = GUILayout.Toggle(w.symLock, "", symLockButtonStyle);
            if (w.symLock != oldSymLock)
            {
                if (w.currentPart != null)
                {
                    // reset part highlighting
                    clearHighlighting(activeParts,w.symLock);
                    setHighlighting(w.currentPart, w.symLock, true);
                }
            }

            GUILayout.Space(5f);

            // Alpha sort button
            if (w.currentPart == null)
            {
                w.alphaSort = GUILayout.Toggle(w.alphaSort, "", azButtonStyle);
            }

            // Part pop-off
            if (w.currentPart != null)
            {
                if (GUILayout.Button("", popoutButtonStyle))
                {
                    w.togglePartSelector = true;
                    popOff = true;
                }
            }
            
        }

        private void showPartSettings(PopOffWindow pow)
        {
            bool oldSymLock = pow.symLock;
            pow.symLock = GUILayout.Toggle(pow.symLock, "", symLockButtonStyle);
            if (pow.symLock != oldSymLock)
            {
                if (pow.currentPart != null)
                {
                    // reset part highlighting
                    clearHighlighting(activeParts, pow.symLock);
                    setHighlighting(pow.currentPart, pow.symLock, true);
                }
            }

            GUILayout.Space(5f);
        }

        // ----------------------------------- Part Highlighting -----------------------------------

        private void setHighlighting(Part p, bool symLock, bool highlight)
        {
            p.SetHighlight(highlight, false);
            if (symLock)
            {
                foreach (Part symPart in p.symmetryCounterparts)
                {
                    symPart.SetHighlight(highlight, false);
                }
            }
        }

        private void clearHighlighting(List<Part> ap, bool symLock)
        {
            foreach (Part p in ap)
            {
                setHighlighting(p, symLock, false);
            }
        }

        // ----------------------------------- Multi-Engine Mode -----------------------------------

        private string getEngineMode(Part p)
        {
            string multiEngineMode = null;
            MultiModeEngine mme = p.GetComponent<MultiModeEngine>();
            if (mme != null)
            {
                multiEngineMode = mme.mode;
            }
            return multiEngineMode;
        }

        private static bool checkEngineMode(string multiEngineMode, PartModule pm)
        {
            bool modeMatches = true;
            ModuleEnginesFX mefx = null;
            if (pm.GetType().ToString() == "ModuleEnginesFX")
            {
                mefx = (ModuleEnginesFX)pm;
                modeMatches = (multiEngineMode == mefx.engineID) ? true : false;
            }
            return modeMatches;
        }

    }
}