using HarmonyLib;
using HMUI;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace BetterSongList.HarmonyPatches.UI {
	[HarmonyPatch]
	class DontShiftAroundStuffScoreSaber {
		static MethodBase TargetMethod() => AccessTools.Method("ScoreSaber.UI.Other.ScoreSaberLeaderboardView:SetPlayButtonState");
		static Exception Cleanup(Exception ex) => null;

		[HarmonyPriority(int.MinValue)]
		// taking state by ref because "the first prefix that returns false will skip all remaining prefixes [..with..] ref arguments"
		// https://harmony.pardeike.net/articles/patching-prefix.html
		static bool Prefix(ref bool state, Button ____playButton) {
			var l = ((RectTransform)____playButton.gameObject.transform).localScale;

			l.y = state ? 1 : 0;
			____playButton.GetComponent<NoTransitionsButton>().enabled = state;
			((RectTransform)____playButton.gameObject.transform).localScale = l;
			return false;
		}
	}
}
