namespace BetterSongList.Util {
	static class BeatmapsUtil {

		public static string GetHashOfLevel(BeatmapLevel level) {
			return level == null ? null : GetHashOfLevelId(level.levelID);
		}
		
		public static string GetHashOfBeatmapKey(BeatmapKey key) {
			return GetHashOfLevelId(key.levelId);
		}
		
		private static string GetHashOfLevelId(string id) {
			if(id.Length < 53)
				return null;

			if(id[12] != '_') // custom_level_<hash, 40 chars>
				return null;

			return id.Substring(13, 40);
		}

		public static int GetCharacteristicFromDifficulty(BeatmapKey diff) {
			var d = diff.beatmapCharacteristic.sortingOrder;

			if(d > 4)
				return 0;

			// 360 and 90 are "flipped" as far as the enum goes
			if(d == 3)
				d = 4;
			else if(d == 4)
				d = 3;

			return (int)d + 1;
		}

		public static string ConcatMappers(string[] allmappers) {
			if(allmappers.Length == 1)
				return allmappers[0];

			return string.Join(" ", allmappers);
		}
	}
}
