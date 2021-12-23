using HarmonyLib;
using HMUI;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace BetterSongList.HarmonyPatches.UI {
	[HarmonyPatch]
	static class DontShiftAroundStuffScoreSaber {
		static MethodBase TargetMethod() => IPA.Loader.PluginManager.GetPluginFromId("ScoreSaber")?
			.Assembly.GetType("ScoreSaber.UI.Other.ScoreSaberLeaderboardView")?
			.GetMethod("SetPlayButtonState");

		static Exception Cleanup(Exception ex) => null;

		static Button lastButton;

		[HarmonyPriority(int.MinValue)]
		// taking state by ref because "the first prefix that returns false will skip all remaining prefixes [..with..] ref arguments"
		// https://harmony.pardeike.net/articles/patching-prefix.html
		static bool Prefix(ref bool state, Button ____playButton) {
			// Scoresaber will use the wrong play button in certain cases.... so I guess I gotta handle that?
			if(____playButton.transform.parent?.parent?.parent?.parent?.name == "LevelCollectionNavigationController")
				lastButton = ____playButton;

			if(lastButton == null)
				return true;

			var l = ((RectTransform)lastButton.gameObject.transform).localScale;

			l.x = l.y = state ? 1 : 0.001f;
			((RectTransform)lastButton.gameObject.transform).localScale = l;
			return false;
		}
	}
}
