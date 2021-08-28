using BeatSaberMarkupLanguage;
using BetterSongList.Util;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace BetterSongList.HarmonyPatches.UI {
	[HarmonyPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.RefreshContent))]
	static class ExtraLevelParams {
		static GameObject extraUI = null;
		static TextMeshProUGUI[] fields = null;

		static HoverHintController hhc = null;
		static IEnumerator ProcessFields() {
			//Need to wait until the end of frame for reasons beyond my understanding
			yield return new WaitForEndOfFrame();

			void ModifyValue(TextMeshProUGUI text, string hoverHint, string icon) {
				text.transform.parent.Find("Icon").GetComponent<ImageView>().SetImage($"#{icon}");
				GameObject.DestroyImmediate(text.GetComponentInParent<LocalizedHoverHint>());
				var hhint = text.GetComponentInParent<HoverHint>();

				if(hhc == null)
					hhc = Resources.FindObjectsOfTypeAll<HoverHintController>().FirstOrDefault();

				// Normally zenjected, not here obviously. I dont think the Controller is ever destroyed so we dont need to explicit null check
				ReflectionUtil.SetField(hhint, "_hoverHintController", hhc);
				hhint.text = hoverHint;
			}

			ModifyValue(fields[0], "Approximate ScoreSaber PP Value", "DifficultyIcon");
			ModifyValue(fields[1], "ScoreSaber Star Rating", "FavoritesIcon");
			ModifyValue(fields[2], "NJS (Note Jump Speed)", "FastNotesIcon");
			ModifyValue(fields[3], "JD (Jump Distance, how close notes spawn)", "MeasureIcon");

			fields[3].richText = true;
		}


		static void Postfix(IBeatmapLevel ____level, IDifficultyBeatmap ____selectedDifficultyBeatmap, LevelParamsPanel ____levelParamsPanel, StandardLevelDetailView __instance) {
			if(extraUI == null) {
				// I wanted to make a custom UI for this with bsml first... But this is MUCH easier and probably looks better
				extraUI = GameObject.Instantiate(____levelParamsPanel, ____levelParamsPanel.transform.parent).gameObject;
				GameObject.Destroy(extraUI.GetComponent<LevelParamsPanel>());
				____levelParamsPanel.transform.localPosition += new Vector3(0, 1);
				extraUI.transform.localPosition -= new Vector3(0, 4);

				fields = extraUI.GetComponentsInChildren<CurvedTextMeshPro>();
				SharedCoroutineStarter.instance.StartCoroutine(ProcessFields());
			}

			if(fields != null) {
				if(!SongDetailsUtil.isAvailable) {
					fields[0].text = fields[1].text = "N/A";
				} else if(SongDetailsUtil.instance != null) {
					// For now we can assume non-standard diff is unranked. Probably not changing any time soon i guess
					var ch = (SongDetailsCache.Structs.MapCharacteristic)BeatmapsUtil.GetCharacteristicFromDifficulty(____selectedDifficultyBeatmap);

					if(ch != SongDetailsCache.Structs.MapCharacteristic.Standard) {
						fields[0].text = fields[1].text = "-";
					} else {
						var mh = BeatmapsUtil.GetHashOfPreview(____level);

						if(mh == null ||
							!SongDetailsUtil.instance.songs.FindByHash(mh, out var song) ||
							!song.GetDifficulty(
								out var diff,
								(SongDetailsCache.Structs.MapDifficulty)____selectedDifficultyBeatmap.difficulty,
								ch
							)
						) {
							fields[0].text = fields[1].text = "?";
						} else if(!diff.ranked) {
							fields[0].text = fields[1].text = "-";
						} else {
							fields[0].text = diff.approximatePpValue.ToString("0.0");
							fields[1].text = diff.stars.ToString("0.0#");
						}
					}
				} else if(!SongDetailsUtil.attemptedToInit) {
					SongDetailsUtil.TryGet().ContinueWith(
						x => { if(x.Result != null) __instance.RefreshContent(); },
						CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext()
					);
				}

				// Basegame maps have no NJS or JD
				if(____selectedDifficultyBeatmap.noteJumpMovementSpeed > 0) {
					fields[2].text = ____selectedDifficultyBeatmap.noteJumpMovementSpeed.ToString("0.0#");
				} else {
					fields[2].text = "?";
				}

				var jd = JumpDistanceCalculator.GetJd(____selectedDifficultyBeatmap);

				if(jd > 0) {
					var minJd = JumpDistanceCalculator.GetMinJd(____selectedDifficultyBeatmap);

					fields[3].text = jd.ToString("0.0");

					// Consider bpm locked if minimum is within 7%
					if(minJd > Config.Instance.UnlockedJd)
						fields[3].text += "<size=3>🔒";
				} else {
					fields[3].text = "?";
				}
			}
		}
	}
}
