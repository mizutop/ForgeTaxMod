// Core/Config.cs — L1 基建：MelonPreferences 配置键集中定义（7.5，禁止 magic string）
using MelonLoader;

namespace ForgeTaxCheatMod.Core
{
    public static class Config
    {
        private static MelonPreferences_Entry<bool> _forceWin;
        private static MelonPreferences_Entry<bool> _neverBreak;
        private static MelonPreferences_Entry<bool> _alwaysGreat;

        public static void Load()
        {
            var cat = MelonPreferences.CreateCategory("ForgeTaxCheat", "ForgeTax 修改器");
            _forceWin = cat.CreateEntry("ForceWin", false, "结算强制成功");
            _neverBreak = cat.CreateEntry("NeverBreak", false, "剑永不损毁");
            _alwaysGreat = cat.CreateEntry("AlwaysGreat", false, "每次敲击必大成功");
        }

        public static bool ForceWin
        {
            get { return _forceWin != null && _forceWin.Value; }
            set { if (_forceWin != null) _forceWin.Value = value; }
        }

        public static bool NeverBreak
        {
            get { return _neverBreak != null && _neverBreak.Value; }
            set { if (_neverBreak != null) _neverBreak.Value = value; }
        }

        public static bool AlwaysGreat
        {
            get { return _alwaysGreat != null && _alwaysGreat.Value; }
            set { if (_alwaysGreat != null) _alwaysGreat.Value = value; }
        }
    }
}
