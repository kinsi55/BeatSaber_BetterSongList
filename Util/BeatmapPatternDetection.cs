using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterSongList.Util {
	static class BeatmapPatternDetection {
		public static bool CheckForCrouchWalls(BeatmapData beatmapData) {
			if(beatmapData.obstaclesCount == 0)
				return false;

			var wallExistence = new float[2];

			foreach(var bme in beatmapData
				.beatmapLinesData
				.SelectMany(x => x.beatmapObjectsData)
				.Where(x => x.beatmapObjectType == BeatmapObjectType.Obstacle && x.lineIndex != 3)
				.OrderBy(x => x.time)
			) {
				var o = (ObstacleData)bme;

				// Ignore 1 wide walls on left
				if(o.lineIndex == 0 && o.width == 1)
					continue;

				// Filter out fake walls, they dont drain energy
				if(o.duration < 0 || o.width <= 0)
					continue;

				// Detect >2 wide walls anywhere, or 2 wide wall in middle
				if(o.obstacleType == ObstacleType.Top && (o.width > 2 || (o.width == 2 && o.lineIndex == 1)))
					return true;

				// Is the wall on the left or right half?
				var isLeftHalf = o.lineIndex <= 1;

				// Check if the other half has an active wall, which would mean there is one on both halfs
				// I know this technically does not check if one of the halves is half-height, but whatever
				if(wallExistence[isLeftHalf ? 1 : 0] >= o.time)
					return true;

				// Extend wall lengths by 200ms so that staggered crouchwalls that dont overlap are caught
				wallExistence[isLeftHalf ? 0 : 1] = Math.Max(wallExistence[isLeftHalf ? 0 : 1], o.time + o.duration + 0.2f);
			}
			return false;
		}
	}
}
