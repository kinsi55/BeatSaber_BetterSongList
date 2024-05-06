using BetterSongList.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace BetterSongList.SortModels {
	public class PrimitiveFunctionSorter : ISorterPrimitive {
		public bool isReady => true;

		Func<BeatmapLevel, float?> sortValueGetter;

		public Task Prepare(CancellationToken cancelToken) => Task.CompletedTask;

		public float? GetValueFor(BeatmapLevel x) => sortValueGetter(x);

		public PrimitiveFunctionSorter(Func<BeatmapLevel, float?> sortValueGetter) {
			this.sortValueGetter = sortValueGetter;
		}
	}

	public class PrimitiveFunctionSorterWithLegend : PrimitiveFunctionSorter, ISorterWithLegend {
		Func<BeatmapLevel, string> legendBuilder = null;
		public PrimitiveFunctionSorterWithLegend(Func<BeatmapLevel, float?> sortValueGetter, Func<BeatmapLevel, string> legendBuilder) : base(sortValueGetter) {
			this.legendBuilder = legendBuilder;
		}

		public IEnumerable<KeyValuePair<string, int>> BuildLegend(BeatmapLevel[] levels) {
			try {
				return SongListLegendBuilder.BuildFor(levels, legendBuilder);
			} catch(Exception ex) {
				Plugin.Log.Debug("Building legend failed:");
				Plugin.Log.Debug(ex);
			}
			return null;
		}
	}

	public class ComparableFunctionSorter : ISorter, ISorterCustom, IComparer<BeatmapLevel> {
		public bool isReady => true;

		Func<BeatmapLevel, BeatmapLevel, int> sortValueGetter;

		public Task Prepare(CancellationToken cancelToken) => Task.CompletedTask;

		public void DoSort(ref IEnumerable<BeatmapLevel> levels, bool ascending) {
			levels = ascending ?
				levels.OrderBy(x => x, this) :
				levels.OrderByDescending(x => x, this);
		}

		public int Compare(BeatmapLevel x, BeatmapLevel y) => sortValueGetter(x, y);

		public ComparableFunctionSorter(Func<BeatmapLevel, BeatmapLevel, int> sortValueGetter) {
			this.sortValueGetter = sortValueGetter;
		}
	}

	public class ComparableFunctionSorterWithLegend : ComparableFunctionSorter, ISorterWithLegend {
		Func<BeatmapLevel, string> legendBuilder = null;
		public ComparableFunctionSorterWithLegend(
			Func<BeatmapLevel, BeatmapLevel, int> sortValueGetter,
			Func<BeatmapLevel, string> legendBuilder) : base(sortValueGetter)
		{
			this.legendBuilder = legendBuilder;
		}

		public IEnumerable<KeyValuePair<string, int>> BuildLegend(BeatmapLevel[] levels) {
			try {
				return SongListLegendBuilder.BuildFor(levels, legendBuilder);
			} catch(Exception ex) {
				Plugin.Log.Debug("Building legend failed:");
				Plugin.Log.Debug(ex);
			}
			return null;
		}
	}
}
