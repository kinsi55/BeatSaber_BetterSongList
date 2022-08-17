using BetterSongList.Util;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
		// Why this isnt publicly accessible through a method? Nobody fucking knows.
		static readonly IPA.Utilities.FieldAccessor<BeatmapLevelsModel, Dictionary<string, IPreviewBeatmapLevel>>.Accessor BeatmapLevelsModel_loadedPreviewBeatmapLevels =
			IPA.Utilities.FieldAccessor<BeatmapLevelsModel, Dictionary<string, IPreviewBeatmapLevel>>.GetAccessor("_loadedPreviewBeatmapLevels");

		static BeatmapLevelsModel beatmapLevelsModel = UnityEngine.Object.FindObjectOfType<BeatmapLevelsModel>();

		static void Prefix(ref LevelSelectionFlowCoordinator.State ____startState, bool firstActivation) {
			if(____startState != null) {
#if DEBUG
				Plugin.Log.Warn("Not restoring last state because we are starting off from somewhere!");
#endif
				return;
			}

			if(!Enum.TryParse(Config.Instance.LastCategory, out SelectLevelCategoryViewController.LevelCategory restoreCategory))
				restoreCategory = SelectLevelCategoryViewController.LevelCategory.None;

			if(!BeatmapLevelsModel_loadedPreviewBeatmapLevels(ref beatmapLevelsModel).TryGetValue(Config.Instance.LastSong, out var m))
				m = null;

			PackPreselect.LoadPackFromCollectionName();

			____startState = (LevelSelectionFlowCoordinator.State)thingy.Invoke(new object[] {
				restoreCategory,
				PackPreselect.restoredPack,
				m,
				null
			});
		}
	}
}
