// Features/Patches.cs — L5 Harmony 补丁（Mono 后端：集中式手动安装，逐补丁 try-catch 降级）
// ① Settle 前缀：结算强制成功（把贡品抬到配额 3 倍，走原生结算全流程，不破坏 UI）
// ② Apply 前缀：剑永不损毁（Destruction → Hold）/ 每击必大成功（raw → GreatSuccess）
using System;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using ForgeTaxCheatMod.Core;

namespace ForgeTaxCheatMod.Features
{
    public static class Patches
    {
        public static bool SettleInstalled { get; private set; }
        public static bool ApplyInstalled { get; private set; }

        public static void Install(HarmonyLib.Harmony harmony)
        {
            Try(harmony, typeof(ForgeCampaign).GetMethod(nameof(ForgeCampaign.Settle)),
                nameof(SettlePrefix), s => SettleInstalled = s, "Settle");
            Try(harmony, typeof(ForgeEconomy).GetMethod(nameof(ForgeEconomy.Apply)),
                nameof(ApplyPrefix), s => ApplyInstalled = s, "Apply");
        }

        private static void Try(HarmonyLib.Harmony harmony, MethodInfo target, string prefixName,
            Action<bool> setFlag, string tag)
        {
            try
            {
                if (target == null) throw new MissingMethodException("补丁目标不存在: " + tag);
                harmony.Patch(target, prefix: new HarmonyMethod(typeof(Patches).GetMethod(prefixName,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)));
                setFlag(true);
                MelonLogger.Msg("[Patch] " + tag + " 安装成功");
            }
            catch (Exception e)
            {
                setFlag(false);
                MelonLogger.Error("[Patch] " + tag + " 安装失败: " + e.Message);
            }
        }

        // 结算强制成功：满足条件时把贡品抬到配额 3 倍（复用游戏原生 won/overpaidTriple 判定）
        private static void SettlePrefix(ref double tribute, ForgeCampaign __instance)
        {
            try
            {
                if (Config.ForceWin && __instance != null && !__instance.settled && !__instance.finished)
                {
                    tribute = Math.Max(tribute, __instance.Quota * 3.0);
                }
            }
            catch { }
        }

        // 打击结果改写： AlwaysGreat 优先于 NeverBreak
        private static void ApplyPrefix(ref ForgeOutcome raw)
        {
            try
            {
                if (Config.AlwaysGreat)
                {
                    raw = ForgeOutcome.GreatSuccess;
                    return;
                }
                if (Config.NeverBreak && raw == ForgeOutcome.Destruction)
                {
                    raw = ForgeOutcome.Hold;
                }
            }
            catch { }
        }
    }
}
