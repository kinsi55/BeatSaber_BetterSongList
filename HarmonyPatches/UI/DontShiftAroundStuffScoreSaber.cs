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
			// Scoresaber will use the wrong play button in certain cases.... so I guess I gotta handle that?
			if(____playButton.transform.parent?.parent?.parent?.parent?.name != "LevelCollectionNavigationController")
				return true;

			var l = ((RectTransform)____playButton.gameObject.transform).sizeDelta;

			l.y = state ? 10 : -5000;
			((RectTransform)____playButton.gameObject.transform).sizeDelta = l;
			// Fix button being broken for whatever reason
			var l2 = ____playButton.GetComponent<NoTransitionsButton>();
			l2.enabled = false;
			l2.enabled = true;
			return false;
		}
	}
}
