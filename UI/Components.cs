// UI/Components.cs — L6 组件库：配色走 backgroundColor/contentColor（save/restore），无 style 参数重载
using UnityEngine;
using ForgeTaxCheatMod.Core;
using ForgeTaxCheatMod.Rules;

namespace ForgeTaxCheatMod.UI
{
    public static class Components
    {
        public static bool PrimaryButton(string text, params GUILayoutOption[] o)
        {
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = Theme.AccentNormal;
            bool hit = GUILayout.Button(text, o);
            GUI.backgroundColor = prev;
            return hit;
        }

        public static bool GhostButton(string text, params GUILayoutOption[] o)
        {
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.12f);
            bool hit = GUILayout.Button(text, o);
            GUI.backgroundColor = prev;
            return hit;
        }

        public static bool ToggleSwitch(bool value, string label, params GUILayoutOption[] o)
        {
            // 必须显式固定宽高，否则在 HorizontalScope 内被拉伸成整行色条
            var rect = GUILayoutUtility.GetRect(46 * Theme.Scale, 24 * Theme.Scale,
                GUILayout.Width(46 * Theme.Scale), GUILayout.Height(24 * Theme.Scale));
            bool dark = Theme.CurrentBase == Theme.BaseTheme.Dark;
            GUI.DrawTexture(rect, Theme.Solid(value ? Theme.AccentNormal
                : (dark ? new Color(0.23f, 0.23f, 0.29f) : new Color(0.82f, 0.82f, 0.85f))));
            var knob = new Rect(rect.x + (value ? 22 * Theme.Scale : 2 * Theme.Scale),
                                rect.y + 2 * Theme.Scale, 20 * Theme.Scale, 20 * Theme.Scale);
            GUI.DrawTexture(knob, Theme.Solid(Color.white));
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                Event.current.Use();
            }
            if (!string.IsNullOrEmpty(label)) GUILayout.Label(label);
            return value;
        }

        public static float Slider(float value, float min, float max, string label, params GUILayoutOption[] o)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(80));
                float nv = GUILayout.HorizontalSlider(value, min, max, GUILayout.MinWidth(120 * Theme.Scale));
                GUILayout.Label(nv.ToString("F2"), GUILayout.Width(44));
                return nv;
            }
        }

        public static void SectionTitle(string title)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("▌ " + title, GUILayout.Width(220));
                var r = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true), GUILayout.Height(1));
                GUI.DrawTexture(r, Theme.Solid(Theme.AccentNormal));
            }
        }

        public static void InfoLine(string label, string value)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(120));
                GUILayout.Label(value);
            }
        }

        public static void StatusDot(FeatureStatus s)
        {
            var rect = GUILayoutUtility.GetRect(10, 16, GUILayout.Width(10));
            Color c = s == FeatureStatus.Implemented ? new Color(0.2f, 0.8f, 0.35f)
                    : s == FeatureStatus.Partial ? new Color(0.9f, 0.75f, 0.1f)
                    : new Color(0.6f, 0.6f, 0.6f);
            GUI.DrawTexture(rect, Theme.Solid(c));
        }
    }
}
