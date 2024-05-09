﻿using BetterSongList.UI;
using HarmonyLib;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionsViewController), nameof(AnnotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection))]
	static class HookSelectedCollection {
		public static BeatmapLevelPack lastSelectedCollection { get; private set; }
		public static bool doRestoreFilter = false;

		[HarmonyPriority(int.MinValue), HarmonyPrefix]
		static void CollectionSet(BeatmapLevelPack beatmapLevelCollection) {
			if(beatmapLevelCollection != null) {
				// Save the collection we're on for reselection purposes
				Config.Instance.LastPack = beatmapLevelCollection.packName ?? "";
			}
#if TRACE
			Plugin.Log.Warn(string.Format("AnnotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection(): {0}", beatmapLevelCollection?.packName));

			//System.Console.WriteLine("=> {0}", new System.Diagnostics.StackTrace().ToString());
#endif

			// If its a playlist we want to start off with no sorting and filtering - Requested by Pixel
			if(Config.Instance.ClearFiltersOnPlaylistSelect) {
				if(beatmapLevelCollection != null && beatmapLevelCollection != SongCore.Loader.CustomLevelsPack) {
					FilterUI.SetSort(null, false, false);

					if(doRestoreFilter = HookLevelCollectionTableSet.filter != null)
						FilterUI.SetFilter(null, false, false);
					// Restore previously used Sort and filter for non-playlists
				} else if(lastSelectedCollection != null) {
					FilterUI.SetSort(Config.Instance.LastSort, false, false);
					if(doRestoreFilter)
						FilterUI.SetFilter(Config.Instance.LastFilter, false, false);
					doRestoreFilter = false;
				}
			}

			lastSelectedCollection = beatmapLevelCollection;
			FilterUI.persistentNuts?.UpdateTransformerOptionsAndDropdowns();
		}

		[HarmonyPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.UpdateSecondChildControllerContent))]
		static class HookLevelCollectionUnset {
			// When leaving playlists, null the "last selected" playlist
			static void Prefix(SelectLevelCategoryViewController.LevelCategory levelCategory) {
				if(levelCategory != SelectLevelCategoryViewController.LevelCategory.CustomSongs)
					CollectionSet(null);
			}
		}
	}
}
