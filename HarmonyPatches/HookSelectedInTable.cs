using HarmonyLib;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelCollectionTableView), nameof(LevelCollectionTableView.HandleDidSelectRowEvent))]
	static class HookSelectedInTable {
		[HarmonyPriority(int.MinValue)]
		static void Postfix(IPreviewBeatmapLevel ____selectedPreviewBeatmapLevel) {
			Config.Instance.LastSong = ____selectedPreviewBeatmapLevel?.levelID;

#if TRACE
			Plugin.Log.Warn(string.Format("LevelCollectionTableView.HandleDidSelectRowEvent(): LastSong: {0}", Config.Instance.LastSong));
#endif
		}
	}
}
