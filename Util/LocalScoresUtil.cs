using System.Linq;
using UnityEngine;

namespace BetterSongList.Util {
	public static class LocalScoresUtil {

		public static PlayerDataModel playerDataModel;

		public static bool hasScores => playerDataModel != null;

		public static void Load() {
			playerDataModel = Object.FindObjectOfType<PlayerDataModel>();
		}

		public static bool HasLocalScore(string levelId) {
			return playerDataModel.playerData.levelsStatsData.Find(x => x.levelID == levelId && x.validScore) != null;
		}

		public static bool HasLocalScore(IPreviewBeatmapLevel levelId) {
			return HasLocalScore(levelId.levelID);
		}
	}
}
