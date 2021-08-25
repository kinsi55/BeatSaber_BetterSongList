using BetterSongList.FilterModels;
using BetterSongList.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSongList.Filters.Models {
	class UnplayedFilter : IFilter {
		public bool isReady => LocalScoresUtil.hasScores;

		public Task Prepare(CancellationToken cancelToken) {
			var t = new TaskCompletionSource<bool>();

			IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => {
				try {
					LocalScoresUtil.Load();
				} catch {}
				t.SetResult(true);
			});
			return t.Task;
		}

		public string GetUnavailabilityReason() => SongDetailsUtil.GetUnavailabilityReason();

		public bool GetValueFor(IPreviewBeatmapLevel level) {
			if(SongDetailsUtil.instance == null)
				return true;

			return !LocalScoresUtil.HasLocalScore(level);
		}
	}
}
