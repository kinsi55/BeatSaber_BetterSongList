using System;
using System.Collections.Generic;

namespace BetterSongList.Util {
	public static class BeatmapPatternDetection {
		public static bool CheckForCrouchWalls(IEnumerable<ObstacleData> obstacles) {
			if(obstacles == null)
				return false;

			var wallExistence = new float[2];

			foreach(var o in obstacles) {
				// Ignore 1 wide walls on left
				if(o.lineIndex == 3 || (o.lineIndex == 0 && o.width == 1))
					continue;

				// Filter out fake walls, they dont drain energy
				if(o.duration < 0 || o.width <= 0)
					continue;

				// Detect >2 wide walls anywhere, or 2 wide wall in middle
				if(o.width > 2 || (o.width == 2 && o.lineIndex == 1)) {
					var layer = o.lineLayer.ToIntLayer();
					if(layer == 2 || layer != 0 && (o.height - layer >= 2))
						return true;
				}

				// Is the wall on the left or right half?
				var isLeftHalf = o.lineIndex <= 1;

				// Check if the other half has an active wall, which would mean there is one on both halfs
				// I know this technically does not check if one of the halves is half-height, but whatever
				if(wallExistence[isLeftHalf ? 1 : 0] >= o.time)
					return true;

				// Extend wall lengths by 120ms so that staggered crouchwalls that dont overlap are caught
				wallExistence[isLeftHalf ? 0 : 1] = Math.Max(wallExistence[isLeftHalf ? 0 : 1], o.time + o.duration + 0.12f);
			}

			return false;
		}

		static int ToIntLayer(this NoteLineLayer lineLayer) {
			switch(lineLayer) {
				case NoteLineLayer.Base:
					return 0;
				case NoteLineLayer.Upper:
					return 1;
				case NoteLineLayer.Top:
					return 2;
				default:
					return 0;
			}
		}
	}
}