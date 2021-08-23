using BetterSongList.SortModels;
using System;
using System.Linq;

namespace BetterSongList {
	static class SortMethods {
		public static readonly ISorter alphabeticalSongname = new FunctionSorterWithLegend(
			(songa, songb) => string.Compare(songa.songName, songb.songName),
			(song) => song.songName.Length > 0 ? song.songName.Substring(0, 1) : null
		);

		public static readonly ISorter bpm = new FunctionSorterWithLegend(
			(songa, songb) => ComparerHelpers.CompareFloats(songa.beatsPerMinute, songb.beatsPerMinute),
			(song) => Math.Round(song.beatsPerMinute).ToString()
		);

		public static readonly ISorter alphabeticalMapper = new FunctionSorterWithLegend(
			(songa, songb) => string.Compare(songa.levelAuthorName, songb.levelAuthorName),
			(song) => song.levelAuthorName.Length > 0 ? song.levelAuthorName.Substring(0, 1) : null
		);
		public static readonly ISorter downloadTime = new FolderDateSorter();

		static float? StartsProcessor(SongDetailsCache.Structs.Song x) {
			var y = x.difficulties;

			float retVal = Config.Instance.SortAsc ? y.Min(x => x.stars) : y.Max(x => x.stars);

			return retVal <= 0 ? (float?)null : retVal;
		}

		public static readonly ISorter stars = new BasicSongDetailsSorterWithLegend(StartsProcessor, x => {
			var y = StartsProcessor(x);

			if(y == null)
				return null;

			return ((float)y).ToString("0.0");
		});

		public static readonly ISorter songLength = new FunctionSorterWithLegend(
			(songa, songb) => ComparerHelpers.CompareFloats(songa.songDuration, songb.songDuration),
			(song) => (song.songDuration < 60 ? "<1" : Math.Round(song.songDuration / 60).ToString()) + " min"
		);

		static int GetQuarter(DateTime date) {
			if(date.Month > 9)
				return 4;
			else if(date.Month > 6)
				return 3;
			else if(date.Month > 3)
				return 2;

			return 1;
		}

		public static readonly ISorter beatSaverDate = new BasicSongDetailsSorterWithLegend(
			x => x.uploadTimeUnix,
		x => {
			var d = x.uploadTime;
			return d.ToString($"Q{GetQuarter(d)} yy");
		});
	}
}
