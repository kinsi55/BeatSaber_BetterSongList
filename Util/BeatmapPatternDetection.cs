using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace BetterSongList.Util {
	public static class BeatmapPatternDetection {
#if !PRE_1_20
		public static bool CheckForCrouchWalls(List<BeatmapSaveDataVersion3.BeatmapSaveData.ObstacleData> obstacles) {
			if(obstacles == null || obstacles.Count == 0)
				return false;

			var wallExistence = new float[2];

			foreach(var o in obstacles) {
				// Ignore 1 wide walls on left
				if(o.line == 3 || (o.line == 0 && o.width == 1))
					continue;

				// Filter out fake walls, they dont drain energy
				if(o.duration < 0 || o.width <= 0)
					continue;

				// Detect >2 wide walls anywhere, or 2 wide wall in middle
				if(o.width > 2 || (o.width == 2 && o.line == 1)) {
					if(o.layer == 2 || o.layer != 0 && (o.height - o.layer >= 2))
						return true;
				}

				// Is the wall on the left or right half?
				var isLeftHalf = o.line <= 1;

				// Check if the other half has an active wall, which would mean there is one on both halfs
				// I know this technically does not check if one of the halves is half-height, but whatever
				if(wallExistence[isLeftHalf ? 1 : 0] >= o.beat)
					return true;

				// Extend wall lengths by 120ms so that staggered crouchwalls that dont overlap are caught
				wallExistence[isLeftHalf ? 0 : 1] = Math.Max(wallExistence[isLeftHalf ? 0 : 1], o.beat + o.duration + 0.12f);
			}
			return false;
		}
#else
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

				// Extend wall lengths by 120ms so that staggered crouchwalls that dont overlap are caught
				wallExistence[isLeftHalf ? 0 : 1] = Math.Max(wallExistence[isLeftHalf ? 0 : 1], o.time + o.duration + 0.12f);
			}
			return false;
		}
#endif
	}
}
