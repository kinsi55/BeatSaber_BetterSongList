using BetterSongList.UI;
using HarmonyLib;
using System.Reflection;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelSearchViewController), nameof(LevelSearchViewController.ResetCurrentFilterParams))]
	static class HookFilterClear {
		static void Prefix() {
			// Compat for stuff like SRM mods so that we clear our filter as well when they request a basegame filter clear
			if(Assembly.GetCallingAssembly().GetName().Name == "Main")
				return;

			FilterUI.SetFilter(null, false, false);
		}
	}
}
