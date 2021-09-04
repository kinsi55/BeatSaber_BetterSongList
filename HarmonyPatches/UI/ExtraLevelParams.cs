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

		static StandardLevelDetailView lastInstance = null;

		public static void UpdateState() {
			if(lastInstance != null)
				lastInstance.RefreshContent();
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

			lastInstance = __instance;

			if(fields != null) {
				if(!SongDetailsUtil.isAvailable) {
					fields[0].text = fields[1].text = "N/A";
				} else if(SongDetailsUtil.instance != null) {
					void wrapper() {
						// For now we can assume non-standard diff is unranked. Probably not changing any time soon i guess
						var ch = (SongDetailsCache.Structs.MapCharacteristic)BeatmapsUtil.GetCharacteristicFromDifficulty(____selectedDifficultyBeatmap);

						if(ch != SongDetailsCache.Structs.MapCharacteristic.Standard) {
							fields[0].text = fields[1].text = "-";
						} else {
							var mh = BeatmapsUtil.GetHashOfPreview(____level);

							if(mh == null ||
								!((SongDetailsCache.SongDetails)SongDetailsUtil.instance).songs.FindByHash(mh, out var song) ||
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
								//TODO: Put this back once new pp estimation algo is mainstream (New SongDetails version)
								//fields[0].text = diff.approximatePpValue.ToString("0.0");
								fields[0].text = (diff.stars * 42.108f * (1.11f - ((.055f / 14f) * diff.stars))).ToString("0.0");
								fields[1].text = diff.stars.ToString("0.0#");
							}
						}
					}
					wrapper();
				} else if(!SongDetailsUtil.attemptedToInit) {
					SongDetailsUtil.TryGet().ContinueWith(
						x => { if(x.Result != null) UpdateState(); },
						CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext()
					);
				}

				// Basegame maps have no NJS or JD
				var njs = ____selectedDifficultyBeatmap.noteJumpMovementSpeed;
				if(njs == 0)
					njs = BeatmapDifficultyMethods.NoteJumpMovementSpeed(____selectedDifficultyBeatmap.difficulty);

				fields[2].text = njs.ToString("0.0#");

				var offset = Config.Instance.ShowMapJDInsteadOfOffset ?
					JumpDistanceCalculator.GetJd(____selectedDifficultyBeatmap.level.beatsPerMinute, njs, ____selectedDifficultyBeatmap.noteJumpStartBeatOffset) :
					____selectedDifficultyBeatmap.noteJumpStartBeatOffset;

				fields[3].text = offset.ToString(Config.Instance.ShowMapJDInsteadOfOffset ? "0.0" : "0.0#");

				var minJd = JumpDistanceCalculator.GetMinJd(____selectedDifficultyBeatmap.level.beatsPerMinute, njs);

				// Consider bpm locked if minimum is within 7%
				if(minJd > Config.Instance.UnlockedJd)
					fields[3].text += "<size=3>🔒";
			}
		}
	}
}
