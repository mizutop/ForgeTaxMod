// UI/Theme.cs — L6 主题单元：四色强调 / 黑白主题 / 尺寸缩放 / GUISkin 动态构建 / 1×1 纯色缓存
// 样式集中于此，渲染处只引用、不内联 new（6.2 规范）
using System.Collections.Generic;
using UnityEngine;

namespace ForgeTaxCheatMod.UI
{
    public static class Theme
    {
        public enum AccentColor { Red, Yellow, Green, Blue }

        private static readonly Dictionary<AccentColor, Color[]> _acc =
            new Dictionary<AccentColor, Color[]>
        {
            { AccentColor.Red,    new[] { Hex("#E53E3E"), Hex("#C53030"), Hex("#9B2C2C"), Hex("#FDE8E8") } },
            { AccentColor.Yellow, new[] { Hex("#D69E2E"), Hex("#B7791F"), Hex("#975A16"), Hex("#FEF3C7") } },
            { AccentColor.Green,  new[] { Hex("#38A169"), Hex("#2F855A"), Hex("#276749"), Hex("#E6FFFA") } },
            { AccentColor.Blue,   new[] { Hex("#3182CE"), Hex("#2B6CB0"), Hex("#2C5282"), Hex("#EBF8FF") } }
        };

        private static Color Hex(string h)
        {
            Color c;
            return ColorUtility.TryParseHtmlString(h, out c) ? c : Color.magenta;
        }

        public static AccentColor CurrentAccent { get; private set; } = AccentColor.Red;
        public static Color AccentNormal { get { return _acc[CurrentAccent][0]; } }
        public static Color AccentHover  { get { return _acc[CurrentAccent][1]; } }
        public static Color AccentActive { get { return _acc[CurrentAccent][2]; } }
        public static Color AccentLight  { get { return _acc[CurrentAccent][3]; } }
        public static Color AccentNormOf(AccentColor c) { return _acc[c][0]; }

        public enum BaseTheme { Light, Dark }
        public static BaseTheme CurrentBase { get; private set; } = BaseTheme.Dark;

        private static float _scale = 1f;
        private static bool _needRebuild = true;
        private static GUISkin _skin;

        public static float Scale
        {
            get { return _scale; }
            set { _scale = Mathf.Clamp(value, 0.7f, 1.4f); _needRebuild = true; }
        }

        public static void SetAccent(AccentColor c) { CurrentAccent = c; _needRebuild = true; }
        public static void SetBaseTheme(BaseTheme t) { CurrentBase = t; _needRebuild = true; }

        public static GUISkin Skin
        {
            get
            {
                if (_needRebuild || _skin == null) { _skin = BuildSkin(); _needRebuild = false; }
                return _skin;
            }
        }

        public static void ApplySkin() { GUI.skin = Skin; }

        private static GUISkin BuildSkin()
        {
            var skin = Object.Instantiate(GUI.skin);
            bool dark = CurrentBase == BaseTheme.Dark;
            Color textP = dark ? Color.white : new Color(0.10f, 0.10f, 0.12f);
            Color surf  = dark ? new Color(0.13f, 0.13f, 0.18f) : new Color(0.98f, 0.985f, 0.99f);
            Color bgFld = dark ? new Color(0.12f, 0.12f, 0.16f) : Color.white;

            skin.button = Style(14, TextAnchor.MiddleCenter, 30, textP);
            skin.button.margin = new RectOffset(4, 4, 3, 3);
            skin.button.normal.background = Solid(surf);
            skin.button.hover.background  = Solid(AccentLight);
            skin.button.active.background = Solid(AccentActive * 0.3f);
            skin.button.normal.textColor  = textP;
            skin.button.hover.textColor   = textP;
            skin.button.active.textColor  = textP;

            skin.toggle   = Style(14, TextAnchor.MiddleLeft, 26, textP);
            skin.textField = Style(14, TextAnchor.MiddleLeft, 34, textP);
            skin.textField.normal.background = Solid(bgFld);
            skin.textField.focused.background = skin.textField.normal.background;
            skin.label = Style(14, TextAnchor.MiddleLeft, 0, textP);
            skin.label.normal.textColor = textP;
            skin.label.hover.textColor  = textP;
            skin.label.active.textColor = textP;

            skin.window = new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(12, 12, 8, 12),
                border = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.UpperCenter,
                fontSize = Mathf.RoundToInt(15 * Scale)
            };
            skin.window.normal.background = Solid(dark ? new Color(0.06f, 0.06f, 0.09f) : new Color(0.95f, 0.95f, 0.96f));
            skin.window.onNormal.background = skin.window.normal.background;
            skin.window.normal.textColor = textP;
            skin.window.onNormal.textColor = textP;

            // 滚动条美化：8px 细轨道 + 强调色圆角感滑块（直角）+ 隐藏上下箭头
            float sbw = Mathf.RoundToInt(8 * Scale);
            Color trackC = dark ? new Color(0.10f, 0.10f, 0.14f) : new Color(0.86f, 0.86f, 0.89f);
            Color thumbC = dark ? new Color(0.32f, 0.32f, 0.40f) : new Color(0.62f, 0.62f, 0.67f);

            var track = skin.verticalScrollbar;
            track.fixedWidth = sbw;
            track.border = new RectOffset(0, 0, 0, 0);
            track.padding = new RectOffset(2, 2, 0, 0);
            track.normal.background = Solid(trackC);
            track.onNormal.background = Solid(trackC);
            track.hover.background = Solid(trackC);
            track.onHover.background = Solid(trackC);
            track.active.background = Solid(trackC);

            var thumb = skin.verticalScrollbarThumb;
            thumb.fixedWidth = sbw;
            thumb.border = new RectOffset(0, 0, 0, 0);
            thumb.normal.background = Solid(thumbC);
            thumb.onNormal.background = Solid(thumbC);
            thumb.hover.background = Solid(AccentNormal);
            thumb.onHover.background = Solid(AccentNormal);
            thumb.active.background = Solid(AccentHover);
            thumb.onActive.background = Solid(AccentHover);

            skin.verticalScrollbarUpButton.fixedWidth = 0f;
            skin.verticalScrollbarUpButton.fixedHeight = 0f;
            skin.verticalScrollbarDownButton.fixedWidth = 0f;
            skin.verticalScrollbarDownButton.fixedHeight = 0f;
            return skin;
        }

        private static GUIStyle Style(int fontSize, TextAnchor align, float fixedH, Color text)
        {
            int s = Mathf.RoundToInt(fontSize * Scale);
            var g = new GUIStyle
            {
                fontSize = s,
                alignment = align,
                // 纯色纹理无需九宫格：border>0 会因 1×1 纹理小于边框和而采样破碎（半透明花屏根因）
                border = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(Mathf.RoundToInt(10 * Scale), Mathf.RoundToInt(10 * Scale),
                                         Mathf.RoundToInt(5 * Scale), Mathf.RoundToInt(5 * Scale))
            };
            if (fixedH > 0) g.fixedHeight = Mathf.RoundToInt(fixedH * Scale);
            g.normal.textColor = text;
            g.hover.textColor = text;
            g.active.textColor = text;
            return g;
        }

        private static readonly Dictionary<Color, Texture2D> _cache = new Dictionary<Color, Texture2D>();

        public static Texture2D Solid(Color c)
        {
            Texture2D t;
            if (_cache.TryGetValue(c, out t) && t) return t;
            t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            t.SetPixel(0, 0, c);
            t.Apply();
            _cache[c] = t;
            return t;
        }
    }
}
