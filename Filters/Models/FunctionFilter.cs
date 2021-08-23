using System;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.FilterModels {
	class FunctionFilter : IFilter {
		public bool isReady => true;

		Func<IPreviewBeatmapLevel, bool> valueProvider;

		public bool GetValueFor(IPreviewBeatmapLevel level) => valueProvider(level);

		public Task Prepare(CancellationToken cancelToken) => Task.CompletedTask;

		public FunctionFilter(Func<IPreviewBeatmapLevel, bool> valueProvider) {
			this.valueProvider = valueProvider;
		}
	}
}
