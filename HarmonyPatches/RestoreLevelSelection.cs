using BetterSongList.Util;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelSelectionFlowCoordinator), "DidActivate")]
	static class LevelSelectionFlowCoordinator_DidActivate {
		static readonly ConstructorInfo thingy = AccessTools.FirstConstructor(typeof(LevelSelectionFlowCoordinator.State), x => !x.IsPublic);

		static void Prefix(ref LevelSelectionFlowCoordinator.State ____startState) {
			if(____startState != null) {
#if DEBUG
				Plugin.Log.Warn("Not restoring last state because we are starting off from somewhere!");
#endif
				return;
			}

			if(!Enum.TryParse(Config.Instance.LastCategory, out SelectLevelCategoryViewController.LevelCategory restoreCategory))
				restoreCategory = SelectLevelCategoryViewController.LevelCategory.None;

			var restoreLevel = Config.Instance.LastSong;

			IPreviewBeatmapLevel m = null;

			foreach(var x in SongCore.Loader.BeatmapLevelsModelSO.allLoadedBeatmapLevelPackCollection.beatmapLevelPacks) {
				foreach(var y in x.beatmapLevelCollection.beatmapLevels) {
					if(y.levelID != restoreLevel)
						continue;

					m = y;
					break;
				}
			}

			____startState = (LevelSelectionFlowCoordinator.State)thingy.Invoke(new object[] {
				restoreCategory,
				new BeatmapLevelPack(PlaylistsUtil.GetPack(Config.Instance.LastPack)?.packID, null, null, null, null, null),
				m,
				null
			});
		}
	}
}
