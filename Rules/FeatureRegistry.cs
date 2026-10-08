// Rules/FeatureRegistry.cs — L4 编排：功能注册表与状态机（5.4），UI/补丁/日志共享单一数据源
using System.Collections.Generic;

namespace ForgeTaxCheatMod.Rules
{
    public enum FeatureStatus { Unknown, Implemented, Partial, Unimplemented }

    public sealed class FeatureInfo
    {
        public string Id;
        public string DisplayName;
        public FeatureStatus Status;
        public string Note;

        public FeatureInfo(string id, string displayName, FeatureStatus status, string note = null)
        {
            Id = id;
            DisplayName = displayName;
            Status = status;
            Note = note;
        }
    }

    public static class FeatureRegistry
    {
        public static readonly List<FeatureInfo> Features = new List<FeatureInfo>();

        public static void Init()
        {
            Features.Clear();
            Add("wallet_add",      "金币增减",           FeatureStatus.Implemented);
            Add("sword_credits",   "剑之铭刻增减",       FeatureStatus.Implemented);
            Add("force_win",       "结算强制成功",       FeatureStatus.Implemented, "结算时贡品自动抬至配额3倍");
            Add("never_break",     "剑永不损毁",         FeatureStatus.Implemented, "毁坏结果改写为稳住");
            Add("always_great",    "每次敲击必大成功",   FeatureStatus.Implemented);
            Add("missed_clear",    "清除违约次数",       FeatureStatus.Implemented);
            Add("deadline",        "延长契约期限",       FeatureStatus.Implemented);
            Add("endless",         "无尽契约模式",       FeatureStatus.Implemented, "开启后契约不会因期限结束");
            Add("reroll_reset",    "重置商店刷新次数",   FeatureStatus.Implemented);
            Add("sword_grade",     "目标剑等级调整",     FeatureStatus.Implemented, "目标 = 铁砧 > 手持 > 准星");
            Add("sword_repair",    "修复断剑",           FeatureStatus.Implemented);
            Add("sword_charge",    "狂热充能拉满",       FeatureStatus.Implemented);
            Add("sword_value",     "剑价值提升",         FeatureStatus.Implemented);
            Add("consumables",     "消耗品补充",         FeatureStatus.Implemented, "守护/回溯/粉笔/锻粉/星尘");
            Add("spawn_sword",     "生成旧剑",           FeatureStatus.Implemented, "调试剑，不计图鉴与成就");
            Add("quick_save",      "立即存档",           FeatureStatus.Implemented);
            Add("time_scale",      "游戏速度调节",       FeatureStatus.Implemented);
            Add("relic_edit",      "圣物编辑",           FeatureStatus.Unimplemented, "风险较高，暂缓");
        }

        private static void Add(string id, string name, FeatureStatus status, string note = null)
        {
            Features.Add(new FeatureInfo(id, name, status, note));
        }
    }
}
