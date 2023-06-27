using BetterSongList.UI;
using BetterSongList.Util;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static SelectLevelCategoryViewController;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.ShowPacksInSecondChildController))]
	static class PackPreselect {
		public static IBeatmapLevelPack restoredPack = null;

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

	[HarmonyPatch(typeof(LevelSelectionFlowCoordinator), "DidActivate")]
	static class LevelSelectionFlowCoordinator_DidActivate {
		static readonly ConstructorInfo thingy = AccessTools.FirstConstructor(typeof(LevelSelectionFlowCoordinator.State), x => x.GetParameters().Length == 4);

		static BeatmapLevelsModel beatmapLevelsModel = UnityEngine.Object.FindObjectOfType<BeatmapLevelsModel>();

		static void Prefix(ref LevelSelectionFlowCoordinator.State ____startState, bool addedToHierarchy) {
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

			if(Config.Instance.LastSong == null || !beatmapLevelsModel._loadedPreviewBeatmapLevels.TryGetValue(Config.Instance.LastSong, out var m))
				m = null;

			PackPreselect.LoadPackFromCollectionName();

			var pack = PackPreselect.restoredPack;

			if(restoreCategory == LevelCategory.All || restoreCategory == LevelCategory.Favorites)
				pack = SongCore.Loader.CustomLevelsPack;

			____startState = (LevelSelectionFlowCoordinator.State)thingy.Invoke(new object[] {
				restoreCategory,
				pack,
				m,
				null
			});
		}
	}
}
