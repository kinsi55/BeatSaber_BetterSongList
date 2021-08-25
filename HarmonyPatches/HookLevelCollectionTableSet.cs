using BetterSongList.FilterModels;
using BetterSongList.SortModels;
using BetterSongList.UI;
using BetterSongList.Util;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace BetterSongList.HarmonyPatches {
	// The main class that handles the modification of the data in the song list
	[HarmonyPatch(typeof(LevelCollectionTableView), nameof(LevelCollectionTableView.SetData))]
	static class HookLevelCollectionTableSet {
		public static ISorter sorter;
		public static IFilter filter;

		public static IPreviewBeatmapLevel[] lastInMapList { get; private set; }
		public static IPreviewBeatmapLevel[] lastOutMapList { get; private set; }
		static Action<IPreviewBeatmapLevel[]> recallLast = null;


		static IPreviewBeatmapLevel[] asyncPreprocessed;

		/// <summary>
		/// Refresh the SongList with the last used BeatMaps array
		/// </summary>
		/// <param name="processAsync"></param>
		public static void Refresh(bool processAsync = false) {
			if(lastInMapList == null)
				return;
#if TRACE
			Plugin.Log.Warn(string.Format("Refresh({0})", processAsync));
#endif
			/*
			 * This probably has problems in regards to race conditions / thread safety... We will see...
			 * Pre-processes the desired songlist state in a second thread - This will then get stored in
			 * a vaiable and used as the result on the next SetData() in the Prefix hook below
			 */
			asyncPreprocessed = null;
			if(processAsync) {
				PrepareStuffIfNecessary(async () => {
					// TODO: Maybe cancellationsource etc
					var inList = lastInMapList;
					await Task.Run(() => FilterWrapper(ref inList));
					asyncPreprocessed = inList;
					Refresh(false);
				}, true);
				return;
			}

			var ml = lastInMapList;
			/*
			 * Forcing a refresh of the table by skipping the optimization check in the SetData():Prefix
			 * because Refresh() is only called in situations where the result will probably change
			 */
			lastInMapList = null;
			recallLast(ml);
		}

		static void FilterWrapper(ref IPreviewBeatmapLevel[] previewBeatmapLevels) {
			if(filter?.isReady != true && sorter?.isReady != true)
				return;

#if TRACE
			Plugin.Log.Debug("FilterWrapper()");
#endif

			try {
				var outV = previewBeatmapLevels.AsEnumerable();

				if(filter?.isReady == true)
					outV = outV.Where(filter.GetValueFor);

				if(sorter?.isReady == true) {
					if(sorter is ISorterPrimitive) {
						((ISorterPrimitive)sorter).DoSort(ref outV);
					} else {
						outV = outV.OrderBy(x => x, sorter);
					}
				}

				previewBeatmapLevels = outV.ToArray();

				if(sorter is ISorterWithLegend && Config.Instance.EnableAlphabetScrollbar)
					customLegend = ((ISorterWithLegend)sorter).BuildLegend(previewBeatmapLevels);
			} catch(Exception ex) {
				Plugin.Log.Warn(string.Format("FilterWrapper() Exception: {0}", ex));
			}
		}

		static CancellationTokenSource sortCancelSrc;

		static bool PrepareStuffIfNecessary(Action cb = null, bool cbOnAlreadyPrepared = false) {
			if(sorter?.isReady == false || filter?.isReady == false) {
#if TRACE
				Plugin.Log.Debug("PrepareStuffIfNecessary()->Prepare");
#endif
				XD.FunnyNull(FilterUI.persistentNuts._filterLoadingIndicator)?.gameObject.SetActive(true);
				sortCancelSrc?.Cancel();
				var thisSrc = sortCancelSrc = new CancellationTokenSource();

				Task.WhenAll(new[] {
					sorter?.isReady == false ? sorter.Prepare(thisSrc.Token) : Task.CompletedTask,
					filter?.isReady == false ? filter.Prepare(thisSrc.Token) : Task.CompletedTask
				}).ContinueWith(x => {
#if TRACE
					Plugin.Log.Debug("PrepareStuffIfNecessary()->ContinueWith");
#endif
					if(sortCancelSrc != thisSrc)
						return;

					sortCancelSrc = null;

					if(!thisSrc.IsCancellationRequested && cb != null)
						cb();
				}, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());

				return true;
			}
			if(cbOnAlreadyPrepared && cb != null)
				cb();
			return false;
		}

		[HarmonyPriority(int.MaxValue)]
		static void Prefix(
			LevelCollectionTableView __instance, TableView ____tableView,
			ref IPreviewBeatmapLevel[] previewBeatmapLevels, HashSet<string> favoriteLevelIds, ref bool beatmapLevelsAreSorted
		) {
			// If SetData is called with the literal same maplist as before we might as well ignore it
			if(previewBeatmapLevels == lastInMapList) {
				previewBeatmapLevels = lastOutMapList;
				return;
			}

			// Playlistlib has its own custom wrapping class for Playlists so it can properly track duplicates, so we need to use its collection
			if(HookSelectedCollection.lastSelectedCollection != null && PlaylistsUtil.hasPlaylistLib)
				previewBeatmapLevels = PlaylistsUtil.GetLevelsForLevelCollection(HookSelectedCollection.lastSelectedCollection)
					//TODO: For now I need to explicitly cast due to a bug in PlaylistLib, remove when not needed any more
					?.Cast<IPreviewBeatmapLevel>().ToArray() ?? previewBeatmapLevels;

			lastInMapList = previewBeatmapLevels;
			var _isSorted = beatmapLevelsAreSorted;
			recallLast = (overrideData) => __instance.SetData(overrideData ?? lastInMapList, favoriteLevelIds, _isSorted);

			//Console.WriteLine("=> {0}", new System.Diagnostics.StackTrace().ToString());

			// If this is true the default Alphabet scrollbar is processed / shown - We dont want that when we use a custom filter
			if(sorter?.isReady == true)
				beatmapLevelsAreSorted = false;

			if(PrepareStuffIfNecessary(() => Refresh(true))) {
				Plugin.Log.Debug(string.Format("Stuff isnt ready yet... Preparing it and then reloading list: Sorter {0}, Filter {1}", sorter?.isReady, filter?.isReady));
			}

			XD.FunnyNull(FilterUI.persistentNuts._filterLoadingIndicator)?.gameObject.SetActive(false);

			if(asyncPreprocessed != null) {
				previewBeatmapLevels = asyncPreprocessed;
				asyncPreprocessed = null;
#if TRACE
				Plugin.Log.Notice("Used Async-Prefiltered");
#endif
				return;
			}

			// Passing these explicitly for thread safety
			FilterWrapper(ref previewBeatmapLevels);
		}


		static List<KeyValuePair<string, int>> customLegend = null;
		static void Postfix(TableView ____tableView, AlphabetScrollbar ____alphabetScrollbar, IPreviewBeatmapLevel[] previewBeatmapLevels) {
			lastOutMapList = previewBeatmapLevels;

			// Basegame already handles cleaning up the legend etc
			if(customLegend == null)
				return;

			/*
			 * We essentially gotta double-init the alphabet scrollbar because basegame
			 * made the great decision to unnecessarily lock down the scrollbar to only
			 * use characters, not strings
			 */
			____alphabetScrollbar.SetData(customLegend.Select(x => new AlphabetScrollInfo.Data(x.Key[0], x.Value)).ToArray());

			// Now that all labels are there we can insert the text we want there...
			var x = ReflectionUtil.GetField<List<TextMeshProUGUI>, AlphabetScrollbar>(____alphabetScrollbar, "_texts");
			for(var i = 0; i < customLegend.Count; i++)
				x[i].text = customLegend[i].Key;

			customLegend = null;

			// Move the table a bit to the right to accomodate for alphabet scollbar
			((RectTransform)____tableView.transform).offsetMin = new Vector2(((RectTransform)____alphabetScrollbar.transform).rect.size.x + 1f, 0f);
			____alphabetScrollbar.gameObject.SetActive(true);
		}
	}
}
