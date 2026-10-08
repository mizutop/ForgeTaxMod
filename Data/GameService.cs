// Data/GameService.cs — L3 数据访问：统一入口获取房间/战役/平衡/目标剑，上层不感知定位细节
using UnityEngine;
using ForgeTaxCheatMod.Core;

namespace ForgeTaxCheatMod.Data
{
    public static class GameService
    {
        private static GPT6ForgeRoom _room;

        public static GPT6ForgeRoom Room
        {
            get
            {
                if (!_room) _room = Object.FindAnyObjectByType<GPT6ForgeRoom>();
                return _room;
            }
        }

        public static ForgeCampaign Campaign
        {
            get
            {
                var r = Room;
                return r ? r.Campaign : null;
            }
        }

        public static ForgeEnhancementBalance Balance
        {
            get
            {
                var r = Room;
                return r ? r.enhancementBalance : null;
            }
        }

        // 目标剑优先级：铁砧上的 → 手持 → 准星指向
        public static GPT6Sword TargetSword
        {
            get
            {
                var r = Room;
                if (!r) return null;
                if (r.OnAnvil) return r.OnAnvil;
                if (r.Held) return r.Held;
                if (r.FocusedSword) return r.FocusedSword;
                return null;
            }
        }

        public static bool Ready
        {
            get
            {
                var c = Campaign;
                var b = Balance;
                return c != null && b != null;
            }
        }

        public static void Refresh()
        {
            var r = Room;
            if (r) r.RefreshDisplays();
        }
    }
}
