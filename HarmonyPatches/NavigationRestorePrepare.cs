using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetterSongList.Util;
using HarmonyLib;

namespace BetterSongList.HarmonyPatches {

	[HarmonyPatch(typeof(LevelSelectionFlowCoordinator), "DidActivate")]
	static class NavigationRestorePrepare {
		static string _levelpack = null;
		static SelectLevelCategoryViewController.LevelCategory _category = SelectLevelCategoryViewController.LevelCategory.None;
		static string _level = null;

		public static string collection {
			get {
				var l = _levelpack;
				_levelpack = null;
				return l;
			}
		}
		public static SelectLevelCategoryViewController.LevelCategory category {
			get {
				var l = _category;
				_category = SelectLevelCategoryViewController.LevelCategory.None;
				return l;
			}
		}
		public static string level {
			get {
				var l = _level;
				_level = null;
				return l;
			}
		}

		static void Prefix(LevelSelectionFlowCoordinator.State ____startState) {
			if(____startState != null && (____startState.beatmapLevelPack != null || ____startState.previewBeatmapLevel != null || ____startState.levelCategory != SelectLevelCategoryViewController.LevelCategory.None)) {
#if DEBUG
				Plugin.Log.Warn("Not restoring last state because we are starting off from somewhere!");
#endif
				return;
			}

			Enum.TryParse(Config.Instance.LastCategory, out _category);
			_levelpack = PlaylistsUtil.GetPack(Config.Instance.LastPack)?.packID;
			_level = Config.Instance.LastSong;
		}
	}
}
