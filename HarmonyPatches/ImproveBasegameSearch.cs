using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(BeatmapLevelSearchHelper), nameof(BeatmapLevelSearchHelper.SearchAndSortBeatmapLevels))]
	static class ImproveBasegameSearch {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(!Config.Instance.ModBasegameSearch)
				return instructions;

			// This appends a space and the levelAuthorName to the levelString variable.
			return new CodeMatcher(instructions)
				.MatchForward(false, new CodeMatch(OpCodes.Newarr))
				.Advance(-1)
				.SetOpcodeAndAdvance(OpCodes.Ldc_I4_7)
				.MatchForward(false, new CodeMatch(OpCodes.Stelem_Ref),
					new CodeMatch(OpCodes.Call))
				.Insert(new CodeInstruction(OpCodes.Stelem_Ref),
					new CodeInstruction(OpCodes.Dup),
					new CodeInstruction(OpCodes.Ldc_I4_5),
					new CodeInstruction(OpCodes.Ldstr, " "),
					new CodeInstruction(OpCodes.Stelem_Ref),
					new CodeInstruction(OpCodes.Dup),
					new CodeInstruction(OpCodes.Ldc_I4_6),
					new CodeInstruction(OpCodes.Ldloc_S, 5),
					new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(IPreviewBeatmapLevel), nameof(IPreviewBeatmapLevel.levelAuthorName))))
				.InstructionEnumeration();
		}
	}
}
