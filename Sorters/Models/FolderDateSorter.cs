using BetterSongList.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.SortModels {
	class FolderDateSorter : ISorterWithLegend {
		public bool isReady => wipTask == null && songTimes != null;

		/*
		 * TODO: For now, I need to use LevelId : int because I have to cast Playlists in LevelCollectionTableSet
		 * once that is gone (Fixed BS Playlist Lib) I can go back to IPreviewBeatmapLevel : int
		 */
		static ConcurrentDictionary<string, int> songTimes = null;

		static TaskCompletionSource<bool> wipTask = null;
		static bool isLoading = false;
		public Task Prepare(CancellationToken cancelToken) => Prepare(cancelToken, false);
		public Task Prepare(CancellationToken cancelToken, bool fullReload) {
			if(songTimes == null) {
				songTimes = new ConcurrentDictionary<string, int>();
				SongCore.Loader.SongsLoadedEvent += (_, _2) => Prepare(CancellationToken.None);
			}

			wipTask ??= new TaskCompletionSource<bool>();

			if(!SongCore.Loader.AreSongsLoaded || SongCore.Loader.AreSongsLoading)
				return wipTask.Task;

			if(!isLoading) {
				isLoading = true;
				Task.Run(() => {
					var xy = new System.Diagnostics.Stopwatch();
					xy.Start();

					foreach(var song in 
						SongCore.Loader.BeatmapLevelsModelSO
						.allLoadedBeatmapLevelPackCollection.beatmapLevelPacks.Where(x => x is SongCore.OverrideClasses.SongCoreCustomBeatmapLevelPack)
						.SelectMany(x => x.beatmapLevelCollection.beatmapLevels)
						.Cast<CustomPreviewBeatmapLevel>()
					) {
						if(songTimes.ContainsKey(song.levelID) && !fullReload)
							continue;

						songTimes[song.levelID] = (int)File.GetCreationTimeUtc(song.customLevelPath + Path.DirectorySeparatorChar + "info.dat").ToUnixTime();
					}

					Plugin.Log.Debug(string.Format("Getting SongFolder dates took {0}ms", xy.ElapsedMilliseconds));
					wipTask.TrySetResult(true);
					wipTask = null;
					isLoading = false;
				});
			}

			return wipTask.Task;
		}

		int GetValueFor(IPreviewBeatmapLevel level) {
			if(songTimes.TryGetValue(level.levelID, out var oVal))
				return Config.Instance.SortAsc ? oVal : -oVal;

			return int.MaxValue;
		}

		public int Compare(IPreviewBeatmapLevel x, IPreviewBeatmapLevel y) => ComparerHelpers.CompareInts(GetValueFor(x), GetValueFor(y));

		const float MONTH_SECS = 1f / (60 * 60 * 24 * 30.4f);
		public List<KeyValuePair<string, int>> BuildLegend(IPreviewBeatmapLevel[] levels) {
			var curUtc = (int)DateTime.UtcNow.ToUnixTime();

			return SongListLegendBuilder.BuildFor(levels, (level) => {
				if(!songTimes.ContainsKey(level.levelID))
					return null;

				var months = (curUtc - songTimes[level.levelID]) * MONTH_SECS;

				if(months < 1)
					return "<1 M";

				return Math.Round(months) + " M";
			});
		}
	}
}
