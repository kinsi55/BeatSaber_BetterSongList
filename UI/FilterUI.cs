using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BetterSongList.FilterModels;
using BetterSongList.HarmonyPatches;
using BetterSongList.Interfaces;
using BetterSongList.SortModels;
using BetterSongList.Util;
using HMUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace BetterSongList.UI {
#if DEBUG
	public
#endif
	class FilterUI {
		internal static readonly FilterUI persistentNuts = new FilterUI();
#pragma warning disable 649
		[UIComponent("root")] private readonly RectTransform rootTransform;
#pragma warning restore
		[UIParams] readonly BSMLParserParams parserParams = null;

		FilterUI() { }

		[UIComponent("sortDropdown")] readonly DropdownWithTableView _sortDropdown = null;
		[UIComponent("filterDropdown")] readonly DropdownWithTableView _filterDropdown = null;


		static Dictionary<string, ISorter> sortOptions = null;
		static Dictionary<string, IFilter> filterOptions = null;

		[UIValue("_sortOptions")] static List<object> _sortOptions = null;
		[UIValue("_filterOptions")] static List<object> _filterOptions = null;

		static void UpdateVisibleTransformers() {
			static bool CheckIsVisible(ITransformerPlugin plugin) {
				plugin.ContextSwitch(HookSelectedCategory.lastSelectedCategory, HookSelectedCollection.lastSelectedCollection);
				return plugin.visible;
			}

			sortOptions = SortMethods.methods
				.Where(x => !(x.Value is ITransformerPlugin plugin) || CheckIsVisible(plugin))
				.OrderBy(x => (x.Value is ITransformerPlugin) ? 0 : 1).ToDictionary(x => x.Key, x => x.Value);

			_sortOptions = sortOptions.Select(x => x.Key).ToList<object>();

			filterOptions = FilterMethods.methods
				.Where(x => !(x.Value is ITransformerPlugin plugin) || CheckIsVisible(plugin))
				.OrderBy(x => (x.Value is ITransformerPlugin) ? 0 : 1)
				.ToDictionary(x => x.Key, x => x.Value);

			_filterOptions = filterOptions.Select(x => x.Key).ToList<object>();
		}

		public void UpdateDropdowns() {
			if(_sortDropdown != null) {
				_sortDropdown.ReloadData();
				HackDropdown(_sortDropdown);
			}
			if(_filterDropdown != null) {
				_filterDropdown.ReloadData();
				HackDropdown(_filterDropdown);
			}
		}

		public void UpdateTransformerOptionsAndDropdowns() {
			UpdateVisibleTransformers();
			UpdateDropdowns();
		}


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

		public static void ClearFilter(bool reloadTable = false) => SetFilter(null, false, reloadTable);
		void _SetFilter(string selected) => SetFilter(selected);
		internal static void SetFilter(string selected, bool storeToConfig = true, bool refresh = true) {
			if(selected == null || !filterOptions.ContainsKey(selected))
				selected = filterOptions.Keys.Last();

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

			if(persistentNuts._sortDirection != null)
				persistentNuts._sortDirection.text = ascending ? "▲" : "▼";
		}

		static void ToggleSortDirection() {
			if(HookLevelCollectionTableSet.sorter == null)
				return;

			SetSortDirection(!Config.Instance.SortAsc);
		}

		static readonly System.Random ran = new System.Random();
		static void SelectRandom() {
			var x = UnityEngine.Object.FindObjectOfType<LevelCollectionTableView>();

			if(x == null)
				return;

			/*
			 * I dont think theres any place in the game where SetData is not called with an Array
			 * 
			 * .Count (Enumerable/Linq) is slower than directly accessing and Arrays Length
			 * 
			 * Not that it matters, but for now we can do this.
			 */
			var ml = (HookLevelCollectionTableSet.lastOutMapList ?? 
				HookLevelCollectionTableSet.lastInMapList)
				as BeatmapLevel[];

			if(ml == null)
				return;

			if(ml.Length < 2)
				return;

			x.SelectLevel(ml[ran.Next(0, ml.Length)]);
		}

		readonly Queue<string> warnings = new Queue<string>();
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
		[UIComponent("sortDirection")] readonly ClickableText _sortDirection = null;
		[UIComponent("failTextLabel")] readonly TextMeshProUGUI _failTextLabel = null;

		internal static void Init() {
			UpdateVisibleTransformers();
			SetSort(Config.Instance.LastSort, false, false);
			SetFilter(Config.Instance.LastFilter, false, false);
			SetSortDirection(Config.Instance.SortAsc);
		}

		internal static void AttachTo(Transform target) {
			BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "BetterSongList.UI.BSML.MainUI.bsml"), target.gameObject, persistentNuts);
			persistentNuts.rootTransform.localScale *= 0.7f;

			(target as RectTransform).sizeDelta += new Vector2(0, 2);
			target.GetChild(0).position -= new Vector3(0, 0.02f);
		}

		bool settingsWereOpened = false;
		BSMLParserParams settingsViewParams = null;
		void SettingsOpened() {
			Config.Instance.SettingsSeenInVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			settingsWereOpened = true;

			BSMLStuff.InitSplitView(ref settingsViewParams, rootTransform.gameObject, SplitViews.Settings.instance).EmitEvent("ShowSettings");
		}
		[UIComponent("settingsButton")] readonly ClickableImage _settingsButton = null;

		[UIAction("#post-parse")]
		void Parsed() {
			settingsViewParams = null;
			UpdateVisibleTransformers();

			foreach(var x in sortOptions) {
				if(x.Value == HookLevelCollectionTableSet.sorter) {
					SetSort(x.Key, false, false);
					break;
				}
			}
			foreach(var x in filterOptions) {
				if(x.Value == HookLevelCollectionTableSet.filter) {
					SetFilter(x.Key, false, false);
					break;
				}
			}

			UpdateDropdowns();

			SetSortDirection(Config.Instance.SortAsc, false);

			SharedCoroutineStarter.instance.StartCoroutine(PossiblyDrawUserAttentionToSettingsButton());
		}

		IEnumerator PossiblyDrawUserAttentionToSettingsButton() {
			try {
				if(System.Version.TryParse(Config.Instance.SettingsSeenInVersion, out var oldV)) {
					if(oldV >= new System.Version("0.2.6.0"))
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
			dropdown._numberOfVisibleCells = c;
			dropdown.ReloadData();
		}
	}
}
