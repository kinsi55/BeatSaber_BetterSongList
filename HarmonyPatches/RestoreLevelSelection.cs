using BetterSongList.Util;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.ShowPacksInSecondChildController))]
	static class PackPreselect {
		[HarmonyPriority(int.MinValue)]
		static void Prefix(ref string ____levelPackIdToBeSelectedAfterPresent) {
			____levelPackIdToBeSelectedAfterPresent ??= NavigationRestorePrepare.collection ?? "";
		}
	}

	/*
	 * For some reason that I do not know yet, SOME TIMES, for SOME PEOPLE (Obviously, not me)
	 * SelectLevel() will be called while _previewBeatmapLevels is null (Eventho SetData() was
	 * already called beforehand. It will then later be re-called from a different control flow.
	 * Honestly, for now I CBA to investigate why this happens - The only thing that this patch
	 * does is prevent a possible Exception that would otherwise happen if this function was
	 * called with the given / checked state
	 * 
	 * TODO: Endure the pain that is finding out why this happens
	 */
	[HarmonyPatch(typeof(LevelCollectionTableView), "SelectLevel")]
	static class PreventExceptionWhenSelectLevelIsCalledEventhoPreviewBeatmapLevelsIsEmptyForSomeReason {
		static bool Prefix(IPreviewBeatmapLevel[] ____previewBeatmapLevels) => ____previewBeatmapLevels != null;
	}

	[HarmonyPatch(typeof(LevelCollectionNavigationController), "DidActivate")]
	static class LevelPreselect {
		[HarmonyPriority(int.MinValue)]
		static void Prefix(LevelCollectionNavigationController __instance, bool addedToHierarchy, ref IPreviewBeatmapLevel ____beatmapLevelToBeSelectedAfterPresent) {
			var restoreTo = NavigationRestorePrepare.level;

			if(!addedToHierarchy || ____beatmapLevelToBeSelectedAfterPresent != null || !Config.Instance.ReselectLastSong || restoreTo == null)
				return;

			____beatmapLevelToBeSelectedAfterPresent = HookLevelCollectionTableSet.lastOutMapList?.FirstOrDefault(x => x.levelID == restoreTo);
#if TRACE
			Plugin.Log.Warn(string.Format("LevelCollectionNavigationController.DidActivate():Prefix ____beatmapLevelToBeSelectedAfterPresent {0}", ____beatmapLevelToBeSelectedAfterPresent));
#endif
		}

		/*
		 * The above will call LevelCollectionViewController.SelectLevel() which would then f up the scroll restore logic as,
		 * by the next table refresh, it will then scroll to an idx instead of the specific map
		 */
	static void Postfix(bool addedToHierarchy, ref bool ____hideDetailViewController) {
#if TRACE
			Plugin.Log.Error("LevelCollectionNavigationController.DidActivate():Postfix");
#endif
			RestoreTableScroll.GotoLastSelectedOnNextSetData();

			/*
			 * If we dont do that, the pre-selected level (Assuming another level wasnt selected afterwads)
			 * will be unselected when we leave and return to the level list (By playing the song, etc)
			 */
			if(addedToHierarchy)
				____hideDetailViewController = false;
		}
	}


	/*
	 * Gotta preselect the category here because if I hack-pre-set it in the LevelSelectionFlowCoordinator.State
	 * stuff falls apart as it tries to use AnnotatedBeatmapLevelCollectionsViewController
	 */
	[HarmonyPatch(typeof(SelectLevelCategoryViewController), nameof(SelectLevelCategoryViewController.Setup))]
	static class RestoreLevelSelection {
		static void Prefix(ref SelectLevelCategoryViewController.LevelCategory selectedCategory) {
			var restoreTo = NavigationRestorePrepare.category;

			if(restoreTo != SelectLevelCategoryViewController.LevelCategory.None)
				selectedCategory = restoreTo;
#if TRACE
			Plugin.Log.Warn(string.Format("SelectLevelCategoryViewController.Setup(): selectedCategory: {0}", selectedCategory));
#endif
		}
	}


	[HarmonyPatch(typeof(LevelPackDetailViewController), nameof(LevelPackDetailViewController.RefreshAvailabilityAsync))]
	static class SongDetailView {
		static bool Prefix(LevelPackDetailViewController __instance, ref IBeatmapLevelPack ____pack) {
#if TRACE
			Plugin.Log.Warn("LevelPackDetailViewController.RefreshAvailabilityAsync()");
#endif
			/*
			 * If LevelCollectionNavigationController loads with no _beatmapLevelToBeSelectedAfterPresent
			 * it will activate the levelPackDetailViewController, which will then die because it doesnt
			 * have any pack setup
			 */
			____pack ??= SongCore.Loader.CustomLevelsPack;

			if(____pack == null) {
				__instance.gameObject.SetActive(false);
				return false;
			}

			return true;
		}
	}
}
