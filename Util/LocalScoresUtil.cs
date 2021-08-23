using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterSongList.Util {
	public static class LocalScoresUtil {

		public static PlayerDataModel playerDataModel = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault();
		//static PlayerDataModel playerDataModel => _playerDataModel = XD.FunnyNull(_playerDataModel) ?? Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault();

		//public static bool has

		public static bool HasLocalScore(string levelId) {
			return playerDataModel.playerData.levelsStatsData.Find(x => x.levelID == levelId && x.validScore) != null;
		}

		public static bool HasLocalScore(IPreviewBeatmapLevel levelId) {
			return HasLocalScore(levelId.levelID);
		}
	}
}
