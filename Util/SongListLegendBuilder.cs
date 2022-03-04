using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSongList.Util {
	public static class SongListLegendBuilder {
		public static IEnumerable<KeyValuePair<string, int>> BuildFor(IPreviewBeatmapLevel[] beatmaps, Func<IPreviewBeatmapLevel, string> displayValueTransformer, int entryLengthLimit = 6, int valueLimit = 28) {
			var x = beatmaps
				.Select((x, i) => new KeyValuePair<string, int>(displayValueTransformer(x), i))
				.Where(x => x.Key != null)
				.GroupBy(x => x.Key.ToUpperInvariant())
				.ToArray();

			var amt = Math.Min(valueLimit, x.Length);

			if(amt <= 1)
				yield break;

			for(var i = 0; i < amt; i++) {
				var bmi = (int)Math.Round(((float)(x.Length - 1) / (amt - 1)) * i);

				var transformedResult = x[bmi].Key;

				if(transformedResult.Length > entryLengthLimit)
					transformedResult = transformedResult.Substring(0, entryLengthLimit);

				yield return new KeyValuePair<string, int>(transformedResult, x[bmi].First().Value);
			}
		}
	}
}
