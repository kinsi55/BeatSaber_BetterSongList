using HarmonyLib;
using HMUI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch]
	static class RestoreTableScroll {
		static IEnumerable<MethodBase> TargetMethods() => AccessTools.GetDeclaredMethods(typeof(LevelCollectionTableView)).Where(x => x.Name == nameof(LevelCollectionTableView.Init));

		static int? scrollToIndex = null;
		static bool doResetScrollOnNext = false;

		public static void ResetScroll() {
			scrollToIndex = 0;
			doResetScrollOnNext = true;

#if TRACE
			Plugin.Log.Warn("RestoreTableScroll.ResetScroll()");
#endif
		}

		[HarmonyPriority(int.MaxValue)]
		static void Prefix(LevelCollectionTableView __instance) {
			if(__instance._isInitialized && __instance._tableView != null && __instance._beatmapLevels != null && !doResetScrollOnNext)
				scrollToIndex = __instance._tableView.GetVisibleCellsIdRange().Item1;

			doResetScrollOnNext = false;

#if TRACE
			Plugin.Log.Warn(string.Format("LevelCollectionTableView.Init():Prefix - scrollToIndex: {0}", scrollToIndex));
#endif
		}

		[HarmonyPatch(typeof(LevelCollectionTableView), nameof(LevelCollectionTableView.SetData))]
		static class DoTheFunnySelect {
			[HarmonyPriority(int.MinValue)]
			static void Postfix(LevelCollectionTableView __instance) {
#if TRACE
				Plugin.Log.Warn(string.Format("DoTheFunnySelect -> LevelCollectionTableView.SetData():Postfix scrollToIndex: {0}", scrollToIndex));
#endif

				if(scrollToIndex == null || scrollToIndex < 0)
					return;

#if TRACE
				Plugin.Log.Warn(string.Format("-> Scrolling to {0}", scrollToIndex));
#endif

				__instance._tableView.ScrollToCellWithIdx(
					(int)scrollToIndex,
					TableView.ScrollPositionType.Beginning,
					false
				);
			}
		}
	}
}
