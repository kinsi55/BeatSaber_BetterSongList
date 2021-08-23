using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSongList.Util {
	static class SongListLegendBuilder {
		public static List<KeyValuePair<string, int>> BuildFor(IPreviewBeatmapLevel[] beatmaps, Func<IPreviewBeatmapLevel, string> displayValueTransformer, int entryLengthLimit = 6, int valueLimit = 28) {
			var x = beatmaps
				.Select((x, i) => new KeyValuePair<string, int>(displayValueTransformer(x), i))
				.Where(x => x.Key != null)
				.GroupBy(x => x.Key)
				.ToList();

			var amt = Math.Min(valueLimit, x.Count);

			if(amt <= 1)
				return null;

			var outList = new List<KeyValuePair<string, int>>(amt);

			for(var i = 0; i < amt; i++) {
				var bmi = (int)Math.Round(((float)(x.Count - 1) / (amt - 1)) * i);

				var transformedResult = x[bmi].Key;

				if(transformedResult.Length > entryLengthLimit)
					transformedResult = transformedResult.Substring(0, entryLengthLimit);

				outList.Add(new KeyValuePair<string, int>(transformedResult, x[bmi].First().Value));
			}

			return outList;
		}
	}
}
