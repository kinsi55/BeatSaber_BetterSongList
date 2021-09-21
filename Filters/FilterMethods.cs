using BetterSongList.FilterModels;
using BetterSongList.Filters.Models;
using SongDetailsCache.Structs;

namespace BetterSongList {
	static class FilterMethods {
		public static readonly IFilter ranked = new BasicSongDetailsFilter(x => ((Song)x).rankedStatus == RankedStatus.Ranked);
		public static readonly IFilter unranked = new BasicSongDetailsFilter(x => ((Song)x).rankedStatus != RankedStatus.Ranked);
		public static readonly IFilter qualified = new BasicSongDetailsFilter(x => ((Song)x).rankedStatus == RankedStatus.Qualified);
		public static readonly IFilter unplayed = new PlayedFilter(true);
		public static readonly IFilter played = new PlayedFilter();
		public static readonly IFilter requirements = new RequirementsFilter();
	}
}
