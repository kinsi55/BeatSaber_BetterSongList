using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.FilterModels {
#if DEBUG
	public
#endif
	interface IFilter {
		public bool isReady { get; }
		Task Prepare(CancellationToken cancelToken);
		bool GetValueFor(IPreviewBeatmapLevel level);
	}
}
