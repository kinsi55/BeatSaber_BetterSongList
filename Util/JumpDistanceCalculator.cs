using System;

namespace BetterSongList.Util {
	static class JumpDistanceCalculator {
		static float hjd(float bpm, float njs, float offset) {
			var noteJumpMovementSpeed = (njs * bpm) / bpm;
			var num = 60f / bpm;
			var hjd = 4f;
			while(noteJumpMovementSpeed * num * hjd > 18f)
				hjd /= 2f;

			hjd += offset;

			return Math.Max(hjd, 1);
		}

		public static float GetJd(float bpm, float njs, float offset) {
			return njs * (60f / bpm) * hjd(bpm, njs, offset) * 2;
		}

		public static float GetMinJd(float bpm, float njs) {
			return njs * (60f / bpm) * 2;
		}
	}
}
