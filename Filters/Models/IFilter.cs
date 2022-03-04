using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.FilterModels {
	public interface IFilter {
		public bool isReady { get; }
		Task Prepare(CancellationToken cancelToken);
		bool GetValueFor(IPreviewBeatmapLevel level);
	}
}
