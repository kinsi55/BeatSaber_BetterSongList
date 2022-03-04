using BetterSongList.Interfaces;
using BetterSongList.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.SortModels {
	public class BasicSongDetailsSorterWithLegend : ISorterWithLegend, ISorterPrimitive, IAvailabilityCheck {
		public bool isReady => SongDetailsUtil.finishedInitAttempt;

#nullable enable
		Func<object, float?> sortValueTransformer;
		Func<object, string?> legendValueTransformer;

		public BasicSongDetailsSorterWithLegend(Func<object, float?> sortValueTransformer, Func<object, string?> legendValueTransformer) {
			this.sortValueTransformer = sortValueTransformer;
			this.legendValueTransformer = legendValueTransformer;
		}
#nullable disable
		public BasicSongDetailsSorterWithLegend(Func<object, float?> sortValueTransformer) {
			this.sortValueTransformer = sortValueTransformer;
			this.legendValueTransformer = (x) => sortValueTransformer(x).ToString();
		}

		public IEnumerable<KeyValuePair<string, int>> BuildLegend(IPreviewBeatmapLevel[] levels) {
			if(SongDetailsUtil.songDetails == null)
				return null;

			try {
				return SongListLegendBuilder.BuildFor(levels, (level) => {
					//if(!GetSongFromBeatmap(level, out var song))
					//	return null;

					var h = BeatmapsUtil.GetHashOfPreview(level);
					if(h == null || !SongDetailsUtil.songDetails.instance.songs.FindByHash(h, out var song))
						return "N/A";

					return legendValueTransformer(song);
				});
			} catch(Exception ex) {
				Plugin.Log.Debug("Building legend failed:");
				Plugin.Log.Debug(ex);
			}
			return null;
		}

		public float? GetValueFor(IPreviewBeatmapLevel x) {
			// Make N/A always end up at the bottom in either sort direction
			if(SongDetailsUtil.songDetails == null)
				return null;

			float? _Get(IPreviewBeatmapLevel x) {
				var h = BeatmapsUtil.GetHashOfPreview(x);
				if(h == null || !SongDetailsUtil.songDetails.instance.songs.FindByHash(h, out var song))
					return null;

				return sortValueTransformer(song);
			}

			return _Get(x);
		}

		public string GetUnavailabilityReason() => SongDetailsUtil.GetUnavailabilityReason();

		public Task Prepare(CancellationToken cancelToken) {
			if(!isReady)
				return SongDetailsUtil.TryGet();

			return Task.CompletedTask;
		}
	}
}
