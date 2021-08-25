namespace BetterSongList.Util {
	static class BeatmapsUtil {
		public static string GetHashOfPreview(IPreviewBeatmapLevel preview) {
			if(preview is CustomPreviewBeatmapLevel)
				return preview.levelID.Substring(13, 40);

			return null;
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
