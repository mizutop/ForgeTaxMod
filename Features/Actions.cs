// Features/Actions.cs — L5 功能动作集：经济 / 回合 / 锻造 / 消耗品 / 存档 / 系统
// 全部动作先判 Ready，异常兜底并记日志（7.2 异常层级）
using System;
using MelonLoader;
using UnityEngine;
using HarmonyLib;
using ForgeTaxCheatMod.Data;

namespace ForgeTaxCheatMod.Features
{
    public static class Actions
    {
        // ---------- 经济域 ----------
        public static bool AddWallet(double amount)
        {
            return Run("加金币", c =>
            {
                c.wallet += amount;
                c.Record.goldEarned += amount;
            }, true);
        }

        public static bool AddSwordCredits(int n)
        {
            return Run("加剑之铭刻", c =>
            {
                c.swordCredits = ForgeEconomy.Increment(c.swordCredits, n);
                c.Record.swordCreditsEarned = ForgeEconomy.Increment(c.Record.swordCreditsEarned, n);
            }, true);
        }

        // ---------- 回合域 ----------
        public static bool ClearMissedTributes()
        {
            return Run("清除违约", c => { c.missedTributes = 0; }, true);
        }

        public static bool ExtendDeadline(int rounds)
        {
            return Run("延长契约", c =>
            {
                c.contractDeadlineRound = Math.Max(c.contractDeadlineRound, c.round + rounds);
            }, true);
        }

        public static bool SetEndless()
        {
            return Run("无尽模式", c => { c.endless = true; }, true);
        }

        public static bool ResetRerolls()
        {
            return Run("重置刷新次数", c => { c.paidRerolls = 0; c.rerolls = 0; }, true);
        }

        // ---------- 锻造域 ----------
        public static bool SwordGradeUp(int n)
        {
            return Run("剑等级+", () =>
            {
                var s = GameService.TargetSword;
                var b = GameService.Balance;
                var c = GameService.Campaign;
                if (s == null || b == null || c == null) return false;
                s.grade = Math.Min(ForgeEconomy.GradeCap(b), s.grade + n);
                s.highestVisualGrade = Math.Max(s.highestVisualGrade, s.grade);
                ForgeEconomy.RefreshValue(s, c, b);
                return true;
            }, true);
        }

        public static bool RepairSword()
        {
            return Run("修复断剑", () =>
            {
                var s = GameService.TargetSword;
                var b = GameService.Balance;
                var c = GameService.Campaign;
                if (s == null || b == null || c == null) return false;
                if (!s.broken) return false;
                s.broken = false;
                s.lastOutcome = ForgeOutcome.Hold;
                s.resultStreak = 0;
                s.effects.charge = Math.Min(10, s.effects.charge);
                ForgeEconomy.RefreshValue(s, c, b);
                s.SetGlow(s.ForgeHeat);
                return true;
            }, true);
        }

        public static bool ChargeFull()
        {
            return Run("狂热充能拉满", () =>
            {
                var s = GameService.TargetSword;
                if (s == null || s.broken) return false;
                s.effects.charge = 10;
                return true;
            }, true);
        }

        public static bool BoostValue(double amount)
        {
            return Run("剑价值提升", () =>
            {
                var s = GameService.TargetSword;
                var b = GameService.Balance;
                var c = GameService.Campaign;
                if (s == null || s.broken || b == null || c == null) return false;
                var e = s.effects;
                double scale = Math.Max(1e-9, e.multiplier * e.greatMultiplier * e.valueScale);
                e.flat += amount / scale;
                ForgeEconomy.RefreshValue(s, c, b);
                return true;
            }, true);
        }

        // ---------- 消耗品域 ----------
        public static bool AddUse(string id, int count)
        {
            return Run("添加消耗品 " + id, c =>
            {
                c.AddUses(id, count);
            }, true);
        }

        public static bool AddAllConsumables(int count)
        {
            return Run("添加全部消耗品", c =>
            {
                foreach (var id in new[] { "ward", "rewind", "chalk", "powder", "starshard" })
                {
                    c.AddUses(id, count);
                }
            }, true);
        }

        // ---------- 生成域 ----------
        // 生成旧剑（0 级未锻造铁剑）到玩家面前；debugSpawned=true 不计图鉴/成就，但随存档保留
        public static bool SpawnOldSword()
        {
            return Run("生成旧剑", () =>
            {
                var r = GameService.Room;
                if (!r || r.swordPrefab == null || r.view == null) return false;
                var cam = r.view.transform;
                var pos = cam.position + cam.forward * 1.2f - cam.up * 0.4f;
                var s = UnityEngine.Object.Instantiate(r.swordPrefab, pos,
                    Quaternion.Euler(0f, cam.eulerAngles.y, 80f));
                s.name = "Cheat / old sword";
                int serial = 1;
                try
                {
                    var m = AccessTools.Method(typeof(GPT6ForgeRoom), "NextSwordSerial");
                    if (m != null) serial = (int)m.Invoke(r, null);
                }
                catch { }
                s.Initialize(serial, r.sound, r.enhancementBalance,
                    UnityEngine.Random.Range(0, 3), UnityEngine.Random.Range(0, 2), false, false, false);
                s.debugSpawned = true;
                s.SetPlace(GPT6Sword.Place.Floor);

                // 关键：拾取检测（FindPickupSword）只遍历房间 private 列表 swords，
                // 游戏原生发剑（IssueSwords/存档恢复/商人购买）均会注册，漏登记则剑无法交互
                try
                {
                    var f = AccessTools.Field(typeof(GPT6ForgeRoom), "swords");
                    (f?.GetValue(r) as System.Collections.Generic.List<GPT6Sword>)?.Add(s);
                }
                catch { }
                return true;
            }, false);
        }

        // ---------- 系统域 ----------
        public static bool QuickSave()
        {
            return Run("立即存档", () =>
            {
                var r = GameService.Room;
                return r && r.SaveRunNow();
            }, false);
        }

        public static void SetTimeScale(float v)
        {
            try
            {
                Time.timeScale = UnityEngine.Mathf.Clamp(v, 0.1f, 4f);
            }
            catch { }
        }

        // ---------- 通用执行器（段级异常 + 日志） ----------
        private static bool Run(string label, Func<bool> action, bool refresh)
        {
            try
            {
                bool ok = action();
                if (ok && refresh) GameService.Refresh();
                return ok;
            }
            catch (Exception e)
            {
                MelonLogger.Error("[" + label + "] " + e.GetType().Name + ": " + e.Message);
                return false;
            }
        }

        private static bool Run(string label, Action<ForgeCampaign> mutate, bool refresh)
        {
            return Run(label, () =>
            {
                var c = GameService.Campaign;
                if (c == null) return false;
                mutate(c);
                return true;
            }, refresh);
        }
    }
}
