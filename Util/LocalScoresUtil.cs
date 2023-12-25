using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace BetterSongList.Util {
	public static class LocalScoresUtil {
		public static PlayerDataModel playerDataModel { get; private set; }
		static HashSet<string> playedMaps = new HashSet<string>(500);

		public static bool hasScores => playerDataModel != null;

		public static void Load() {
			playerDataModel = Object.FindObjectOfType<PlayerDataModel>();

			foreach(var x in playerDataModel?.playerData?.levelsStatsData) {
				if(!x.Value.validScore)
					continue;

				playedMaps.Add(x.Key.songId);
			}
		}

		[HarmonyPatch(typeof(PlayerLevelStatsData), nameof(PlayerLevelStatsData.UpdateScoreData))]
		static class InterceptNewScores {
			static void Prefix(bool ____validScore, string ____levelID) {
				// Will become valid after this UpdateScoreData() call
				if(!____validScore)
					playedMaps.Add(____levelID);
			}
		}

		public static bool HasLocalScore(string levelId) => playedMaps.Contains(levelId);

		public static bool HasLocalScore(IPreviewBeatmapLevel level) => HasLocalScore(level.levelID);
	}
}
