using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using HarmonyLib;
using HMUI;
using UnityEngine;
using UnityEngine.Events;

namespace BetterSongList.HarmonyPatches.UI {
	[HarmonyPatch(typeof(LevelCollectionTableView), nameof(LevelCollectionTableView.Init), new Type[] { })]
	static class ScrollEnhancements {
		static GameObject[] buttons = null;
		static void Prefix(bool ____isInitialized, TableView ____tableView, LevelCollectionTableView __instance) {
			if(!____isInitialized)
				SharedCoroutineStarter.instance.StartCoroutine(DoTheFunny(____tableView, __instance.transform));

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
				//.parent // LevelCollectionNavigationController
			);

			// Appropriately size the button rect
			var r = (RectTransform)newBtn.transform;
			r.anchorMin = new Vector2(0.96f, 0.893f - vOffs);
			r.anchorMax = new Vector2(1, 0.953f - vOffs);

			var i = newBtn.GetComponentInChildren<ImageView>();
			if(Icon[0] == '#') {
				i.SetImage(Icon);
			}

			// Put the Icon in the middle of the touchable rect
			r = (RectTransform)i.transform;
			r.offsetMax = Vector2.zero;
			r.offsetMin = new Vector2(-2.5f, -2.5f);
			r.localEulerAngles = new Vector3(0, 0, rotation);


			var btn = newBtn.GetComponent<NoTransitionsButton>();
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


			var btnUpFast = BuildButton(buton, "-", 0, 0, () => Scroll(0.1f, -1));
			var btnDownFast = BuildButton(buton, "-", 0.86f, 180, () => Scroll(0.1f, 1));

			buttons = new[] {
				btnUpFast,
				BuildButton(buton, "#HeightIcon", 0.09f, 0, () => Scroll(1, 0)),
				BuildButton(buton, "#HeightIcon", 0.77f, 180, () => Scroll(1, 1)),
				btnDownFast
			}.Select(x => x.gameObject).ToArray();

			var Tex2D = new Texture2D(2, 2);
			Tex2D.LoadImage(Utilities.GetResource(Assembly.GetExecutingAssembly(), "BetterSongList.UI.DoubleArrowIcon.png"));

			var sp = Sprite.Create(Tex2D, new Rect(0, 0, Tex2D.width, Tex2D.height), Vector2.zero, 10);

			btnUpFast.GetComponentInChildren<ImageView>().sprite = sp;
			btnDownFast.GetComponentInChildren<ImageView>().sprite = sp;
		}
	}
}
