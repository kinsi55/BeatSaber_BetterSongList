using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BetterSongList.UI;
using BetterSongList.Util;
using HarmonyLib;
using HMUI;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SongCore;
using UnityEngine;
using UnityEngine.UI;

namespace BetterSongList.HarmonyPatches.UI {
	[HarmonyPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.RefreshContent))]
	static class SongDeleteButton {
		static Button deleteButton = null;

		static BeatmapLevel lastLevel = null;

		static bool isWip => lastLevel != null && lastLevel.levelID.Contains(" WIP");

		public static void UpdateState() {
			if(deleteButton == null)
				return;

			deleteButton.interactable = lastLevel != null && (Config.Instance.AllowWipDelete || !isWip);
		}

		class DeleteConfirmHandler {
			public static Lazy<DeleteConfirmHandler> instance = new Lazy<DeleteConfirmHandler>(() => new DeleteConfirmHandler());

			[UIParams] readonly BSMLParserParams parserParams = null;

			public void ConfirmDelete() => parserParams.EmitEvent("Show");

			void Confirm() {
				if(lastLevel == null)
					return;

				var path = Loader.CustomLevelLoader._loadedBeatmapSaveData.TryGetValue(lastLevel.levelID, out var loadedSaveData)
					? loadedSaveData.customLevelFolderInfo.folderPath
					: null;

				if(string.IsNullOrEmpty(path))
					goto FAILED;

				try {
					Loader.Instance.DeleteSong(path, !isWip);
				} catch {
					goto FAILED;
				}

				if(!isWip)
					return;

				Task.Run(() => {
					try {
						WinApi.DeleteFileOrFolder(path);
					} catch { }
				});
				return;

				FAILED:
					FilterUI.persistentNuts.ShowErrorASAP("Deleting the map failed because it failed. Deal with it");
			}
		}

		[HarmonyPriority(int.MinValue)]
		static void Postfix(StandardLevelDetailView __instance) {
			if(deleteButton == null && __instance._practiceButton != null) {
				var newButton = GameObject.Instantiate(__instance._practiceButton.gameObject, __instance._practiceButton.transform.parent);
				deleteButton = newButton.GetComponentInChildren<Button>();

				deleteButton.onClick.AddListener(DeleteConfirmHandler.instance.Value.ConfirmDelete);

				newButton.GetComponentsInChildren<LayoutElement>().Last().minWidth = 12;
				newButton.transform.SetAsFirstSibling();

				var t = newButton.GetComponentInChildren<CurvedTextMeshPro>();

				var iconG = new GameObject("Icon");
				iconG.transform.SetParent(t.transform.parent, false);
				iconG.transform.localScale = new Vector3(0.69f, 0.69f);
				var icon = iconG.AddComponent<ImageView>();

				icon.color = t.color;
				icon._skew = 0.2f;
				icon.material = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "UINoGlow");
				icon.SetImage("#DeleteIcon");

				GameObject.DestroyImmediate(t.gameObject);

				BSMLParser.instance.Parse(
					Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "BetterSongList.UI.BSML.SongDeleteConfirm.bsml"),
					__instance.transform.parent.gameObject,
					DeleteConfirmHandler.instance.Value
				);
			}

			lastLevel = !__instance._beatmapLevel.hasPrecalculatedData ? __instance._beatmapLevel : null;

			UpdateState();
		}
	}
}
