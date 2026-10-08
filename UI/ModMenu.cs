// UI/ModMenu.cs — L6 主窗口：四域 Tab（经济/锻造/回合/系统）、暗色切换、尺寸滑块、状态熔断
using System;
using UnityEngine;
using ForgeTaxCheatMod.Core;
using ForgeTaxCheatMod.Data;
using ForgeTaxCheatMod.Features;

namespace ForgeTaxCheatMod.UI
{
    public static class ModMenu
    {
        private static bool _show;
        private static Vector2 _scroll;
        private static int _tab;
        private static float _timeScale = 1f;
        private static float _fuseUntil;
        private static readonly string[] TabNames = { "经济", "锻造", "回合", "系统" };
        private static readonly Theme.AccentColor[] TabColors =
            { Theme.AccentColor.Red, Theme.AccentColor.Yellow, Theme.AccentColor.Green, Theme.AccentColor.Blue };

        public static bool Visible { get { return _show; } }
        public static void Toggle() { _show = !_show; }

        public static void Render()
        {
            if (!_show) return;
            if (Time.realtimeSinceStartup < _fuseUntil) return;   // 熔断：异常后 3 秒内跳过渲染
            try
            {
                Theme.ApplySkin();
                float w = 760 * Theme.Scale, h = 560 * Theme.Scale;
                var win = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

                // 背景压暗遮罩：突出面板层次，并吞掉窗口外的点击防止穿透到游戏
                var prevColor = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.6f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = prevColor;
                if (Event.current.type == EventType.MouseDown && !win.Contains(Event.current.mousePosition))
                {
                    Event.current.Use();
                }

                GUI.Window(20260415, win, Win, "ForgeTax 修改器 · Mizuof");
            }
            catch
            {
                _fuseUntil = Time.realtimeSinceStartup + 3f;
                try { DrainLayoutStack(); } catch { }
            }
        }

        private static void Win(int id)
        {
            // 顶部工具栏：暗色切换 + 尺寸滑块
            using (new GUILayout.HorizontalScope())
            {
                bool dark = Theme.CurrentBase == Theme.BaseTheme.Dark;
                bool nd = Components.ToggleSwitch(dark, "暗色");
                if (nd != dark) Theme.SetBaseTheme(nd ? Theme.BaseTheme.Dark : Theme.BaseTheme.Light);
                GUILayout.Label("", GUILayout.ExpandWidth(true));
                GUILayout.Label("尺寸", GUILayout.Width(34));
                float ns = GUILayout.HorizontalSlider(Theme.Scale, 0.7f, 1.4f, GUILayout.Width(90));
                if (Math.Abs(ns - Theme.Scale) > 0.001f) Theme.Scale = ns;
            }

            // 四域 Tab：激活态以强调色着色
            using (new GUILayout.HorizontalScope())
            {
                for (int i = 0; i < TabNames.Length; i++)
                {
                    bool active = _tab == i;
                    var prevBg = GUI.backgroundColor;
                    var prevC = GUI.contentColor;
                    GUI.backgroundColor = active ? TabAccent(i) : Color.clear;
                    GUI.contentColor = active ? Color.white : Color.gray;
                    if (GUILayout.Button(TabNames[i], GUILayout.MinWidth(80 * Theme.Scale)) && !active)
                    {
                        _tab = i;
                        Theme.SetAccent(TabColors[i]);
                    }
                    GUI.backgroundColor = prevBg;
                    GUI.contentColor = prevC;
                }
            }

            if (!GameService.Ready)
            {
                GUILayout.Space(12);
                GUILayout.Label("未检测到游戏运行状态（请进入锻造坊后使用）。");
                Footer();
                return;
            }

            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
            try
            {
                switch (_tab)
                {
                    case 0: RenderEconomy(); break;
                    case 1: RenderForge(); break;
                    case 2: RenderRound(); break;
                    case 3: RenderSystem(); break;
                }
            }
            finally
            {
                GUILayout.EndScrollView();
            }

            Footer();
        }

