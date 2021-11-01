using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BetterSongList.FilterModels;
using BetterSongList.HarmonyPatches;
using BetterSongList.HarmonyPatches.UI;
using BetterSongList.SortModels;
using BetterSongList.Util;
using HMUI;
using IPA.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
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
			{ "BPM", SortMethods.bpm },
			{ "BeatSaver Date", SortMethods.beatSaverDate },
			{ "Default", null }
		};

		static Dictionary<string, IFilter> filterOptions = new Dictionary<string, IFilter>() {
			{ "All", null },
			{ "Ranked", FilterMethods.ranked },
			{ "Qualified", FilterMethods.qualified },
			{ "Unplayed", FilterMethods.unplayed },
			{ "Played", FilterMethods.played },
			{ "Requirements", FilterMethods.requirements },
			{ "Unranked", FilterMethods.unranked },
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

#if DEBUG
			Plugin.Log.Warn(string.Format("Setting Sort to {0}", selected));
#endif
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

		static void SettingsClosed() {
			SongDeleteButton.UpdateState();
			ScrollEnhancements.UpdateState();
			ExtraLevelParams.UpdateState();
			Config.Instance.Changed();
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

#if DEBUG
			Plugin.Log.Warn(string.Format("Setting Filter to {0}", selected));
#endif
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
				persistentNuts.ShowErrorASAP("You have 'SongDataCore' installed, but dont have any Plugin which is using it (Normally SongBrowser would be mainly what uses it).\nIt's advised to remove that as it can increase load times.");
		}

		internal static void AttachTo(Transform target) {
			BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "BetterSongList.UI.BSML.MainUI.bsml"), target.gameObject, persistentNuts);
			persistentNuts.rootTransform.localScale *= 0.7f;

			(target as RectTransform).sizeDelta += new Vector2(0, 2);
			target.GetChild(0).position -= new Vector3(0, 0.02f);
		}

		bool settingsWereOpened = false;
		void SettingsOpened() {
			Config.Instance.SettingsSeenInVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			settingsWereOpened = true;
		}
		[UIComponent("settingsButton")] readonly ClickableImage _settingsButton = null;

		[UIAction("#post-parse")]
		void Parsed() {
			HackDropdown(_sortDropdown);
			HackDropdown(_filterDropdown);

			SetSort((string)sortOptions.Select(x => new object[] { x.Key, x.Value }).FirstOrDefault(x => x[1] == HookLevelCollectionTableSet.sorter)?[0], false, false);
			SetFilter((string)filterOptions.Select(x => new object[] { x.Key, x.Value }).FirstOrDefault(x => x[1] == HookLevelCollectionTableSet.filter)?[0], false, false);

			SetSortDirection(Config.Instance.SortAsc, false);

			SharedCoroutineStarter.instance.StartCoroutine(PossiblyDrawUserAttentionToSettingsButton());
		}

		IEnumerator PossiblyDrawUserAttentionToSettingsButton() {
			try {
				if(System.Version.TryParse(Config.Instance.SettingsSeenInVersion, out var oldV)) {
					if(oldV >= new System.Version("0.1.2.0"))
						yield break;
				}
			} catch { }

			while(!settingsWereOpened) {
				yield return new WaitForSeconds(.5f);
				if(_settingsButton != null)
					_settingsButton.color = Color.green;

				yield return new WaitForSeconds(.5f);
				if(_settingsButton != null)
					_settingsButton.color = Color.white;
			}
		}

		static void HackDropdown(DropdownWithTableView dropdown) {
			var c = Mathf.Min(9, dropdown.tableViewDataSource.NumberOfCells());
			ReflectionUtil.SetField(dropdown, "_numberOfVisibleCells", c);
			dropdown.ReloadData();

			var m = ReflectionUtil.GetField<ModalView, DropdownWithTableView>(dropdown, "_modalView");
			((RectTransform)m.transform).pivot = new Vector2(0.5f, 0.14f - (c * 0.011f));
		}


		[UIComponent("sponsorsText")] CurvedTextMeshPro sponsorsText = null;
		void OpenSponsorsLink() => Process.Start("https://github.com/sponsors/kinsi55");
		void OpenSponsorsModal() {
			parserParams.EmitEvent("CloseSettings");
			sponsorsText.text = "Loading...";
			Task.Run(() => {
				string desc = "Failed to load";
				try {
					desc = (new WebClient()).DownloadString("http://kinsi.me/sponsors/bsout.php");
				} catch { }

				_ = IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => {
					sponsorsText.text = desc;
				});
			}).ConfigureAwait(false);
		}
	}
}
