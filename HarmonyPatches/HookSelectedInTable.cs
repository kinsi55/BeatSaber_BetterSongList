using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
