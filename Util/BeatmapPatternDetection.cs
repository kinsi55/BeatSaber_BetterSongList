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

			foreach(var x in beatmapData.beatmapLinesData) {
				foreach(var bme in x.beatmapObjectsData) {
					if(bme.beatmapObjectType != BeatmapObjectType.Obstacle)
						continue;

					var o = (ObstacleData)bme;

					// Filter out fake walls, they dont drain energy
					if(o.duration < 0 || o.width <= 0)
						continue;

					// Detect >2 wide walls anywhere, or 2 wide wall in middle
					if(o.obstacleType == ObstacleType.Top && (o.width > 2 || (o.width == 2 && o.lineIndex == 1)))
						return true;

					// Skip 1 wide walls on outer lanes
					if(o.width == 1 && (o.lineIndex == 1 || o.lineIndex == 4))
						continue;

					// Is the wall on the left or right half?
					var isLeftHalf = o.lineIndex <= 2;

					// Check if the other half has an active wall, which would mean there is one on both halfs
					if(wallExistence[isLeftHalf ? 1 : 0] >= o.time)
						return true;

					wallExistence[isLeftHalf ? 0 : 1] = Math.Max(wallExistence[isLeftHalf ? 0 : 1], o.time + o.duration);
				}
			}
			return false;
		}
	}
}
