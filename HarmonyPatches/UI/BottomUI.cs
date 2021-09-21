using BetterSongList.UI;
using HarmonyLib;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace BetterSongList.HarmonyPatches.UI {
	[HarmonyPatch(typeof(LevelCollectionNavigationController), "DidActivate")]
	static class BottomUI {
		[HarmonyPriority(int.MaxValue)]
		static void Prefix(LevelCollectionNavigationController __instance, bool firstActivation) {
			SharedCoroutineStarter.instance.StartCoroutine(FixPos(__instance.transform));

			if(!firstActivation)
				return;

			SharedCoroutineStarter.instance.StartCoroutine(InitDelayed(__instance.transform));
		}

		// Levelnav is kinda too far down in 1.18
		static IEnumerator FixPos(Transform t) {
			yield return new WaitForEndOfFrame();
			t.localPosition = new Vector3(0, -7);
		}

		static IEnumerator InitDelayed(Transform t) {
			yield return new WaitForEndOfFrame();
			FilterUI.AttachTo(t.parent);
		}
	}
}
