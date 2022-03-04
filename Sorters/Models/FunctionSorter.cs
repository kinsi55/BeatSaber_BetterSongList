using BetterSongList.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace BetterSongList.SortModels {
	public class PrimitiveFunctionSorter : ISorterPrimitive {
		public bool isReady => true;

		Func<IPreviewBeatmapLevel, float?> sortValueGetter;

		public Task Prepare(CancellationToken cancelToken) => Task.CompletedTask;

		public float? GetValueFor(IPreviewBeatmapLevel x) => sortValueGetter(x);

		public PrimitiveFunctionSorter(Func<IPreviewBeatmapLevel, float?> sortValueGetter) {
			this.sortValueGetter = sortValueGetter;
		}
	}

	public sealed class PrimitiveFunctionSorterWithLegend : PrimitiveFunctionSorter, ISorterWithLegend {
		Func<IPreviewBeatmapLevel, string> legendBuilder = null;
		public PrimitiveFunctionSorterWithLegend(Func<IPreviewBeatmapLevel, float?> sortValueGetter, Func<IPreviewBeatmapLevel, string> legendBuilder) : base(sortValueGetter) {
			this.legendBuilder = legendBuilder;
		}

		public IEnumerable<KeyValuePair<string, int>> BuildLegend(IPreviewBeatmapLevel[] levels) {
			try {
				return SongListLegendBuilder.BuildFor(levels, legendBuilder);
			} catch(Exception ex) {
				Plugin.Log.Debug("Building legend failed:");
				Plugin.Log.Debug(ex);
			}
			return null;
		}
	}

	public class ComparableFunctionSorter : ISorter, ISorterCustom, IComparer<IPreviewBeatmapLevel> {
		public bool isReady => true;

		Func<IPreviewBeatmapLevel, IPreviewBeatmapLevel, int> sortValueGetter;

		public Task Prepare(CancellationToken cancelToken) => Task.CompletedTask;

		public void DoSort(ref IEnumerable<IPreviewBeatmapLevel> levels, bool ascending) {
			levels = ascending ?
				levels.OrderBy(x => x, this) :
				levels.OrderByDescending(x => x, this);
		}

		public int Compare(IPreviewBeatmapLevel x, IPreviewBeatmapLevel y) => sortValueGetter(x, y);

		public ComparableFunctionSorter(Func<IPreviewBeatmapLevel, IPreviewBeatmapLevel, int> sortValueGetter) {
			this.sortValueGetter = sortValueGetter;
		}
	}

	public sealed class ComparableFunctionSorterWithLegend : ComparableFunctionSorter, ISorterWithLegend {
		Func<IPreviewBeatmapLevel, string> legendBuilder = null;
		public ComparableFunctionSorterWithLegend(
			Func<IPreviewBeatmapLevel, IPreviewBeatmapLevel, int> sortValueGetter, 
			Func<IPreviewBeatmapLevel, string> legendBuilder) : base(sortValueGetter) 
		{
			this.legendBuilder = legendBuilder;
		}

		public IEnumerable<KeyValuePair<string, int>> BuildLegend(IPreviewBeatmapLevel[] levels) {
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
