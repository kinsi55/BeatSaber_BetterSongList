using HarmonyLib;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelCollectionTableView), nameof(LevelCollectionTableView.HandleDidSelectRowEvent))]
	static class HookSelectedInTable {
		[HarmonyPriority(int.MinValue)]
		static void Postfix(BeatmapLevel ____selectedBeatmapLevel) {
			Config.Instance.LastSong = ____selectedBeatmapLevel?.levelID;

#if TRACE
			Plugin.Log.Warn(string.Format("LevelCollectionTableView.HandleDidSelectRowEvent(): LastSong: {0}", Config.Instance.LastSong));
#endif
		}
	}
}
