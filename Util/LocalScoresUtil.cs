using System.Collections.Generic;
using UnityEngine;

namespace BetterSongList.Util {
	public static class LocalScoresUtil {

		public static PlayerDataModel playerDataModel;

		public static bool hasScores => playerDataModel != null;

		public static void Load() {
			playerDataModel = Object.FindObjectOfType<PlayerDataModel>();
		}

		public static bool HasLocalScore(string levelId) {
			return playerDataModel.playerData.levelsStatsData.Find(x => x.validScore && x.levelID == levelId) != null;
		}

		static HashSet<IPreviewBeatmapLevel> playedMaps = new HashSet<IPreviewBeatmapLevel>();
		public static bool HasLocalScore(IPreviewBeatmapLevel level) {
			if(playedMaps.Contains(level))
				return true;

			if(HasLocalScore(level.levelID)) {
				playedMaps.Add(level);
				return true;
			}

			return false;
		}
	}
}
