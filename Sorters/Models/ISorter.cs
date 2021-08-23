using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.SortModels {
	interface ISorter : IComparer<IPreviewBeatmapLevel> {
		public bool isReady { get; }
		Task Prepare(CancellationToken cancelToken);
	}

	interface ISorterWithLegend : ISorter {
		public List<KeyValuePair<string, int>> BuildLegend(IPreviewBeatmapLevel[] levels);
	}

	interface ISorterPrimitive {
		public void DoSort(ref IEnumerable<IPreviewBeatmapLevel> levels);
	}

	static class ComparerHelpers {
		public static int CompareFloats(float a, float b) {
			if(a == b) return 0;

			return a > b ? 1 : -1;
		}
		public static int CompareInts(int a, int b) {
			if(a == b) return 0;

			return a > b ? 1 : -1;
		}
	}
}
