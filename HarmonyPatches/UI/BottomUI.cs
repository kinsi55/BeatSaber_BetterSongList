using BetterSongList.UI;
using HarmonyLib;
using System.Collections;
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

		static void Postfix(ref bool ____hideDetailViewController) {
			// This doesn't get reset properly in the original method and causes a race condition that doesn't dismiss view controllers properly on the level end screen if
			// a) a song is automatically selected by BSL when coming from the main menu
			// b) the active tab is a non-level-pack tab (i.e. favorites or all songs)
			// c) a view controller is presented on the level end screen (e.g. by BeatSaviorData)
			// This causes ClearChildViewControllers to be called too early and eventually HMUI.Screen.SetRootViewController gets called twice
			// in a row for the same screen so TransitionCoroutine doesn't have the time to complete and disable the previous view controller.
			____hideDetailViewController = false;
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
