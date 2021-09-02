using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BetterSongList.FilterModels;
using BetterSongList.HarmonyPatches;
using BetterSongList.SortModels;
using BetterSongList.Util;
using HMUI;
using IPA.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterSongList.UI {
#if DEBUG
	public
#endif
	class FilterUI {
		internal static readonly FilterUI persistentNuts = new FilterUI();
#pragma warning disable 649
		[UIComponent("root")] private RectTransform rootTransform;
#pragma warning restore
		[UIParams] readonly BSMLParserParams parserParams = null;

		FilterUI() { }

		static Dictionary<string, ISorter> sortOptions = new Dictionary<string, ISorter>() {
			{ "Song Name", SortMethods.alphabeticalSongname },
			{ "Download Date", SortMethods.downloadTime },
			{ "Ranked Stars", SortMethods.stars },
			{ "Song Length", SortMethods.songLength },
			{ "Mapper Name", SortMethods.alphabeticalMapper },
			{ "BPM", SortMethods.bpm },
			{ "BeatSaver Date", SortMethods.beatSaverDate },
			{ "Default", null }
		};

		static Dictionary<string, IFilter> filterOptions = new Dictionary<string, IFilter>() {
			{ "All", null },
			{ "Ranked", FilterMethods.ranked },
			{ "Qualified", FilterMethods.qualified },
			{ "Unplayed", FilterMethods.unplayed },
			{ "Unranked", FilterMethods.unranked }
		};

		[UIValue("_sortOptions")] static List<object> _sortOptions = sortOptions.Keys.ToList<object>();
		[UIValue("_filterOptions")] static List<object> _filterOptions = filterOptions.Keys.ToList<object>();

		void _SetSort(string selected) => SetSort(selected);
		internal static void SetSort(string selected, bool storeToConfig = true, bool refresh = true) {
			if(selected == null || !sortOptions.ContainsKey(selected))
				selected = sortOptions.Keys.Last();

			var newSort = sortOptions[selected];
			var unavReason = (newSort as IAvailabilityCheck)?.GetUnavailabilityReason();

			if(unavReason != null) {
				persistentNuts?.ShowErrorASAP($"Can't sort by {selected} - {unavReason}");
				SetSort(null, false, false);
				return;
			}

			Plugin.Log.Warn(string.Format("Setting Sort to {0}", selected));
			if(HookLevelCollectionTableSet.sorter != newSort) {
				if(storeToConfig)
					Config.Instance.LastSort = selected;

				HookLevelCollectionTableSet.sorter = newSort;
				RestoreTableScroll.ResetScroll();
				if(refresh)
					HookLevelCollectionTableSet.Refresh(true);
			}

			XD.FunnyNull(persistentNuts._sortDropdown)?.SelectCellWithIdx(_sortOptions.IndexOf(selected));
		}

		public static void ClearFilter(bool reloadTable = false) => SetFilter(null, false, reloadTable);
		void _SetFilter(string selected) => SetFilter(selected);
		internal static void SetFilter(string selected, bool storeToConfig = true, bool refresh = true) {
			if(selected == null || !filterOptions.ContainsKey(selected))
				selected = filterOptions.Keys.First();

			var newFilter = filterOptions[selected];
			var unavReason = (newFilter as IAvailabilityCheck)?.GetUnavailabilityReason();

			if(unavReason != null) {
				persistentNuts?.ShowErrorASAP($"Can't filter by {selected} - {unavReason}");
				SetFilter(null, false, false);
				return;
			}

			Plugin.Log.Warn(string.Format("Setting Filter to {0}", selected));
			if(HookLevelCollectionTableSet.filter != filterOptions[selected]) {
				if(storeToConfig)
					Config.Instance.LastFilter = selected;

				HookLevelCollectionTableSet.filter = filterOptions[selected];
				RestoreTableScroll.ResetScroll();
				if(refresh)
					HookLevelCollectionTableSet.Refresh(true);
			}

			XD.FunnyNull(persistentNuts._filterDropdown)?.SelectCellWithIdx(_filterOptions.IndexOf(selected));
		}

		internal static void SetSortDirection(bool ascending, bool refresh = true) {
			if(HookLevelCollectionTableSet.sorter == null)
				return;

			if(Config.Instance.SortAsc != ascending) {
				Config.Instance.SortAsc = ascending;
				RestoreTableScroll.ResetScroll();
				if(refresh)
					HookLevelCollectionTableSet.Refresh(true);
			}

			XD.FunnyNull(persistentNuts._sortDirection)?.SetText(ascending ? "▲" : "▼");
		}

		static void ToggleSortDirection() {
			if(HookLevelCollectionTableSet.sorter == null)
				return;

			SetSortDirection(!Config.Instance.SortAsc);
		}

		static void SelectRandom() {
			var x = Resources.FindObjectsOfTypeAll<LevelCollectionTableView>().FirstOrDefault();

			if(x == null)
				return;

			var ml = HookLevelCollectionTableSet.lastOutMapList ?? HookLevelCollectionTableSet.lastInMapList;

			if(ml.Length < 2)
				return;

			x.SelectLevel(ml[Random.Range(0, ml.Length)]);
		}

		Queue<string> warnings = new Queue<string>();
		bool warningLoadInProgress;
		public void ShowErrorASAP(string text = null) {
			if(text != null)
				warnings.Enqueue(text);
			if(!warningLoadInProgress)
				SharedCoroutineStarter.instance.StartCoroutine(_ShowError());
		}

		[UIAction("PossiblyShowNextWarning")] void PossiblyShowNextWarning() => ShowErrorASAP();

		IEnumerator _ShowError() {
			warningLoadInProgress = true;
			yield return new WaitUntil(() => _failTextLabel != null);
			var x = _failTextLabel.GetComponentInParent<ViewController>();
			if(x != null) {
				yield return new WaitUntil(() => !x.isInTransition);

				if(x.isActivated && warnings.Count > 0) {
					_failTextLabel.text = warnings.Dequeue();
					parserParams.EmitEvent("IncompatabilityNotice");
				}
			}
			warningLoadInProgress = false;
		}


		[UIComponent("filterLoadingIndicator")] internal readonly ImageView _filterLoadingIndicator = null;
		[UIComponent("sortDropdown")] readonly DropdownWithTableView _sortDropdown = null;
		[UIComponent("filterDropdown")] readonly DropdownWithTableView _filterDropdown = null;
		[UIComponent("sortDirection")] readonly ClickableText _sortDirection = null;
		[UIComponent("failTextLabel")] readonly TextMeshProUGUI _failTextLabel = null;
		readonly string version = $"BetterSongList v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)} by Kinsi55";
		Config cfgi => Config.Instance;

		internal static void Init() {
			SetSort(Config.Instance.LastSort, true, false);
			SetFilter(Config.Instance.LastFilter, true, false);
			SetSortDirection(Config.Instance.SortAsc);

			if(!SongDataCoreChecker.didCheck && SongDataCoreChecker.IsInstalled() && !SongDataCoreChecker.IsUsed())
				persistentNuts.ShowErrorASAP("You have the Plugin 'SongDataCore' installed but no Plugin which is using it. It's advised to remove it");
		}

		internal static void AttachTo(Transform target) {
			BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "BetterSongList.UI.BSML.MainUI.bsml"), target.gameObject, persistentNuts);
			persistentNuts.rootTransform.localScale *= 0.7f;

			(target as RectTransform).sizeDelta += new Vector2(0, 2);
			target.GetChild(0).position -= new Vector3(0, 0.02f);
		}

		[UIAction("#post-parse")]
		void Parsed() {
			HackDropdown(_sortDropdown);
			HackDropdown(_filterDropdown);

			SetSort(Config.Instance.LastSort, false, false);
			SetFilter(Config.Instance.LastFilter, false, false);

			SetSortDirection(Config.Instance.SortAsc, false);
		}

		static void HackDropdown(DropdownWithTableView dropdown) {
			ReflectionUtil.SetField(dropdown, "_numberOfVisibleCells", 9);
			dropdown.ReloadData();
			// Offset it far down so that its not sticking up 10 kilometers
			var l = ReflectionUtil.GetField<Button, DropdownWithTableView>(dropdown, "_button");
			l.onClick.RemoveAllListeners();
			l.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
				var offsHack = (dropdown.transform as RectTransform);

				offsHack.offsetMin = new Vector2(offsHack.offsetMin.x, 5 + (dropdown.tableViewDataSource.NumberOfCells() * 11));
				dropdown.OnButtonClick();
				offsHack.offsetMin = new Vector2(offsHack.offsetMin.x, 0);

				// We should only do this on the first load because the modified position will stick
				l.onClick.RemoveAllListeners();
				l.onClick.AddListener(new UnityEngine.Events.UnityAction(dropdown.OnButtonClick));
			}));
		}
	}
}
