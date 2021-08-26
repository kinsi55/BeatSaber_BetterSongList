using HarmonyLib;
using System;
using System.Linq;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(BeatmapLevelFilterModel), "LevelContainsText")]
	static class ImproveBasegameSearch {
		[HarmonyPriority(int.MinValue + 10)]
		static bool Prefix(IPreviewBeatmapLevel beatmapLevel, ref string[] searchTexts, ref bool __result) {
			if(!Config.Instance.ModBasegameSearch)
				return true;

			if(searchTexts.Any(x => x.Length > 2 && beatmapLevel.levelAuthorName.IndexOf(x, 0, StringComparison.CurrentCultureIgnoreCase) != -1)) {
				__result = true;
				return false;
			}
			if(searchTexts.Length > 1)
				searchTexts = new string[] { string.Join(" ", searchTexts) };

			return true;
		}
	}
}
