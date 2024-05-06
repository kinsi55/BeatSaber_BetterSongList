using BetterSongList.Interfaces;
using BetterSongList.SortModels;
using BetterSongList.Util;
using SongDetailsCache.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace BetterSongList {
	public static class SortMethods {
		public static readonly ISorter alphabeticalSongname = new ComparableFunctionSorterWithLegend(
			(songa, songb) => string.Compare(songa.songName, songb.songName),
			song => song.songName.Length > 0 ? song.songName.Substring(0, 1) : null
		);

		public static readonly ISorter bpm = new PrimitiveFunctionSorterWithLegend(
			song => song.beatsPerMinute,
			song => Math.Round(song.beatsPerMinute).ToString()
		);

		public static readonly ISorter alphabeticalMapper = new ComparableFunctionSorterWithLegend(
			(songa, songb) => {
				var songaAuthors = songa.allMappers.Concat(songa.allLighters).Distinct().Join();
				var songaButhors = songb.allMappers.Concat(songb.allLighters).Distinct().Join();
				return string.Compare(songaAuthors, songaButhors);
			},
			song => {
				var authors = song.allMappers.Concat(song.allLighters).Distinct().Join();
				return authors.Length > 0 ? authors.Substring(0, 1) : null;
			});
		public static readonly ISorter downloadTime = new FolderDateSorter();

		internal static float? StarsProcessor(object xx) {
			var x = (Song)xx;
			if(x.rankedStatus != RankedStatus.Ranked)
				return null;

			float ret = 0;

			for(var i = (int)x.diffOffset; i < x.diffOffset + x.diffCount; i++) {
				var diff = SongDetailsUtil.songDetails.instance.difficulties[i];

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

		const float funnyOptim = 1 / 60f;
		public static readonly ISorter songLength = new PrimitiveFunctionSorterWithLegend(
			song => song.songDuration,
			song => (song.songDuration < 60 ? "<1" : Math.Round(song.songDuration * funnyOptim).ToString()) + " min"
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


		internal static Dictionary<string, ISorter> methods = new Dictionary<string, ISorter>() {
			{ "Song Name", alphabeticalSongname },
			{ "Download Date", downloadTime },
			{ "Ranked Stars", stars },
			{ "Song Length", songLength },
			{ "BPM", bpm },
			{ "BeatSaver Date", beatSaverDate },
			{ "Default", null }
		};

		static bool Register(ITransformerPlugin sorter) {
			var name = sorter.name;

			if(name.Length > 20)
				throw new ArgumentException("The name of the Transformer cannot exceed 20 Characters");

			if(!Config.Instance.AllowPluginSortsAndFilters)
				return false;

			name = $"🔌{name}";

			methods.Add(name, sorter);

			return true;
		}

		public static bool RegisterPrimitiveSorter<T>(T sorter) where T : ISorterPrimitive, ITransformerPlugin => Register(sorter);
		public static bool RegisterCustomSorter<T>(T sorter) where T : ISorterCustom, ITransformerPlugin => Register(sorter);
	}
}
