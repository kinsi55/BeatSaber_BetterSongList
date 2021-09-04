using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HarmonyLib;
using HMUI;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace BetterSongList.HarmonyPatches.UI {
	[HarmonyPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.RefreshContent))]
	static class SongDeleteButton {
		static Button deleteButton = null;

		static CustomPreviewBeatmapLevel lastLevel = null;
		public static void UpdateState() {
			if(deleteButton == null)
				return;

			deleteButton.interactable = lastLevel != null && (Config.Instance.AllowWipDelete || !lastLevel.levelID.Contains(" WIP"));
		}

		class DeleteConfirmHandler {
			public static Lazy<DeleteConfirmHandler> instance = new Lazy<DeleteConfirmHandler>(() => new DeleteConfirmHandler());

			[UIParams] readonly BSMLParserParams parserParams = null;

			public void ConfirmDelete() => parserParams.EmitEvent("Show");

			void Confirm() {
				if(lastLevel == null)
					return;

				SongCore.Loader.Instance.DeleteSong(lastLevel.customLevelPath);
			}
		}

		[HarmonyPriority(int.MinValue)]
		static void Postfix(StandardLevelDetailView __instance, Button ____practiceButton, IPreviewBeatmapLevel ____level) {
			if(deleteButton == null && ____practiceButton != null) {
				var newButton = GameObject.Instantiate(____practiceButton.gameObject, ____practiceButton.transform.parent);
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
				IPA.Utilities.ReflectionUtil.SetField(icon, "_skew", 0.2f);
				icon.material = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "UINoGlow");
				icon.SetImage("#DeleteIcon");

				GameObject.DestroyImmediate(t.gameObject);

				BSMLParser.instance.Parse(
					Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "BetterSongList.UI.BSML.SongDeleteConfirm.bsml"),
					__instance.transform.parent.gameObject,
					DeleteConfirmHandler.instance.Value
				);
			}

			lastLevel = ____level as CustomPreviewBeatmapLevel;

			UpdateState();
		}
	}

	//Having both ends up overlapping the fast scroll buttons and that is kind of annoying
	[HarmonyPatch]
	static class deletebuttont {
		static MethodBase TargetMethod() => AccessTools.Method("BeatSaberPlus.Modules.GameTweaker.Patches.PStandardLevelDetailView:SetDeleteSongButtonEnabled");
		static Exception Cleanup(Exception ex) => null;
		static bool Prefix() => false;
	}
}
