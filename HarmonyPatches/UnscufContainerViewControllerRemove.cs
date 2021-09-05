using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using HMUI;

namespace BetterSongList.HarmonyPatches {
	/*
	 * In LevelSearchViewController:ResetCurrentFilterParams, EventSystem.current some times evaluates to null,
	 * which will then break the UI until you reload. This adds a nullcheck to that call to fix that for now.
	 * I'm not sure why this is such a big problem with BetterSongList but oh well (Personally I cant reproduce it)
	 */
	[HarmonyPatch]
	static class UnscufContainerViewControllerRemove {
		static MethodBase TargetMethod() => AccessTools.FirstInner(typeof(ContainerViewController), t => t.Name.StartsWith($"<RemoveViewControllers"))?.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance);

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
			if(instructions.ElementAt(44).opcode != OpCodes.Call && instructions.ElementAt(46).opcode != OpCodes.Callvirt) {
				Plugin.Log.Warn("UnscufContainerViewControllerRemove not in effect - Unexpected opcodes");
				return instructions;
			}

			var l = instructions.ToList();

			var afterSetSelected = il.DefineLabel();
			l[47].labels.Add(afterSetSelected);

			l.InsertRange(44, new[] {
				// Call EventSystem.current
				l[44],
				// Load Null
				new CodeInstruction(OpCodes.Ldnull),
				// Check if is null
				new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Object), "op_Equality")),
				// Skip the SetSelectedGameObject() call if null
				new CodeInstruction(OpCodes.Brtrue, afterSetSelected)
			});

#if DEBUG
			Plugin.Log.Warn("UnscufContainerViewControllerRemove applied");
#endif
			return l;
		}
	}
}
