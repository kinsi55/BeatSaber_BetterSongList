using HarmonyLib;
using HMUI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch]
	static class RestoreTableScroll {
		static IEnumerable<MethodBase> TargetMethods() => AccessTools.GetDeclaredMethods(typeof(LevelCollectionTableView)).Where(x => x.Name == "Init");

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
		static void Prefix(bool ____isInitialized, TableView ____tableView, IPreviewBeatmapLevel[] ____previewBeatmapLevels) {
			if(____isInitialized && ____tableView != null && ____previewBeatmapLevels != null && !doResetScrollOnNext)
				scrollToIndex = ____tableView.GetVisibleCellsIdRange().Item1;

			doResetScrollOnNext = false;

#if TRACE
			Plugin.Log.Warn(string.Format("LevelCollectionTableView.Init():Prefix - scrollToIndex: {0}", scrollToIndex));
#endif
		}

		[HarmonyPatch(typeof(LevelCollectionTableView), nameof(LevelCollectionTableView.SetData))]
		static class DoTheFunnySelect {
			[HarmonyPriority(int.MinValue)]
			static void Postfix(TableView ____tableView, IPreviewBeatmapLevel[] ____previewBeatmapLevels, bool ____showLevelPackHeader) {
#if TRACE
				Plugin.Log.Warn(string.Format("DoTheFunnySelect -> LevelCollectionTableView.SetData():Postfix scrollToIndex: {0}", scrollToIndex));
#endif

				if(scrollToIndex == null || scrollToIndex < 0)
					return;

#if TRACE
				Plugin.Log.Warn(string.Format("-> Scrolling to {0}", scrollToIndex));
#endif

				____tableView.ScrollToCellWithIdx(
					(int)scrollToIndex,
					TableView.ScrollPositionType.Beginning,
					false
				);
			}
		}
	}
}
