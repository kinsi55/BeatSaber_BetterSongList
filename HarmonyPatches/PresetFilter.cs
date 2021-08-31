using HarmonyLib;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelFilterParams), "filterByOwned", MethodType.Getter)]
	static class PresetFilter {
		static void Postfix(ref bool __result) {
			__result = __result || Config.Instance.AutoFilterUnowned;
		}
	}
}
