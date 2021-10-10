using BetterSongList.SortModels;
using BetterSongList.Util;
using SongDetailsCache.Structs;
using System;

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

		public static readonly ISorter alphabeticalAuthor = new FunctionSorterWithLegend(
			(songa, songb) => string.Compare(songa.songAuthorName, songb.songAuthorName),
			(song) => song.songAuthorName.Length > 0 ? song.songAuthorName.Substring(0, 1) : null
		);

		public static readonly ISorter alphabeticalMapper = new FunctionSorterWithLegend(
			(songa, songb) => string.Compare(songa.levelAuthorName, songb.levelAuthorName),
			(song) => song.levelAuthorName.Length > 0 ? song.levelAuthorName.Substring(0, 1) : null
		);
		public static readonly ISorter downloadTime = new FolderDateSorter();

		static float? StarsProcessor(object xx) {
			var x = (Song)xx;
			if(x.rankedStatus != RankedStatus.Ranked)
				return null;

			float ret = 0;

			for(var i = (int)x.diffOffset; i < x.diffOffset + x.diffCount; i++) {
				var diff = ((SongDetailsCache.SongDetails)SongDetailsUtil.instance).difficulties[i];

				if(diff.stars == 0)
					continue;

				if(ret == 0) {
					ret = diff.stars;
					continue;
				}

				if(Config.Instance.SortAsc) {
					if(ret < diff.stars)
						continue;
				} else if(ret > diff.stars) {
					continue;
				}

				ret = diff.stars;
			}

			return ret == 0 ? (float?)null : ret;
		}

		public static readonly ISorter stars = new BasicSongDetailsSorterWithLegend(StarsProcessor, x => {
			var y = StarsProcessor((Song)x);

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
			x => ((Song)x).uploadTimeUnix,
		x => {
			var d = ((Song)x).uploadTime;
			return d.ToString($"Q{GetQuarter(d)} yy");
		});
	}
}
