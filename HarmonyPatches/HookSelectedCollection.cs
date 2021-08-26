using BetterSongList.UI;
using HarmonyLib;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionsViewController), nameof(AnnotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection))]
	public static class HookSelectedCollection {
		public static IAnnotatedBeatmapLevelCollection lastSelectedCollection { get; private set; }

		[HarmonyPriority(int.MinValue), HarmonyPrefix]
		static void CollectionSet(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection) {
			if(annotatedBeatmapLevelCollection != null) {
				// Save the collection we're on for reselection purposes
				Config.Instance.LastPack = annotatedBeatmapLevelCollection.collectionName ?? "";
			}
#if TRACE
			Plugin.Log.Warn("AnnotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection()");
#endif

			// If its a playlist we want to start off with no sorting and filtering - Requested by Pixel
			if(annotatedBeatmapLevelCollection != null && Config.Instance.ClearFiltersOnPlaylistSelect && annotatedBeatmapLevelCollection != SongCore.Loader.CustomLevelsPack) {
				FilterUI.SetSort(null, false, false);
				FilterUI.SetFilter(null, false, false);
				// Restore previously used Sort and filter for non-playlists
			} else if(lastSelectedCollection != null) {
				FilterUI.SetSort(Config.Instance.LastSort, false, false);
				FilterUI.SetFilter(Config.Instance.LastFilter, false, false);
			}

			lastSelectedCollection = annotatedBeatmapLevelCollection;
		}

		[HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionsViewController), "DidDeactivate")]
		static class HookLevelCollectionUnset {
			// When leaving playlists, null the "last selected" playlist
			static void Postfix() => CollectionSet(null);
		}

		[HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionsViewController), nameof(AnnotatedBeatmapLevelCollectionsViewController.SetData))]
		static class HookLevelCollectionInit {
			// Restore the playlist when re-entering playlists (Does not call HandleDidSelectAnnotatedBeatmapLevelCollection())
			static void Postfix(AnnotatedBeatmapLevelCollectionsViewController __instance) => CollectionSet(__instance.selectedAnnotatedBeatmapLevelCollection);
		}
	}
}
