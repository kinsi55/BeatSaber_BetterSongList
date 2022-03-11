using BetterSongList.UI;
using HarmonyLib;
using static SelectLevelCategoryViewController;

namespace BetterSongList.HarmonyPatches {
	// Hook and save the last selected tab
	[HarmonyPatch(typeof(SelectLevelCategoryViewController), nameof(SelectLevelCategoryViewController.LevelFilterCategoryIconSegmentedControlDidSelectCell))]
	static class HookSelectedCategory {
		public static LevelCategory lastSelectedCategory { get; private set; } = LevelCategory.None;

		// When switching categories we want to reset the table to the topp
		[HarmonyPriority(int.MinValue)]
		static void Prefix(SelectLevelCategoryViewController __instance) {
#if TRACE
			Plugin.Log.Debug("SelectLevelCategoryViewController.LevelFilterCategoryIconSegmentedControlDidSelectCell():Prefix");
#endif
			if(lastSelectedCategory == __instance.selectedLevelCategory)
				return;

			lastSelectedCategory = __instance.selectedLevelCategory;
			Config.Instance.LastCategory = __instance.selectedLevelCategory.ToString();
#if TRACE
			Plugin.Log.Warn("SelectLevelCategoryViewController.LevelFilterCategoryIconSegmentedControlDidSelectCell():Prefix => ResetScroll()");
#endif
			RestoreTableScroll.ResetScroll();
			FilterUI.persistentNuts?.UpdateTransformerOptionsAndDropdowns();
		}

		//[HarmonyPriority(int.MinValue)]
		//static void Postfix(SelectLevelCategoryViewController __instance) {
		//	lastSelectedCategory = __instance.selectedLevelCategory;

		//	Config.Instance.LastCategory = __instance.selectedLevelCategory.ToString();
		//}
	}
}
