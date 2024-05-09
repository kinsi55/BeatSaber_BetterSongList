using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BetterSongList.Util;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace BetterSongList.HarmonyPatches.UI {

	[HarmonyPatch(typeof(LevelParamsPanel), nameof(LevelParamsPanel.obstaclesCount), MethodType.Setter)]
	static class WarnCrouchWalls {

		static CancellationTokenSource crouchWallTokenSource = null;

		static readonly Dictionary<BeatmapKey, bool> crouchWallCache = new Dictionary<BeatmapKey, bool>();

		// For some reason, hooking these two methods doesn't work.
		// Even though they are the only methods that set the obstacle count text,
		// our text assignment in POSTFIX is still being overwritten.
		// [HarmonyTargetMethods]
		// static IEnumerable<MethodInfo> TargetMethods() {
		// 	yield return AccessTools.Method(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.CalculateAndSetContent));
		// 	yield return AccessTools.Method(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.SetContent), new Type[]{});
		// }

		[HarmonyPostfix]
		static void Postfix(LevelParamsPanel __instance, int value) {
			crouchWallTokenSource?.Cancel();
			crouchWallTokenSource = new CancellationTokenSource();

			if(value <= 0)
				return;

			var detailView = ExtraLevelParams.lastInstance;
			if(detailView == null)
				return;

			var obstaclesText = __instance._obstaclesCountText;
			obstaclesText.fontStyle = FontStyles.Italic;

			// Crouchwalls HAHABALLS
			if(Config.Instance.ShowWarningIfMapHasCrouchWallsBecauseMappersThinkSprinklingThemInRandomlyIsFun) {
				Task.Run(async () => {
					var token = crouchWallTokenSource.Token;
					await DetectAndWarnCrouchWalls(detailView, obstaclesText, value, token);
					if(token.IsCancellationRequested)
						Plugin.Log.Debug("CrouchWall detection cancelled");
				});
			}
		}

		static async Task DetectAndWarnCrouchWalls(StandardLevelDetailView detailView, TMP_Text obstaclesText, int obstaclesCount, CancellationToken token) {
			var beatmapKey = detailView.beatmapKey;
			var beatmapLevel = detailView._beatmapLevel;

			Plugin.Log.Info("Detecting CrouchWalls");
			if(!crouchWallCache.TryGetValue(beatmapKey, out var hasCrouchWalls))
			{
				var beatmapLevelsModel = detailView._beatmapLevelsModel;
				var beatmapDataLoader = detailView._beatmapDataLoader;

				var loadResult = await beatmapLevelsModel.LoadBeatmapLevelDataAsync(beatmapKey.levelId, token);
				if(loadResult.isError)
				{
					Plugin.Log.Error("Failed to get BeatmapLevelData.");
					return;
				}

				if(token.IsCancellationRequested)
					return;

				var beatmapLevelData = loadResult.beatmapLevelData!;
				var beatmapData = await beatmapDataLoader.LoadBeatmapDataAsync(
					beatmapLevelData,
					beatmapKey,
					beatmapLevel.beatsPerMinute,
					false,
					null,
					null,
					null,
					false);

				if(token.IsCancellationRequested)
					return;

				// We have the entire map now

				var obstacles = beatmapData.GetBeatmapDataItems<ObstacleData>(0);
				hasCrouchWalls = BeatmapPatternDetection.CheckForCrouchWalls(obstacles);
				crouchWallCache[beatmapKey] = hasCrouchWalls;
			}

			if(!token.IsCancellationRequested && hasCrouchWalls) {
				// Plugin.Log.Warn("There are CrouchWalls!");
				SharedCoroutineStarter.instance.StartCoroutine(ShowCrouchWallsWarning(obstaclesText, obstaclesCount));
			}
		}

		static IEnumerator ShowCrouchWallsWarning(TMP_Text obstaclesText, int obstaclesCount) {
			yield return new WaitForEndOfFrame();
			obstaclesText.richText = true;
			obstaclesText.fontStyle = FontStyles.Normal;
			obstaclesText.text = $"<i>{obstaclesCount}</i> <b><size=3.3><color=#FF0>⚠</color></size></b>";
		}
	}
}