        private static void Footer()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("F1 关闭");
                GUILayout.Label("", GUILayout.ExpandWidth(true));
            }
            GUI.DragWindow();
        }

        // ---------- 经济 ----------
        private static void RenderEconomy()
        {
            var c = GameService.Campaign;
            Components.SectionTitle("状态");
            Components.InfoLine("回合", c.round + " / 期限 " + c.ContractDeadline);
            Components.InfoLine("钱包", ForgeEnhancementBalance.Money(c.wallet) + " G");
            Components.InfoLine("配额", ForgeEnhancementBalance.Money(c.Quota) + " G");
            Components.InfoLine("剑之铭刻", c.swordCredits.ToString());
            Components.SectionTitle("金币");
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+1,000 G")) Actions.AddWallet(1000);
                if (GUILayout.Button("+10,000 G")) Actions.AddWallet(10000);
                if (GUILayout.Button("+100,000 G")) Actions.AddWallet(100000);
            }
            Components.SectionTitle("剑之铭刻（商店货币）");
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+1")) Actions.AddSwordCredits(1);
                if (GUILayout.Button("+10")) Actions.AddSwordCredits(10);
                if (GUILayout.Button("+100")) Actions.AddSwordCredits(100);
            }
        }

        // ---------- 锻造 ----------
        private static void RenderForge()
        {
            var s = GameService.TargetSword;
            var b = GameService.Balance;
            Components.SectionTitle("目标剑（铁砧 > 手持 > 准星）");
            if (s == null)
            {
                GUILayout.Label("未找到目标剑。");
                return;
            }
            Components.InfoLine("等级", "+" + s.grade + " / 上限 " + ForgeEconomy.GradeCap(b));
            Components.InfoLine("价值", ForgeEnhancementBalance.Money(s.value) + " G");
            Components.InfoLine("状态", s.broken ? "已损毁" : "完好 · 敲击 " + s.strikes + " 次 · 充能 " + s.effects.charge + "/10");
            Components.SectionTitle("操作");
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("等级 +1")) Actions.SwordGradeUp(1);
                if (GUILayout.Button("等级 +5")) Actions.SwordGradeUp(5);
            }
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("修复断剑")) Actions.RepairSword();
                if (GUILayout.Button("狂热充能拉满")) Actions.ChargeFull();
            }
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("价值 +5,000 G")) Actions.BoostValue(5000);
                if (GUILayout.Button("价值 +50,000 G")) Actions.BoostValue(50000);
            }
            Components.SectionTitle("生成");
            if (Components.PrimaryButton("生成旧剑（0 级铁剑 · 投放面前）")) Actions.SpawnOldSword();
        }

        // ---------- 回合 ----------
        private static void RenderRound()
        {
            var c = GameService.Campaign;
            Components.SectionTitle("战斗保护");
            bool nb = Components.ToggleSwitch(Config.NeverBreak, "剑永不损毁（损毁结果→稳住）");
            if (nb != Config.NeverBreak) Config.NeverBreak = nb;
            bool ag = Components.ToggleSwitch(Config.AlwaysGreat, "每次敲击必大成功");
            if (ag != Config.AlwaysGreat) Config.AlwaysGreat = ag;
            bool fw = Components.ToggleSwitch(Config.ForceWin, "结算强制成功（贡品抬至配额3倍）");
            if (fw != Config.ForceWin) Config.ForceWin = fw;

            Components.SectionTitle("回合操作");
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("清除违约 (" + c.missedTributes + "/3)")) Actions.ClearMissedTributes();
                if (GUILayout.Button("重置商店刷新")) Actions.ResetRerolls();
            }
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("契约期限 +10 回合")) Actions.ExtendDeadline(10);
                if (GUILayout.Button("开启无尽模式") && !c.endless) Actions.SetEndless();
            }
            if (c.endless) GUILayout.Label("无尽模式已开启：契约不会因期限结束。");

            Components.SectionTitle("消耗品");
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("守护 +5")) Actions.AddUse("ward", 5);
                if (GUILayout.Button("回溯 +5")) Actions.AddUse("rewind", 5);
            }
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("粉笔 +5")) Actions.AddUse("chalk", 5);
                if (GUILayout.Button("锻粉 +5")) Actions.AddUse("powder", 5);
                if (GUILayout.Button("星尘 +5")) Actions.AddUse("starshard", 5);
            }
            if (GUILayout.Button("全部消耗品 +5")) Actions.AddAllConsumables(5);
        }

        // ---------- 系统 ----------
        private static void RenderSystem()
        {
            Components.SectionTitle("系统");
            if (GUILayout.Button("立即存档")) Actions.QuickSave();
            float ts = Components.Slider(_timeScale, 0.25f, 3f, "游戏速度");
            if (Math.Abs(ts - _timeScale) > 0.001f) { _timeScale = ts; Actions.SetTimeScale(ts); }
            if (GUILayout.Button("恢复 1x 速度")) { _timeScale = 1f; Actions.SetTimeScale(1f); }
            if (GUILayout.Button("刷新游戏界面")) GameService.Refresh();

            Components.SectionTitle("支持");
            if (Components.PrimaryButton("赞助作者 · Mizuof"))
            {
                Application.OpenURL("https://www.mizu7.top/archives/thankyou");
            }
        }

        // 渲染异常后排空 IMGUI 布局栈（6.7），防 Stack empty at EndX
        private static void DrainLayoutStack()
        {
            try { GUILayout.EndScrollView(); } catch { }
        }

        private static Color TabAccent(int i)
        {
            return Theme.AccentNormOf(TabColors[i]);
        }
    }
}
