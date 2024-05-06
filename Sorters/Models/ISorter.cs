using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.SortModels {
	public interface ISorter {
		public bool isReady { get; }
		Task Prepare(CancellationToken cancelToken);
	}

	public interface ISorterWithLegend : ISorter {
		public IEnumerable<KeyValuePair<string, int>> BuildLegend(BeatmapLevel[] levels);
	}

	public interface ISorterCustom : ISorter {
		public void DoSort(ref IEnumerable<BeatmapLevel> levels, bool ascending);
	}

	public interface ISorterPrimitive : ISorter {
		public float? GetValueFor(BeatmapLevel level);
	}
}
