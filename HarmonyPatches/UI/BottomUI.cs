using BetterSongList.UI;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace BetterSongList.HarmonyPatches.UI {
	[HarmonyPatch(typeof(LevelCollectionNavigationController), "DidActivate")]
	static class BottomUI {
		[HarmonyPriority(int.MaxValue)]
		static void Prefix(LevelCollectionNavigationController __instance, bool firstActivation) {
			if(!firstActivation)
				return;

			SharedCoroutineStarter.instance.StartCoroutine(InitDelayed(__instance.transform));
		}

		static IEnumerator InitDelayed(Transform t) {
			yield return new WaitForEndOfFrame();
			FilterUI.AttachTo(t.parent);
		}
	}
}
