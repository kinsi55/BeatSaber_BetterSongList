using BeatSaberMarkupLanguage;
using HarmonyLib;
using HMUI;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace BetterSongList.HarmonyPatches.UI {
	[HarmonyPatch(typeof(LevelCollectionTableView), nameof(LevelCollectionTableView.Init), new Type[] { })]
	static class ScrollEnhancements {
		static GameObject[] buttons = null;
		static void Prefix(LevelCollectionTableView __instance) {
			if(!__instance._isInitialized)
				SharedCoroutineStarter.instance.StartCoroutine(DoTheFunny(__instance._tableView, __instance.transform));

			UpdateState();
		}

		public static void UpdateState() {
			buttons?.Do(x => { if(x != null) x.SetActive(Config.Instance.ExtendSongsScrollbar); });
		}

		static Transform BuildButton(Transform baseButton, string Icon, float vOffs, float rotation, UnityAction cb) {
			var newBtn = GameObject.Instantiate(baseButton,
				baseButton
				.parent // ScrollBar
				.parent // LevelsTableView
				.parent // LevelCollecionViewController
			);

			// Appropriately size the button rect
			var r = (RectTransform)newBtn.transform;
			r.anchorMin = new Vector2(0.96f, 0.893f - vOffs);
			r.anchorMax = new Vector2(1, 0.953f - vOffs);

			var i = newBtn.GetComponentInChildren<ImageView>();
			if(Icon?[0] == '#')
				i.SetImageAsync(Icon);

			// Put the Icon in the middle of the touchable rect
			r = (RectTransform)i.transform;
			r.offsetMax = Vector2.zero;
			r.offsetMin = new Vector2(-2.5f, -2.5f);
			r.localEulerAngles = new Vector3(0, 0, rotation);


			var btn = newBtn.GetComponent<NoTransitionsButton>();
			btn.interactable = true;
			btn.onClick.AddListener(cb);

			return newBtn;
		}

		static IEnumerator DoTheFunny(TableView table, Transform a) {
			//yield return new WaitForSeconds(2f);
			yield return new WaitForEndOfFrame();

			// Add more horizontal space to the the LevelCollecionViewController
			var r = (RectTransform)table.transform.parent.parent;
			r.sizeDelta += new Vector2(4, 0);

			// Offset the LevelsTableView to the original position
			r = (RectTransform)table.transform.parent;
			r.anchorMin += new Vector2(0.02f, 0);
			r.sizeDelta -= new Vector2(2, 0);

			// Yoink the original scrollbar button
			var buton = a.Find("ScrollBar/UpButton");

			void Scroll(float step, int direction) {
				var cells = table.dataSource.NumberOfCells();
				if(cells == 0)
					return;

				var amt = cells * step * direction;

				if(step != 1)
					amt += table.GetVisibleCellsIdRange().Item1;

				table.ScrollToCellWithIdx((int)amt, TableView.ScrollPositionType.Beginning, true);
			}


			var btnUpFast = BuildButton(buton, null, 0, -90, () => Scroll(0.1f, -1));
			var btnDownFast = BuildButton(buton, null, 0.86f, 90, () => Scroll(0.1f, 1));

			buttons = new[] {
				btnUpFast,
				BuildButton(buton, "#HeightIcon", 0.10f, 0, () => Scroll(1, 0)),
				BuildButton(buton, "#HeightIcon", 0.76f, 180, () => Scroll(1, 1)),
				btnDownFast
			}.Select(x => x.gameObject).ToArray();

			Utilities.LoadSpriteFromAssemblyAsync("BetterSongList.UI.DoubleArrowIcon.png").ContinueWith(x => {
				btnUpFast.GetComponentInChildren<ImageView>().sprite = x.Result;
				btnDownFast.GetComponentInChildren<ImageView>().sprite = x.Result;
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}
