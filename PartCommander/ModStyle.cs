// ModStyle.cs
// Skin and style settings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartCommander
{
    public class ModStyle
    {
        public GUISkin skin;
        public Dictionary<string, GUIStyle> guiStyles;
        public int fontSize = 12;
        public int minWidth = 100;
        public int minHeight = 100;

        public ModStyle()
        {
            guiStyles = new Dictionary<string, GUIStyle>();

            skin = GameObject.Instantiate(HighLogic.Skin) as GUISkin;

            skin.button.padding = new RectOffset() { left = 3, right = 3, top = 3, bottom = 3 };
            skin.button.wordWrap = true;
            skin.button.fontSize = fontSize;

            skin.label.padding.top = 0;
            skin.label.fontSize = fontSize;

            skin.verticalScrollbar.fixedWidth = 10f;

            skin.window.onNormal.textColor = skin.window.normal.textColor = XKCDColors.Green_Yellow;
            skin.window.onHover.textColor = skin.window.hover.textColor = XKCDColors.YellowishOrange;
            skin.window.onFocused.textColor = skin.window.focused.textColor = Color.red;
            skin.window.onActive.textColor = skin.window.active.textColor = Color.blue;
            skin.window.padding.left = skin.window.padding.right = skin.window.padding.bottom = 2;
            skin.window.fontSize = (fontSize + 2);
            skin.window.padding = new RectOffset() { left = 1, top = 5, right = 1, bottom = 1 };

            guiStyles["resizeButton"] = GetToggleButtonStyle("resize", 20, 20, true);
            guiStyles["symLockButton"] = GetToggleButtonStyle("symlock", 20, 20, false);
            guiStyles["azButton"] = GetToggleButtonStyle("az", 20, 20, false);
            guiStyles["closeButton"] = GetToggleButtonStyle("close", 15, 15, true);
            guiStyles["popoutButton"] = GetToggleButtonStyle("popout", 20, 20, true);
            guiStyles["resourcesButton"] = GetToggleButtonStyle("resources", 20, 20, false);
            guiStyles["tempButton"] = GetToggleButtonStyle("temp", 20, 20, false);
            guiStyles["aeroButton"] = GetToggleButtonStyle("aero", 20, 20, false);

            guiStyles["centeredLabel"] = new GUIStyle();
            guiStyles["centeredLabel"].name = "centeredLabel";
            guiStyles["centeredLabel"].fontSize = fontSize + 3;
            guiStyles["centeredLabel"].fontStyle = FontStyle.Bold;
            guiStyles["centeredLabel"].alignment = TextAnchor.MiddleCenter;
            guiStyles["centeredLabel"].wordWrap = true;
            guiStyles["centeredLabel"].normal.textColor = Color.yellow;
            guiStyles["centeredLabel"].padding = new RectOffset() { left = 20, right = 20, top = 0, bottom = 0 };

            guiStyles["tooltip"] = new GUIStyle();
            guiStyles["tooltip"].name = "tooltip";
            guiStyles["tooltip"].fontSize = fontSize+3;
            guiStyles["tooltip"].normal.textColor = Color.yellow;

        }

        public Texture2D GetImage(String path, int width, int height)
        {
            Texture2D img = new Texture2D(width, height, TextureFormat.ARGB32, false);
            img = GameDatabase.Instance.GetTexture(path, false);
            return img;
        }

        public GUIStyle GetToggleButtonStyle(string styleName, int width, int height, bool hover)
        {
            GUIStyle myStyle = new GUIStyle();
            Texture2D styleOff = GetImage("PartCommander/textures/" + styleName + "_off", width, height);
            Texture2D styleOn = GetImage("PartCommander/textures/" + styleName + "_on", width, height);

            myStyle.name = styleName + "Button";
            myStyle.padding = new RectOffset() { left = 0, right = 0, top = 0, bottom = 0 };
            myStyle.border = new RectOffset() { left = 0, right = 0, top = 0, bottom = 0 };
            myStyle.margin = new RectOffset() { left = 0, right = 0, top = 2, bottom = 2 };
            myStyle.normal.background = styleOff;
            myStyle.onNormal.background = styleOn;
            if (hover)
            {
                myStyle.hover.background = styleOn;
            }
            myStyle.active.background = styleOn;
            myStyle.fixedWidth = width;
            myStyle.fixedHeight = height;
            return myStyle;
        }
    }
}
