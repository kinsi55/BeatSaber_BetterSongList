using HarmonyLib;

namespace BetterSongList.HarmonyPatches {
	// Hook and save the last selected tab
	[HarmonyPatch(typeof(SelectLevelCategoryViewController), nameof(SelectLevelCategoryViewController.LevelFilterCategoryIconSegmentedControlDidSelectCell))]
	static class HookSelectedCategory {
		// When switching categories we want to reset the table to the topp
		static void Prefix(SelectLevelCategoryViewController __instance) {
#if TRACE
			Plugin.Log.Debug("SelectLevelCategoryViewController.LevelFilterCategoryIconSegmentedControlDidSelectCell():Prefix");
#endif
			if(Config.Instance.LastCategory == __instance.selectedLevelCategory.ToString())
				return;
#if TRACE
			Plugin.Log.Warn("SelectLevelCategoryViewController.LevelFilterCategoryIconSegmentedControlDidSelectCell():Prefix => ResetScroll()");
#endif
			RestoreTableScroll.ResetScroll();
		}

		[HarmonyPriority(int.MinValue)]
		static void Postfix(SelectLevelCategoryViewController __instance) {
			Config.Instance.LastCategory = __instance.selectedLevelCategory.ToString();
		}
	}
}
