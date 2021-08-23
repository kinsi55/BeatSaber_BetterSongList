namespace BetterSongList.Util {
	static class BeatmapsUtil {
		public static string GetHashOfPreview(IPreviewBeatmapLevel preview) {
			if(preview.levelID.Length < 53)
				return null;

			if(preview.levelID[12] != '_') // custom_level_<hash, 40 chars>
				return null;

			return preview.levelID.Substring(13, 40);
		}

		public static int GetCharacteristicFromDifficulty(IDifficultyBeatmap diff) {
			var d = diff.parentDifficultyBeatmapSet?.beatmapCharacteristic.sortingOrder;

			if(d == null || d > 4)
				return 0;

			// 360 and 90 are "flipped" as far as the enum goes
			if(d == 3)
				d = 4;
			else if(d == 4)
				d = 3;

			return (int)d + 1;
		}
	}
}
