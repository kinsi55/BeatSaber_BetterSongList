using HarmonyLib;

namespace BetterSongList.HarmonyPatches {
	/*
	 * With the normal Game control flow, LevelCollectionTableView.SetData() gets called twice when entering
	 * the solo view this will prevent that (Minor optimization). Could be yeeted if it causes problems
	 */
	[HarmonyPatch(typeof(LevelSearchViewController), nameof(LevelSearchViewController.ResetFilterParams))]
	static class PreventLevelSearchViewControllerDoubleInit {
		static bool Prefix(LevelSearchViewController __instance, ref bool ____onlyFavorites, bool onlyFavorites) {
			____onlyFavorites = onlyFavorites;

#if DEBUG
			Plugin.Log.Warn(string.Format("LevelSearchViewController.ResetFilterParams():Prefix isActiveAndEnabled: {0}, onlyFavorites: {1}", __instance.isActiveAndEnabled, onlyFavorites));
#endif

			return __instance.isActiveAndEnabled;
		}
	}
}
