using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using BetterSongList.Util;
using HarmonyLib;
using MonoMod.Utils;
using TMPro;
using UnityEngine;

namespace BetterSongList.HarmonyPatches.UI {

	[HarmonyPatch]
	static class WarnCrouchWallsAsyncPatch {
		[HarmonyTargetMethod]
		static MethodInfo TargetMethod() {
			return AccessTools.Method(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.CalculateAndSetContent))
				.GetStateMachineTarget();
		}

		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var obstaclesCountSetter = AccessTools.PropertySetter(typeof(LevelParamsPanel), nameof(LevelParamsPanel.obstaclesCount));
			var found = false;
			
			// inject our crouch wall warning code after it sets the obstaclesCount
			foreach(CodeInstruction instruction in instructions)
			{
				if(instruction.opcode == OpCodes.Callvirt && instruction.operand is MethodInfo method && method == obstaclesCountSetter) {
					yield return instruction;  // after it sets the obstaclesCount
					yield return new CodeInstruction(OpCodes.Ldloc_1); // Load variable `standardLevelDetailView`
					yield return new CodeInstruction(OpCodes.Ldloc_2); // Load variable "BeatmapDataBasicInfo result3"
					yield return Transpilers.EmitDelegate<Action<StandardLevelDetailView, BeatmapDataBasicInfo>>((view, info) => 
						WarnCrouchWalls.DetectAndWarnCrouchWalls(view, info.obstaclesCount));
					found = true;
				} else {
					yield return instruction;
				}
			}

			if(!found) throw new Exception("Didn't find the call to obstaclesCount setter");
		}
	}

	[HarmonyPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.SetContent), new Type[]{})]
	static class WarnCrouchWalls {

		static CancellationTokenSource crouchWallTokenSource = null;

		static readonly Dictionary<BeatmapKey, bool> crouchWallCache = new Dictionary<BeatmapKey, bool>();
		
		[HarmonyPostfix]
		private static void Postfix(StandardLevelDetailView __instance) {
			var beatmapKey = __instance.beatmapKey;
			var beatmapLevel = __instance._beatmapLevel;
			var basicData = beatmapLevel.GetDifficultyBeatmapData(beatmapKey.beatmapCharacteristic, beatmapKey.difficulty);
			if (basicData == null)
				return;

			DetectAndWarnCrouchWalls(__instance, basicData.obstaclesCount);
		}
		
		internal static void DetectAndWarnCrouchWalls(StandardLevelDetailView detailView, int obstaclesCount) {
			crouchWallTokenSource?.Cancel();
			crouchWallTokenSource = new CancellationTokenSource();

			// Crouchwalls HAHABALLS
			if(Config.Instance.ShowWarningIfMapHasCrouchWallsBecauseMappersThinkSprinklingThemInRandomlyIsFun) {
				Task.Run(async () => {
					var token = crouchWallTokenSource.Token;
					await DetectAndWarnCrouchWalls(detailView, obstaclesCount, token);
					if(token.IsCancellationRequested)
						Plugin.Log.Debug("CrouchWall detection cancelled");
				});
			}
		}

		private static async Task DetectAndWarnCrouchWalls(StandardLevelDetailView detailView, int obstaclesCount, CancellationToken token) {
			var beatmapKey = detailView.beatmapKey;
			var beatmapLevel = detailView._beatmapLevel;
			var obstaclesText = detailView._levelParamsPanel._obstaclesCountText;

			Plugin.Log.Debug("Detecting CrouchWalls");
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