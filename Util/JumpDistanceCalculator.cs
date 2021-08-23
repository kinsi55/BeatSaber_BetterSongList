using System;

namespace BetterSongList.Util {
	static class JumpDistanceCalculator {
		static float hjd(IDifficultyBeatmap lvl, float? overrideOffset = null) {
			var noteJumpMovementSpeed = (lvl.noteJumpMovementSpeed * lvl.level.beatsPerMinute) / lvl.level.beatsPerMinute;
			var num = 60f / lvl.level.beatsPerMinute;
			var hjd = 4f;
			while(noteJumpMovementSpeed * num * hjd > 18f)
				hjd /= 2f;

			hjd += overrideOffset ?? lvl.noteJumpStartBeatOffset;

			return Math.Max(hjd, 1);
		}

		public static float GetJd(IDifficultyBeatmap diff, float? overrideOffset = null) {
			return diff.noteJumpMovementSpeed * (60f / diff.level.beatsPerMinute) * hjd(diff, overrideOffset) * 2;
		}

		public static float GetMinJd(IDifficultyBeatmap diff) {
			return diff.noteJumpMovementSpeed * (60f / diff.level.beatsPerMinute) * 2;
		}
	}
}
