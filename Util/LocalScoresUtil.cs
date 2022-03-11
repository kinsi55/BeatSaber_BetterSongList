using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BetterSongList.Util {
	public static class LocalScoresUtil {
		public static PlayerDataModel playerDataModel { get; private set; }
		static HashSet<string> playedMaps = new HashSet<string>(500);

		public static bool hasScores => playerDataModel != null;

		public static void Load() {
			playerDataModel = Object.FindObjectOfType<PlayerDataModel>();

			foreach(var x in playerDataModel?.playerData?.levelsStatsData) {
				if(x.validScore && !playedMaps.Contains(x.levelID))
					playedMaps.Add(x.levelID);
			}
		}

		public static bool HasLocalScore(string levelId) {
			if(playedMaps.Contains(levelId))
				return true;

			var l = playerDataModel.playerData.levelsStatsData;
			for(var i = l.Count; i-- > playedMaps.Count;)
				if(l[i].validScore && l[i].levelID == levelId)
					return true;

			return false;
		}

		public static bool HasLocalScore(IPreviewBeatmapLevel level) => HasLocalScore(level.levelID);
	}
}
