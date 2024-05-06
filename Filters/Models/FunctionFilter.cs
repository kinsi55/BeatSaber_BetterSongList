using System;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.FilterModels {
	public class FunctionFilter : IFilter {
		public bool isReady => true;

		Func<BeatmapLevel, bool> valueProvider;

		public bool GetValueFor(BeatmapLevel level) => valueProvider(level);

		public Task Prepare(CancellationToken cancelToken) => Task.CompletedTask;

		public FunctionFilter(Func<BeatmapLevel, bool> valueProvider) {
			this.valueProvider = valueProvider;
		}
	}
}
