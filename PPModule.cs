using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;

namespace PropPainter {
    public sealed class PPModule : IUserMod, ILoadingExtension {
        private const string m_modName = @"Prop Painter: Revisited";
        private const string m_modDesc = @"Improved version of Prop Painter with fixes";
        internal const string m_modVersion = @"1.1";
        internal const string m_modFileVersion = m_modVersion + ".*";

        public string Name => m_modName + ' ' + m_modVersion;
        public string Description => m_modDesc;

        public void OnEnabled() {
            HarmonyHelper.DoOnHarmonyReady(PPPatcher.EnablePatch);
            PPManager.Initialize();
        }

        public void OnDisabled() {
            if (HarmonyHelper.IsHarmonyInstalled) PPPatcher.DisablePatch();
        }

        public void OnSettingsUI(UIHelperBase helper) {
            UIPanel root = (helper.AddGroup(m_modName + @" -- Version " + m_modVersion) as UIHelper).self as UIPanel;
            root.width = root.parent.width;
            UILabel desc = root.AddUIComponent<UILabel>();
            desc.width = root.width - 70f;
            desc.autoSize = false;
            desc.autoHeight = true;
            desc.wordWrap = true;
            desc.text = "This is an improved version of Prop Painter.\n\nImprovements include:\n" +
                        "(1) Less memory footprint, as no dictionary or lists are used. One simple array is used to store all 65k props colors.\n\n" +
                        "(2) Loading and saving is faster, as no extra memory is created to save/load color data.\n\n" +
                        "(3) Faster color access compared to old Prop Painter, as all color access is done within a delegate with color array stored in the stack.\n\n" +
                        "(4) Only one method is patched compared to old Prop Painter patching 4 methods (of which 3 were patching MoveIt methods)\n\n" +
                        "(5) Does not create any unnecessary gameobjects, contrary to the old Prop Painter which creates one GameObject to run color data synchronization";
        }

        public void OnCreated(ILoading loading) {
            PPPatcher.AttachMoveItPostProcess();
        }

        public void OnLevelLoaded(LoadMode mode) {
            MoveIt.UIToolOptionPanel.AddMoreButtonCallback += PPManager.AddPropPainterBtn;
        }

        public void OnLevelUnloading() { }

        public void OnReleased() { }
    }
}
