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

			// TODO: Remove once <1.18.1 support is dropped
			var l = IPA.Utilities.UnityGame.GameVersion.SemverValue;

			return Math.Max(hjd, l.Minor > 18 || (l.Minor == 18 && l.Patch >= 1) ? 0.25f : 1);
		}

		public static float GetJd(float bpm, float njs, float offset) {
			return njs * (60f / bpm) * hjd(bpm, njs, offset) * 2;
		}
	}
}
