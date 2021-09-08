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
		static bool gotoLastSelectedOnNextSetData = false;

		public static void ResetScroll() {
			scrollToIndex = 0;
			doResetScrollOnNext = true;

#if TRACE
			Plugin.Log.Warn("RestoreTableScroll.ResetScroll()");
#endif
		}

		public static void GotoLastSelectedOnNextSetData() {
			gotoLastSelectedOnNextSetData = true;
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
				bool specificMap = false;

				if(scrollToIndex == null || gotoLastSelectedOnNextSetData) {
					gotoLastSelectedOnNextSetData = false;
					// If we havent saved an index yet where to scroll to, work with the last selected level
					for(int i = 0; i < (____previewBeatmapLevels?.Length ?? 0); i++) {
						if(____previewBeatmapLevels[i].levelID == Config.Instance.LastSong) {
							scrollToIndex = i;
							specificMap = true;
							break;
						}
					}

					scrollToIndex ??= 0;
				}

				if(scrollToIndex < 0)
					return;

				if(____showLevelPackHeader)
					scrollToIndex++;

#if TRACE
				Plugin.Log.Warn(string.Format("-> Scrolling to {0} (Specific map: {1})", scrollToIndex, specificMap));
#endif

				if(specificMap)
					____tableView.SelectCellWithIdx((int)scrollToIndex, false);

				____tableView.ScrollToCellWithIdx(
					(int)scrollToIndex,
					specificMap ? TableView.ScrollPositionType.Center : TableView.ScrollPositionType.Beginning,
					specificMap
				);
			}
		}
	}
}
