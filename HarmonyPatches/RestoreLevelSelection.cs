using BetterSongList.UI;
using BetterSongList.Util;
using HarmonyLib;
using System;
using static SelectLevelCategoryViewController;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.ShowPacksInSecondChildController))]
	static class PackPreselect {
		public static BeatmapLevelPack restoredPack = null;

		public static void LoadPackFromCollectionName() {
			if(restoredPack?.shortPackName == Config.Instance.LastPack)
				return;

			if(Config.Instance.LastPack == null) {
				restoredPack = null;
				return;
			}

			restoredPack = PlaylistsUtil.GetPack(Config.Instance.LastPack);
		}

		[HarmonyPriority(int.MinValue)]
		static void Prefix(ref string ____levelPackIdToBeSelectedAfterPresent) {
			if(____levelPackIdToBeSelectedAfterPresent != null)
				return;

			LoadPackFromCollectionName();
			____levelPackIdToBeSelectedAfterPresent = restoredPack?.packID;
		}
	}

	// Animation might get stuck when switching category if it hasn't finished.
	[HarmonyPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.HandleSelectLevelCategoryViewControllerDidSelectLevelCategory))]
	static class PackPreselectAnimationFix {
		static void Postfix(LevelFilteringNavigationController __instance) {
			__instance._annotatedBeatmapLevelCollectionsViewController._annotatedBeatmapLevelCollectionsGridView._animator.DespawnAllActiveTweens();
		}
	}

	// For some reason the collection is trying to be closed when it hasn't been opened yet.
	[HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionsGridView), nameof(AnnotatedBeatmapLevelCollectionsGridView.CloseLevelCollection))]
	static class CloseLevelCollectionFix {
		static bool Prefix(AnnotatedBeatmapLevelCollectionsGridView __instance) => __instance._gridView.columnCount != 0;
	}

	[HarmonyPatch(typeof(LevelSelectionFlowCoordinator), nameof(LevelSelectionFlowCoordinator.DidActivate))]
	static class LevelSelectionFlowCoordinator_DidActivate {
		static void Prefix(LevelSelectionFlowCoordinator __instance, ref LevelSelectionFlowCoordinator.State ____startState, bool addedToHierarchy) {
			if(!addedToHierarchy)
				return;

			if(____startState != null) {
#if DEBUG
				Plugin.Log.Warn("Not restoring last state because we are starting off from somewhere!");
#endif
				FilterUI.SetFilter(null, false, false);
				return;
			}

			if(!Enum.TryParse(Config.Instance.LastCategory, out LevelCategory restoreCategory))
				restoreCategory = LevelCategory.None;

			if(Config.Instance.LastSong == null ||
			   !__instance
			   .levelSelectionNavigationController
			   ._levelFilteringNavigationController
			   ._beatmapLevelsModel
			   ._allLoadedBeatmapLevelsRepository
			   .TryGetBeatmapLevelById(Config.Instance.LastSong, out var lastSelectedLevel)
			)
				lastSelectedLevel = null;

			PackPreselect.LoadPackFromCollectionName();

			var pack = PackPreselect.restoredPack;

			if(restoreCategory == LevelCategory.All || restoreCategory == LevelCategory.Favorites)
				pack = SongCore.Loader.CustomLevelsPack;

			____startState = new LevelSelectionFlowCoordinator.State(
				restoreCategory,
				pack,
				new BeatmapKey(),
				lastSelectedLevel);
		}
	}
}
