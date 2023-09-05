using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(BeatmapLevelSearchHelper), nameof(BeatmapLevelSearchHelper.SearchAndSortBeatmapLevels))]
	static class ImproveBasegameSearch {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(!Config.Instance.ModBasegameSearch)
				return instructions;

			// This appends a space and the levelAuthorName to the levelString variable.
			var matcher = new CodeMatcher(instructions)
				.MatchForward(true,
					new CodeMatch(OpCodes.Ldloc_S, null, "L_previewBeatmapLevel"),
					new CodeMatch(),
					new CodeMatch(OpCodes.Stelem_Ref),
					new CodeMatch(x => x.opcode == OpCodes.Call && (x.operand as MethodInfo)?.Name == nameof(string.Concat), "Call_Concat"),
					new CodeMatch(OpCodes.Stloc_S, null, "L_levelStringSt")
				);

			matcher.Advance(1).Insert(
				new CodeInstruction(OpCodes.Ldc_I4_3),
				new CodeInstruction(OpCodes.Newarr, typeof(string)),

				new CodeInstruction(OpCodes.Dup),
				new CodeInstruction(OpCodes.Ldc_I4_0),
				new CodeInstruction(OpCodes.Ldloc_S, matcher.NamedMatch("L_levelStringSt").operand),
				new CodeInstruction(OpCodes.Stelem_Ref),

				new CodeInstruction(OpCodes.Dup),
				new CodeInstruction(OpCodes.Ldc_I4_1),
				new CodeInstruction(OpCodes.Ldstr, " "),
				new CodeInstruction(OpCodes.Stelem_Ref),

				new CodeInstruction(OpCodes.Dup),
				new CodeInstruction(OpCodes.Ldc_I4_2),
				matcher.NamedMatch("L_previewBeatmapLevel"),
				new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(IPreviewBeatmapLevel), nameof(IPreviewBeatmapLevel.levelAuthorName))),
				new CodeInstruction(OpCodes.Stelem_Ref),

				matcher.NamedMatch("Call_Concat"),
				matcher.NamedMatch("L_levelStringSt")
			);

			return matcher.InstructionEnumeration();
		}
	}
}
