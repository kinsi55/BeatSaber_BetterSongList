﻿using BetterSongList.Util;
using SongDetailsCache.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.SortModels {
	class BasicSongDetailsSorterWithLegend : ISorterWithLegend, ISorterPrimitive, IAvailabilityCheck {
		public bool isReady => SongDetailsUtil.finishedInitAttempt;

#nullable enable
		Func<Song, float?> sortValueTransformer;
		Func<Song, string?> legendValueTransformer;

		public BasicSongDetailsSorterWithLegend(Func<Song, float?> sortValueTransformer, Func<Song, string?> legendValueTransformer) {
			this.sortValueTransformer = sortValueTransformer;
			this.legendValueTransformer = legendValueTransformer;
		}
#nullable disable
		public BasicSongDetailsSorterWithLegend(Func<Song, float?> sortValueTransformer) {
			this.sortValueTransformer = sortValueTransformer;
			this.legendValueTransformer = (x) => sortValueTransformer(x).ToString();
		}

		public List<KeyValuePair<string, int>> BuildLegend(IPreviewBeatmapLevel[] levels) {
			if(SongDetailsUtil.instance == null)
				return null;

			try {
				return SongListLegendBuilder.BuildFor(levels, (level) => {
					//if(!GetSongFromBeatmap(level, out var song))
					//	return null;

					var h = BeatmapsUtil.GetHashOfPreview(level);
					if(h == null || !SongDetailsUtil.instance.songs.FindByHash(h, out var song))
						return null;

					return legendValueTransformer(song);
				});
			} catch(Exception ex) {
				Plugin.Log.Debug("Building legend failed:");
				Plugin.Log.Debug(ex);
			}
			return null;
		}

		public void DoSort(ref IEnumerable<IPreviewBeatmapLevel> levels) {
			if(SongDetailsUtil.instance == null)
				return;

			float Sorter(IPreviewBeatmapLevel x) {
				//if(!GetSongFromBeatmap(x, out var song))
				//	return Config.Instance.SortAsc ? 0 : 0;

				var h = BeatmapsUtil.GetHashOfPreview(x);
				if(h == null || !SongDetailsUtil.instance.songs.FindByHash(h, out var song))
					return Config.Instance.SortAsc ? 0 : 0;

				return sortValueTransformer(song) ?? 0;
			}

			if(Config.Instance.SortAsc) {
				levels = levels.OrderBy(Sorter);
			} else {
				levels = levels.OrderByDescending(Sorter);
			}
		}

		public int Compare(IPreviewBeatmapLevel x, IPreviewBeatmapLevel y) => throw new MissingMethodException();

		//TODO: Switch to this later when SongDetailsCache update with Song.none is established
		//bool GetSongFromBeatmap(IPreviewBeatmapLevel level, out Song song) {
		//	song = Song.none;
		//	var h = BeatmapsUtil.GetHashOfPreview(level);
		//	if(h == null)
		//		return false;

		//	return SongDetailsUtil.instance.songs.FindByHash(h, out song);
		//}
		public string GetUnavailabilityReason() => SongDetailsUtil.GetUnavailabilityReason();

		public Task Prepare(CancellationToken cancelToken) {
			if(!isReady)
				return SongDetailsUtil.TryGet();

			return Task.CompletedTask;
		}

	}
}