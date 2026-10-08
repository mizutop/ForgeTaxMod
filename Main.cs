// Main.cs — L1 入口：初始化配置/注册表/补丁，快捷键与 OnGUI 渲染
using System;
using MelonLoader;
using UnityEngine;
using ForgeTaxCheatMod.Core;
using ForgeTaxCheatMod.Features;
using ForgeTaxCheatMod.Rules;
using ForgeTaxCheatMod.UI;

[assembly: MelonInfo(typeof(ForgeTaxCheatMod.Main), "ForgeTax Cheat Mod", "1.0.0", "Mizuof")]
[assembly: MelonGame(null, null)]

namespace ForgeTaxCheatMod
{
    public sealed class Main : MelonMod
    {
        private static bool _legacyInputBroken;
        private CursorLockMode _lockBefore;
        private bool _visibleBefore;

        public override void OnInitializeMelon()
        {
            PrintBanner();
            Config.Load();
            FeatureRegistry.Init();
            Patches.Install(new HarmonyLib.Harmony("com.mizuof.forgetax.cheat"));
            LoggerInstance.Msg("ForgeTax Cheat Mod v1.0.0 (作者: Mizuof) 初始化完成，按 F1 打开修改菜单。");
        }

        // 启动横幅：作者水印（Mizuof），样式固定，禁止改动
        private void PrintBanner()
        {
            LoggerInstance.Msg("  ███╗   ███╗██╗███████╗██╗   ██╗ ██████╗ ███████╗");
            LoggerInstance.Msg("  ████╗ ████║██║╚══███╔╝██║   ██║██╔═══██╗██╔════╝");
            LoggerInstance.Msg("  ██╔████╔██║██║  ███╔╝ ██║   ██║██║   ██║█████╗  ");
            LoggerInstance.Msg("  ██║╚██╔╝██║██║ ███╔╝  ██║   ██║██║   ██║██╔══╝  ");
            LoggerInstance.Msg("  ██║ ╚═╝ ██║██║███████╗╚██████╔╝╚██████╔╝██║     ");
            LoggerInstance.Msg("  ╚═╝     ╚═╝╚═╝╚══════╝ ╚═════╝  ╚═════╝ ╚═╝     ");
            LoggerInstance.Msg("  Trainer for Big Ambitions — Author: Mizuof");
        }

        public override void OnUpdate()
        {
            if (TogglePressed())
            {
                ModMenu.Toggle();
                OnMenuToggled(ModMenu.Visible);
            }

            // 游戏会每帧重新锁定光标，菜单打开期间必须逐帧强制解锁
            if (ModMenu.Visible)
            {
                try
                {
                    if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
                    if (!Cursor.visible) Cursor.visible = true;
                }
                catch { }
            }
        }

        public override void OnGUI()
        {
            ModMenu.Render();
        }

        private void OnMenuToggled(bool open)
        {
            try
            {
                if (open)
                {
                    _lockBefore = Cursor.lockState;
                    _visibleBefore = Cursor.visible;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = _lockBefore;
                    Cursor.visible = _visibleBefore;
                }
            }
            catch { }
        }

        // 游戏可能只启用 InputSystem（Unity 6），旧 Input 不可用时降级到新输入系统
        private static bool TogglePressed()
        {
            if (!_legacyInputBroken)
            {
                try
                {
                    return Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Insert);
                }
                catch (InvalidOperationException)
                {
                    _legacyInputBroken = true;
                }
            }
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return false;
            return kb.f1Key.wasPressedThisFrame || kb.insertKey.wasPressedThisFrame;
        }
    }
}
