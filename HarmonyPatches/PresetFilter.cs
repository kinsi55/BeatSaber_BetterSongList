using HarmonyLib;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelSearchViewController), nameof(LevelSearchViewController.ResetOptionFilterSettings))]
	static class PresetFilter {
		static void Postfix(LevelSearchViewController __instance) {
			if(Config.Instance.AutoFilterUnowned)
				__instance._currentSearchFilter.songOwned = true;
		}
	}
}
