using BetterSongList.UI;
using HarmonyLib;
using System.Diagnostics;
using System.Linq;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelSearchViewController), nameof(LevelSearchViewController.ResetCurrentFilterParams))]
	static class HookFilterClear {
		[HarmonyPriority(int.MaxValue)]
		static void Prefix() {
			// GetCallingAssembly isnt good enough because of Harmony patches
			var x = new StackTrace().GetFrames().DefaultIfEmpty(null).ElementAtOrDefault(2)?.GetMethod().DeclaringType.Assembly.GetName().Name;

			// Compat for stuff like SRM mods so that we clear our filter as well when they request a basegame filter clear
			if(x == "Main")
				return;

#if DEBUG
			Plugin.Log.Debug(string.Format("HookFilterClear(): Filter clear requested from other assembly ({0})", System.Reflection.Assembly.GetCallingAssembly().GetName()));
#endif
			FilterUI.SetFilter(null, false, false);
		}
	}
}
