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
        // Public variables
        public GUISkin skin;
        public List<Part> activeParts = new List<Part>();
        public List<Part> hiddenParts = new List<Part>();
        public Part currentPart = null;
        public bool symLock = true;
        public int fontSize = 12;

        // Private variables
        private bool visibleWindow = false;
        private bool visibleUI = true;

        private int windowId;
        private Rect windowRect = new Rect();
        private Vector2 scrollPos = new Vector2(0f, 0f);

        private bool showPartSelector = true;
        private bool resizingWindow = false;
        private bool controlsLocked = false;

        private string controlsLockID = "PartCommander_LockID";

        private GUIStyle resizeButtonStyle;

        internal static String PathPlugin = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).Replace("\\", "/");
        internal static String texturePath = string.Format("{0}/textures", PathPlugin);

        private Texture2D texResizeOn = new Texture2D(20, 20, TextureFormat.ARGB32, false);
        private Texture2D texResizeOff = new Texture2D(20, 20, TextureFormat.ARGB32, false);
        private Texture2D texToolbar = new Texture2D(38, 38, TextureFormat.ARGB32, false);

        private ApplicationLauncherButton launcherButton = null;

        private int windowDefaultX = Screen.width - 270;
        private int windowDefaultY = Screen.height / 2 - 200;
        private float windowDefaultWidth = 250f;
        private float windowDefaultHeight = 400f;


        public void Awake()
        {
            // Load our skin/styles/textures
            skin = SetupSkin();

            // Hook into events for Application Launcher
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
            GameEvents.onGameSceneLoadRequested.Add(onSceneChange);
        }

        public void Start()
        {
            // Setup the window
            windowRect = new Rect(windowDefaultX, windowDefaultY, windowDefaultWidth, windowDefaultHeight);
            windowId = GUIUtility.GetControlID(FocusType.Passive);

            // Add hooks for showing/hiding on F2
            GameEvents.onShowUI.Add(showUI);
            GameEvents.onHideUI.Add(hideUI);

            // Load Application Launcher
            if (launcherButton == null)
            {
                OnGUIApplicationLauncherReady();
            }

        }

        // Creates application launcher button
        private void OnGUIApplicationLauncherReady()
        {
            if (launcherButton == null)
            {
                launcherButton = ApplicationLauncher.Instance.AddModApplication(showWindow, hideWindow, null, null, null, null, ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW, texToolbar);
            }
        }

        // Sets up skin and styles, loads textures
        private GUISkin SetupSkin()
        {
            // Load textures
            loadTexture(ref texResizeOn, "resize_on.png");
            loadTexture(ref texResizeOff, "resize_off.png");
            loadTexture(ref texToolbar, "toolbar.png");

            // Setup skin
            GUISkin skin = GameObject.Instantiate(HighLogic.Skin) as GUISkin;

            skin.button.padding = new RectOffset() { left = 1, right = 1, top = 3, bottom = 2 };
            skin.button.wordWrap = true;
            skin.button.fontSize = fontSize;

            skin.toggle.border.top = skin.toggle.border.bottom = skin.toggle.border.left = skin.toggle.border.right = 0;
            skin.toggle.margin = new RectOffset(5, 0, 0, 0);
            skin.toggle.padding = new RectOffset() { left = 5, top = 3, right = 3, bottom = 3 };
            skin.toggle.fontSize = fontSize;

            skin.horizontalSlider.margin = new RectOffset();

            skin.label.padding.top = 0;
            skin.label.fontSize = fontSize;

            skin.verticalScrollbar.fixedWidth = 10f;

            skin.window.onNormal.textColor = skin.window.normal.textColor = XKCDColors.Green_Yellow;
            skin.window.onHover.textColor = skin.window.hover.textColor = XKCDColors.YellowishOrange;
            skin.window.onFocused.textColor = skin.window.focused.textColor = Color.red;
            skin.window.onActive.textColor = skin.window.active.textColor = Color.blue;
            skin.window.padding.left = skin.window.padding.right = skin.window.padding.bottom = 2;
            skin.window.fontSize = (fontSize + 2);

            resizeButtonStyle = new GUIStyle();
            resizeButtonStyle.name = "resizeButton";
            resizeButtonStyle.padding = new RectOffset() { left = 0, right = 0, top = 0, bottom = 0 };
            resizeButtonStyle.border = new RectOffset() { left = 0, right = 0, top = 0, bottom = 0 };
            resizeButtonStyle.margin = new RectOffset() { left = 0, right = 0, top = 2, bottom = 2 };
            resizeButtonStyle.normal.background = texResizeOff;
            resizeButtonStyle.hover.background = texResizeOn;

            return (skin);
        }

        public void showUI()
        {
            visibleUI = true;
        }

        public void hideUI()
        {
            visibleUI = false;
        }

        public void showWindow()
        {
            visibleWindow = true;
        }

        public void hideWindow()
        {
            visibleWindow = false;
        }

        public void Update()
        {
            handleResize();
            controlLock();
            getActiveParts();

        }

        private void handleResize()
        {
            if (Input.GetMouseButtonUp(0))
            {
                resizingWindow = false;
            }

            if (resizingWindow)
            {
                windowRect.width = Input.mousePosition.x - windowRect.x + 10;
                windowRect.width = windowRect.width < 150 ? 150 : windowRect.width;
                windowRect.height = (Screen.height - Input.mousePosition.y) - windowRect.y + 10;
                windowRect.height = windowRect.height < 200 ? 200 : windowRect.height;
            }
        }

        private void controlLock()
        {
            // Lock camera controls when over window
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;
            if (windowRect.Contains(mousePos) && !controlsLocked)
            {
                //TODO: also need to block camera controls in IVA
                InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, controlsLockID);
                controlsLocked = true;

                if (currentPart != null && showPartSelector == false)
                {
                    setHighlighting(currentPart, true);
                }
            }
            else if (!windowRect.Contains(mousePos) && controlsLocked)
            {
                InputLockManager.RemoveControlLock(controlsLockID);
                controlsLocked = false;
                clearHighlighting();
            }
        }

        private void setHighlighting(Part p, bool highlight)
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


        private void clearHighlighting()
        {
            foreach (Part p in activeParts)
            {
                p.SetHighlight(false, false);
                if (symLock)
                {
                    foreach (Part symPart in p.symmetryCounterparts)
                    {
                        symPart.SetHighlight(false, false);
                    }
                }
            }
        }

        private void getActiveParts()
        {
            // Build list of active parts
            activeParts.Clear();
            hiddenParts.Clear();
            foreach (Part p in FlightGlobals.ActiveVessel.Parts)
            {
                bool includePart = false;
                if (!hiddenParts.Contains(p))
                {
                    // Hide other members of the symmetry
                    if (symLock)
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
                                    if (f.guiActive)
                                    {
                                        // Only include "settable" fields
                                        //if (f.uiControlFlight.GetType().ToString() == "UI_Toggle" || f.uiControlFlight.GetType().ToString() == "UI_FloatRange")
                                        //{

                                        includePart = true;
                                        break;
                                        //}
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
            hiddenParts.Clear(); // don't need this anymore, so clear it out
        }

        public void OnGUI()
        {
            if (visibleWindow && visibleUI)
            {
                GUI.skin = skin;
                windowRect = GUILayout.Window(windowId, windowRect, mainWindow, FlightGlobals.ActiveVessel.vesselName);
            }
        }

        public void mainWindow(int id)
        {
            bool nothing = true;

            GUILayout.BeginVertical();

            showPartSelectorButton();

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            if (showPartSelector)
            {
                nothing = showParts(nothing);
            }
            else
            {
                if (currentPart != null)
                {
                    nothing = showOptions(nothing);
                }
                else
                {
                    GUILayout.Label("Please select a part");
                }
            }

            if (nothing)
            {
                GUILayout.Label("Nothing to display.");
            }

            GUILayout.EndScrollView();
            GUILayout.Space(2f);
            showSettings();

            GUILayout.EndVertical();

            // Create resize button in bottom right corner
            if (GUI.RepeatButton(new Rect(windowRect.width - 23, windowRect.height - 23, 20, 20), "", resizeButtonStyle))
            {
                resizingWindow = true;
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void showSettings()
        {
            symLock = GUILayout.Toggle(symLock, "Lock Symmetry");
        }

        private void showPartSelectorButton()
        {
            string partSelectorLabel = "--Select a Part--";
            if (currentPart != null)
            {
                if (currentPart.vessel.isActiveVessel)
                {
                    partSelectorLabel = currentPart.partInfo.title;
                }
                else
                {
                    GameEvents.onPartActionUIDismiss.Fire(currentPart);
                    currentPart = null;
                    showPartSelector = true;
                }
            }

            if (GUILayout.Button(partSelectorLabel))
            {
                // toggle part selector
                showPartSelector = !showPartSelector;
                if (showPartSelector)
                {
                    if (currentPart != null)
                    {
                        GameEvents.onPartActionUIDismiss.Fire(currentPart);
                    }
                    currentPart = null;
                }
                else
                {
                    if (currentPart == null)
                    {
                        showPartSelector = true;
                    }
                }
            }
        }

        private bool showParts(bool nothing)
        {
            GUILayout.Space(10f);
            if (activeParts.Count() > 0)
            {
                nothing = false;
                foreach (Part p in activeParts)
                {
                    string partTitle = (symLock && p.symmetryCounterparts.Count() > 0) ? p.partInfo.title + " (x" + (p.symmetryCounterparts.Count() + 1) + ")" : p.partInfo.title;
                    if ((GUILayout.Button(partTitle)) || activeParts.Count() == 1)
                    {
                        GameEvents.onPartActionUICreate.Fire(p);
                        currentPart = p;
                        showPartSelector = false;
                    }
                    partButtonHover(p);
                }
            }
            return nothing;
        }

        private void partButtonHover(Part p)
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    setHighlighting(p, true);
                }
                else
                {
                    if (controlsLocked)
                    {
                        setHighlighting(p, false);
                    }
                }
            }
        }

        private bool showOptions(bool nothing)
        {
            string multiEngineMode = getEngineMode(currentPart);
            nothing = showFields(nothing, multiEngineMode);
            nothing = showEvents(nothing, multiEngineMode);
            nothing = showResources(nothing);
            return nothing;
        }

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

        private bool showResources(bool nothing)
        {
            foreach (PartResource pr in currentPart.Resources)
            {
                if (pr.isActiveAndEnabled)
                {
                    GUILayout.Label(pr.resourceName + ": " + string.Format("{0:N2}", Math.Round(pr.amount, 2)) + "/" + string.Format("{0:N2}", pr.maxAmount));
                    nothing = false;
                }
            }
            return nothing;
        }

        private bool showEvents(bool nothing, string multiEngineMode)
        {
            foreach (PartModule pm in currentPart.Modules)
            {
                if (pm.Events != null)
                {
                    if (checkEngineMode(multiEngineMode, pm))
                    {
                        foreach (BaseEvent e in pm.Events)
                        {
                            if (e.active && e.guiActive)
                            {
                                nothing = false;
                                showEvent(multiEngineMode, pm, e);
                            }
                        }
                    }
                }

            }
            return nothing;
        }

        private void showEvent(string multiEngineMode, PartModule pm, BaseEvent e)
        {
            if (GUILayout.Button(e.guiName))
            {
                e.Invoke();
                if (symLock)
                {
                    foreach (Part symPart in currentPart.symmetryCounterparts)
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

        private static bool checkEngineMode(string multiEngineMode, PartModule pm)
        {
            bool doIt = true;
            // Special handling for multi-mode engines (ie R.A.P.I.E.R)
            ModuleEnginesFX mefx = null;
            if (pm.GetType().ToString() == "ModuleEnginesFX")
            {
                mefx = (ModuleEnginesFX)pm;
                doIt = (multiEngineMode == mefx.engineID) ? true : false;
            }
            return doIt;
        }

        private bool showFields(bool nothing, string multiEngineMode)
        {
            foreach (PartModule pm in currentPart.Modules)
            {
                if (pm.Fields != null)
                {
                    if (checkEngineMode(multiEngineMode, pm))
                    {
                        foreach (BaseField f in pm.Fields)
                        {
                            if (f.guiActive)
                            {
                                nothing = false;

                                if (f.uiControlFlight.GetType().ToString() == "UI_Toggle")
                                {
                                    showToggleField(multiEngineMode, pm, f);
                                }
                                else if (f.uiControlFlight.GetType().ToString() == "UI_FloatRange")
                                {
                                    showSliderField(multiEngineMode, pm, f);

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
            return nothing;
        }

        private void showSliderField(string multiEngineMode, PartModule pm, BaseField f)
        {
            UI_FloatRange fr = (UI_FloatRange)f.uiControlFlight;
            GUILayout.Label(f.GuiString(f.host));
            float curVal = (float)f.GetValue(f.host);
            curVal = Mathf.Clamp(curVal, fr.minValue, fr.maxValue);
            curVal = GUILayout.HorizontalSlider(curVal, fr.minValue, fr.maxValue);
            GUILayout.Space(10f);
            curVal = Mathf.CeilToInt(curVal / fr.stepIncrement) * fr.stepIncrement;
            setPartModuleFieldValue(multiEngineMode, symLock, pm, f, curVal);
        }

        private void setPartModuleFieldValue<T>(string multiEngineMode, bool symLock, PartModule pm, BaseField f, T curVal)
        {
            f.SetValue(curVal, f.host);
            if (symLock)
            {
                foreach (Part symPart in currentPart.symmetryCounterparts)
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

        private void showToggleField(string multiEngineMode, PartModule pm, BaseField f)
        {
            UI_Toggle t = (UI_Toggle)f.uiControlFlight;
            bool curVal = (bool)f.GetValue(f.host);
            string curText = curVal ? t.enabledText : t.disabledText;

            if (GUILayout.Button(f.guiName + ": " + curText))
            {
                curVal = !curVal;
                setPartModuleFieldValue(multiEngineMode, symLock, pm, f, curVal);
            }

        }

        protected void OnDestroy()
        {
            visibleWindow = false;
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIApplicationLauncherReady);
            GameEvents.onGameSceneLoadRequested.Remove(onSceneChange);
            removeLauncherButton();

            if (InputLockManager.lockStack.ContainsKey(controlsLockID))
                InputLockManager.RemoveControlLock(controlsLockID);
        }

        public static bool loadTexture(ref Texture2D texture, String fileName, String folder = "")
        {
            bool textureLoaded = false;
            try
            {
                if (folder == "") folder = texturePath;

                if (System.IO.File.Exists(String.Format("{0}/{1}", folder, fileName)))
                {
                    try
                    {
                        texture.LoadImage(System.IO.File.ReadAllBytes(String.Format("{0}/{1}", folder, fileName)));
                        textureLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        print("[CC] error loading texture " + ex.Message);
                    }
                }
                else
                {
                    print("[CC] can't find texture file " + folder + "/" + fileName);
                }


            }
            catch (Exception ex)
            {
                print("[CC] error loading texture " + ex.Message);
            }
            return textureLoaded;
        }

        public void removeLauncherButton()
        {
            if (launcherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(launcherButton);

            }
        }

        public void onSceneChange(GameScenes scene)
        {
            removeLauncherButton();
        }
    }
}