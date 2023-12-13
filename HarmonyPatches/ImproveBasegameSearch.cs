using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelFilter), nameof(LevelFilter.FilterLevelByText))]
	static class ImproveBasegameSearch {
		[HarmonyPriority(int.MinValue + 10)]
		static void Prefix(ref string[] searchTerms) {
			if(!Config.Instance.ModBasegameSearch)
				return;

			if(searchTerms.Length > 1)
				searchTerms = new string[] { string.Join(" ", searchTerms) };
		}
	}
}
