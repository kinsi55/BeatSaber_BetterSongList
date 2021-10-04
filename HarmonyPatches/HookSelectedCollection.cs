using BetterSongList.UI;
using HarmonyLib;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionsViewController), nameof(AnnotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection))]
	static class HookSelectedCollection {
		public static IAnnotatedBeatmapLevelCollection lastSelectedCollection { get; private set; }

		[HarmonyPriority(int.MinValue), HarmonyPrefix]
		static void CollectionSet(IAnnotatedBeatmapLevelCollection beatmapLevelCollection) {
			if(beatmapLevelCollection != null) {
				// Save the collection we're on for reselection purposes
				Config.Instance.LastPack = beatmapLevelCollection.collectionName ?? "";
			}
#if TRACE
			Plugin.Log.Warn(string.Format("AnnotatedBeatmapLevelCollectionsViewController.HandleDidSelectAnnotatedBeatmapLevelCollection(): {0}", beatmapLevelCollection?.collectionName));
#endif

			// If its a playlist we want to start off with no sorting and filtering - Requested by Pixel
			if(beatmapLevelCollection != null && Config.Instance.ClearFiltersOnPlaylistSelect && beatmapLevelCollection != SongCore.Loader.CustomLevelsPack) {
				FilterUI.SetSort(null, false, false);
				FilterUI.SetFilter(null, false, false);
				// Restore previously used Sort and filter for non-playlists
			} else if(lastSelectedCollection != null) {
				FilterUI.SetSort(Config.Instance.LastSort, false, false);
				FilterUI.SetFilter(Config.Instance.LastFilter, false, false);
			}

			lastSelectedCollection = beatmapLevelCollection;
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
