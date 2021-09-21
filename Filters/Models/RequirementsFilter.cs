using BetterSongList.Util;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.FilterModels {
	class RequirementsFilter : IFilter {
		public bool isReady => SongCore.Loader.AreSongsLoaded;

		static TaskCompletionSource<bool> wipTask = null;
		static bool inited = false;
		public Task Prepare(CancellationToken cancelToken) => Prepare(cancelToken, false);
		public Task Prepare(CancellationToken cancelToken, bool fullReload) {
			if(wipTask?.Task.IsCompleted != false)
				wipTask = new TaskCompletionSource<bool>();

			if(!inited && (inited = true))
				SongCore.Loader.SongsLoadedEvent += (_, _2) => wipTask.SetResult(true);

			return wipTask.Task;
		}

		public bool GetValueFor(IPreviewBeatmapLevel level) {
			var mid = BeatmapsUtil.GetHashOfPreview(level);

			if(mid == null)
				return false;

			return SongCore.Collections.RetrieveExtraSongData(mid)?
				._difficulties.Any(x => x.additionalDifficultyData._requirements.Any(x => x.Length != 0)) == true;
		}
	}
}
