using System;

namespace BetterSongList.Util {
	static class JumpDistanceCalculator {
		static float hjd(float bpm, float njs, float offset) {
			var num = 60f / bpm;
			var hjd = 4f;
			while(njs * num * hjd > 17.999f)
				hjd /= 2f;

			hjd += offset;

			return Math.Max(hjd, 0.25f);
		}

		public static float GetJd(float bpm, float njs, float offset) {
			return njs * (60f / bpm) * hjd(bpm, njs, offset) * 2;
		}
	}
}
