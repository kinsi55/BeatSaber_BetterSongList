using BetterSongList.FilterModels;
using BetterSongList.Filters.Models;
using BetterSongList.Util;

namespace BetterSongList {
	static class FilterMethods {
		public static readonly IFilter ranked = new BasicSongDetailsFilter(x => x.rankedStatus == SongDetailsCache.Structs.RankedStatus.Ranked);
		public static readonly IFilter unranked = new BasicSongDetailsFilter(x => x.rankedStatus != SongDetailsCache.Structs.RankedStatus.Ranked);
		public static readonly IFilter qualified = new BasicSongDetailsFilter(x => x.rankedStatus == SongDetailsCache.Structs.RankedStatus.Qualified);
		public static readonly IFilter unplayed = new UnplayedFilter();
	}
}
