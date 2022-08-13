using BetterSongList.Util;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.ShowPacksInSecondChildController))]
	static class PackPreselect {
		[HarmonyPriority(int.MinValue)]
		static void Prefix(ref string ____levelPackIdToBeSelectedAfterPresent) {
			____levelPackIdToBeSelectedAfterPresent ??= LevelSelectionFlowCoordinator_DidActivate.restoredPackid;
		}
	}

	[HarmonyPatch(typeof(LevelSelectionFlowCoordinator), "DidActivate")]
	static class LevelSelectionFlowCoordinator_DidActivate {
		static readonly ConstructorInfo thingy = AccessTools.FirstConstructor(typeof(LevelSelectionFlowCoordinator.State), x => x.GetParameters().Length == 4);
		// Why this isnt publicly accessible through a method? Nobody fucking knows.
		static readonly IPA.Utilities.FieldAccessor<BeatmapLevelsModel, Dictionary<string, IPreviewBeatmapLevel>>.Accessor BeatmapLevelsModel_loadedPreviewBeatmapLevels =
			IPA.Utilities.FieldAccessor<BeatmapLevelsModel, Dictionary<string, IPreviewBeatmapLevel>>.GetAccessor("_loadedPreviewBeatmapLevels");

		static BeatmapLevelsModel beatmapLevelsModel = UnityEngine.Object.FindObjectOfType<BeatmapLevelsModel>();

		public static string restoredPackid = null;

		static void Prefix(ref LevelSelectionFlowCoordinator.State ____startState) {
			restoredPackid = ____startState?.beatmapLevelPack?.packID;
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

			restoredPackid = Config.Instance.LastPack == null ? null : PlaylistsUtil.GetPack(Config.Instance.LastPack)?.packID;

			____startState = (LevelSelectionFlowCoordinator.State)thingy.Invoke(new object[] {
				restoreCategory,
				restoredPackid == null ? null : new BeatmapLevelPack(restoredPackid, null, null, null, null, null),
				m,
				null
			});
		}
	}
}
