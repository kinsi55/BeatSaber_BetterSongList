using BetterSongList.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.SortModels {
	class FunctionSorter : ISorter {
		public bool isReady => true;

		Func<IPreviewBeatmapLevel, IPreviewBeatmapLevel, int> sorter;

		public Task Prepare(CancellationToken cancelToken) => Task.CompletedTask;

		public int Compare(IPreviewBeatmapLevel x, IPreviewBeatmapLevel y) => Config.Instance.SortAsc ? sorter(x, y) : -sorter(x, y);

		public FunctionSorter(Func<IPreviewBeatmapLevel, IPreviewBeatmapLevel, int> sorter) {
			this.sorter = sorter;
		}
	}
	class FunctionSorterWithLegend : FunctionSorter, ISorterWithLegend {
		Func<IPreviewBeatmapLevel, string> legendBuilder = null;
		public FunctionSorterWithLegend(Func<IPreviewBeatmapLevel, IPreviewBeatmapLevel, int> sorter, Func<IPreviewBeatmapLevel, string> legendBuilder) : base(sorter) {
			this.legendBuilder = legendBuilder;
		}

		public List<KeyValuePair<string, int>> BuildLegend(IPreviewBeatmapLevel[] levels) {
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
