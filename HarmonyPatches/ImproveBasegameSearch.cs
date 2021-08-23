using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(BeatmapLevelFilterModel), "LevelContainsText")]
	static class ImproveBasegameSearch {
		[HarmonyPriority(int.MinValue + 10)]
		static bool Prefix(IPreviewBeatmapLevel beatmapLevel, ref string[] searchTexts, ref bool __result) {
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
