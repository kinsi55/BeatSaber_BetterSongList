using BetterSongList.Util;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSongList.HarmonyPatches {
	//[HarmonyPatch(typeof(LevelSelectionFlowCoordinator), "DidActivate")]
	//static class HookLevelSelectionFlowCoordinator {
	//	public static bool isPreparing { get; private set; } = false;
	//	public static bool restoreInProgress { get; private set; } = false;

	//	public static IBeatmapLevelPack currentPack { get; private set; } = null;

	//	[HarmonyPriority(Priority.VeryLow)]
	//	static void Prefix(bool addedToHierarchy, ref LevelSelectionFlowCoordinator.State ____startState) {
	//		Plugin.Log.Critical("LevelSelectionFlowCoordinator.DidActivate():Prefix");
	//		isPreparing = addedToHierarchy;
	//		restoreInProgress = isPreparing && ____startState == null;

	//		if(restoreInProgress) {
	//			//Enum.TryParse<SelectLevelCategoryViewController.LevelCategory>(Config.Instance.LastCategory, out var scategory);

	//			/*
	//			 * AAAAND this is where I would pass a state with The right Category, Level Pack and level...
	//			 * But the basegame does not do that anywhere, and it seems like it is broken because when doing
	//			 * that, errors are thrown and stuff collapses... so we gotta do more junk with more hooks. Ugh
	//			 */
	//			//____startState = new LevelSelectionFlowCoordinator.State(
	//			//	PlaylistsUtil.GetPack(Config.Instance.LastPack),
	//			//	SongCore.Loader.CustomLevels.First().Value
	//			//);
	//			//IPA.Utilities.ReflectionUtil.SetField(____startState, "levelCategory", (SelectLevelCategoryViewController.LevelCategory?)scategory);
	//			//SongDetailView.blockNextLevelpackIfEmpty = true;
	//		}

	//		//currentPack = ____startState?.beatmapLevelPack;
	//	}

	//	[HarmonyPriority(Priority.First)]
	//	static void Postfix(LevelSelectionNavigationController ___levelSelectionNavigationController) {
	//		Plugin.Log.Critical("LevelSelectionFlowCoordinator.DidActivate():Postfix");
	//		restoreInProgress = isPreparing = false;
	//	}

	//	//Needing an extra hook for this is kinda meh
	//	[HarmonyPatch(typeof(LevelCollectionNavigationController), "DidActivate")]
	//	static class LevelPreselect {
	//		[HarmonyPriority(int.MinValue)]
	//		static bool Prefix(LevelCollectionNavigationController __instance, bool addedToHierarchy, ref IPreviewBeatmapLevel ____beatmapLevelToBeSelectedAfterPresent) {
	//			if(!addedToHierarchy || ____beatmapLevelToBeSelectedAfterPresent != null || !Config.Instance.ReselectLastSong)
	//				return true;

	//			____beatmapLevelToBeSelectedAfterPresent = HookLevelCollectionTableView.lastInMapList?.FirstOrDefault(x => x.levelID == Config.Instance.LastSong);


	//			return true;
	//		}
	//	}

	//	/*
	//	 * Doing this with a seperate hook. I could just inject it into the ____startState of LevelSelectionFlowCoordinator
	//	 * but doing so would break stuff :/ */
	//	//[HarmonyPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.ShowPacksInSecondChildController))]
	//	//static class PackPreselect {
	//	//	[HarmonyPriority(int.MinValue)]
	//	//	static void Prefix(IReadOnlyList<IBeatmapLevelPack> beatmapLevelPacks, ref string ____levelPackIdToBeSelectedAfterPresent) {
	//	//		____levelPackIdToBeSelectedAfterPresent = PlaylistsUtil.GetPack(Config.Instance.LastPack)?.packID;
	//	//	}
	//	//}
	//}

	[HarmonyPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.ShowPacksInSecondChildController))]
	static class PackPreselect {
		[HarmonyPriority(int.MinValue)]
		static void Prefix(IReadOnlyList<IBeatmapLevelPack> beatmapLevelPacks, ref string ____levelPackIdToBeSelectedAfterPresent) {
			____levelPackIdToBeSelectedAfterPresent = PlaylistsUtil.GetPack(Config.Instance.LastPack)?.packID;
		}
	}

	[HarmonyPatch(typeof(LevelCollectionNavigationController), "DidActivate")]
	static class LevelPreselect {
		[HarmonyPriority(int.MinValue)]
		static void Prefix(LevelCollectionNavigationController __instance, bool addedToHierarchy, ref IPreviewBeatmapLevel ____beatmapLevelToBeSelectedAfterPresent) {
			if(!addedToHierarchy || ____beatmapLevelToBeSelectedAfterPresent != null || !Config.Instance.ReselectLastSong)
				return;

			____beatmapLevelToBeSelectedAfterPresent = HookLevelCollectionTableView.lastOutMapList?.FirstOrDefault(x => x.levelID == Config.Instance.LastSong);
#if TRACE
			Plugin.Log.Warn(string.Format("LevelCollectionNavigationController.DidActivate():Prefix ____beatmapLevelToBeSelectedAfterPresent {0}", ____beatmapLevelToBeSelectedAfterPresent));
#endif
		}

		/*
		 * The above will call LevelCollectionViewController.SelectLevel() which would then f up the scroll restore logic as,
		 * by the next table refresh, it will then scroll to an idx instead of the specific map
		 */
		static void Postfix(bool addedToHierarchy, ref bool ____hideDetailViewController) {
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
			Enum.TryParse<SelectLevelCategoryViewController.LevelCategory>(Config.Instance.LastCategory, out selectedCategory);
#if TRACE
			Plugin.Log.Warn(string.Format("SelectLevelCategoryViewController.Setup(): selectedCategory: {0}", selectedCategory));
#endif
			//if(!HookLevelSelectionFlowCoordinator.restoreInProgress)
			//	return;


			/*
			 * Need to manually do that as the LevelPackDetailViewController isnt populated when you start right into the "All"
			 * tab, which would then break stuff if we are not pre-selecting a song as it tries to show the cover.
			 * Should probably handle this differently or just never activate the LevelPackDetailViewController in that case
			 */
			if(selectedCategory == SelectLevelCategoryViewController.LevelCategory.All) {
				//var x = Resources.FindObjectsOfTypeAll<LevelCollectionNavigationController>().FirstOrDefault();

				//if(x != null) {
				//	ReflectionUtil.SetField(x, "_levelPack", HookLevelSelectionFlowCoordinator.currentPack);

				//	Resources.FindObjectsOfTypeAll<LevelPackDetailViewController>().FirstOrDefault().SetData(HookLevelSelectionFlowCoordinator.currentPack);
				//}
			}
		}
	}


	[HarmonyPatch(typeof(LevelPackDetailViewController), nameof(LevelPackDetailViewController.RefreshAvailabilityAsync))]
	static class SongDetailView {
		public static bool blockNextLevelpackIfEmpty = false;
		static void Prefix(LevelPackDetailViewController __instance, ref IBeatmapLevelPack ____pack) {
#if TRACE
			Plugin.Log.Warn("LevelPackDetailViewController.RefreshAvailabilityAsync()");
#endif
			/*
			 * If LevelCollectionNavigationController loads with no _beatmapLevelToBeSelectedAfterPresent
			 * it will activate the levelPackDetailViewController, which will then die because it doesnt
			 * have any pack setup
			 */
			____pack ??= SongCore.Loader.CustomLevelsPack;
		}
	}
}
