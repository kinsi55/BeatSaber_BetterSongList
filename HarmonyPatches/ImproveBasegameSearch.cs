using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BetterSongList.Util;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelFilter), nameof(LevelFilter.FilterLevelByText))]
	static class ImproveBasegameSearch {
		[HarmonyPriority(int.MinValue + 10)]
		static void Prefix(ref string[] searchTerms) {
			if(!Config.Instance.ModBasegameSearch)
				return;

			if(searchTerms.Length > 1)
				searchTerms = new string[] { string.Join(" ", searchTerms) };
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
			if(!Config.Instance.ModBasegameSearch)
				return instructions;

			// This appends a space and the mappers/lighters to the levelString variable.
			var matcher = new CodeMatcher(instructions)
				.MatchForward(true,
					new CodeMatch(OpCodes.Ldloc_2, null, "L_beatmapLevel"),
					new CodeMatch(),
					new CodeMatch(OpCodes.Stelem_Ref),
					new CodeMatch(x => x.opcode == OpCodes.Call && (x.operand as MethodInfo)?.Name == nameof(string.Concat), "Call_Concat")
				);

			var text = il.DeclareLocal(typeof(string));

			matcher.Advance(1).Insert(
				new CodeInstruction(OpCodes.Stloc, text),
				new CodeInstruction(OpCodes.Ldc_I4_3),
				new CodeInstruction(OpCodes.Newarr, typeof(string)),

				new CodeInstruction(OpCodes.Dup),
				new CodeInstruction(OpCodes.Ldc_I4_0),
				new CodeInstruction(OpCodes.Ldloc, text),
				new CodeInstruction(OpCodes.Stelem_Ref),

				new CodeInstruction(OpCodes.Dup),
				new CodeInstruction(OpCodes.Ldc_I4_1),
				new CodeInstruction(OpCodes.Ldstr, " "),
				new CodeInstruction(OpCodes.Stelem_Ref),

				new CodeInstruction(OpCodes.Dup),
				new CodeInstruction(OpCodes.Ldc_I4_2),
				matcher.NamedMatch("L_beatmapLevel"),
				Transpilers.EmitDelegate<Func<BeatmapLevel, string>>(level => BeatmapsUtil.ConcatMappers(level.allMappers)),
				new CodeInstruction(OpCodes.Stelem_Ref),

				matcher.NamedMatch("Call_Concat")
			);

			return matcher.InstructionEnumeration();
		}
	}
}
