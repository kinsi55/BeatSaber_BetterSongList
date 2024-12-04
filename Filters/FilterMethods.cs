using BetterSongList.FilterModels;
using BetterSongList.Interfaces;
using BetterSongList.UI;
using SongDetailsCache.Structs;
using System;
using System.Collections.Generic;

namespace BetterSongList {
	public static class FilterMethods {
		public static readonly IFilter ranked = new BasicSongDetailsFilter(x => ((Song)x).rankedStatus == RankedStatus.Ranked);
		public static readonly IFilter unranked = new BasicSongDetailsFilter(x => ((Song)x).rankedStatus != RankedStatus.Ranked);
		public static readonly IFilter qualified = new BasicSongDetailsFilter(x => ((Song)x).rankedStatus == RankedStatus.Qualified);
		public static readonly IFilter unplayed = new PlayedFilter(true);
		public static readonly IFilter played = new PlayedFilter();
		public static readonly IFilter requirements = new RequirementsFilter();

		internal static Dictionary<string, IFilter> methods = new Dictionary<string, IFilter>() {
			{ "Ranked", ranked },
			{ "Qualified", qualified },
			{ "Unplayed", unplayed },
			{ "Played", played },
			{ "Requirements", requirements },
			{ "Unranked", unranked },
			{ "All", null }
		};

		public static bool Register<T>(T filter) where T: ITransformerPlugin, IFilter {
			var name = filter.name;

			if(name.Length > 20)
				throw new ArgumentException("The name of the Transformer cannot exceed 20 Characters");

			if(!Config.Instance.AllowPluginSortsAndFilters)
				return false;

			if(FilterUI.initialized)
				throw new ArgumentException("You must register your Transformer before the Song List UI is initialized / parsed");

			name = $"🔌{name}";

			methods.Add(name, filter);

#if DEBUG
			Plugin.Log.Info(string.Format("Registered Filter {0}", name));
#endif

			return true;
		}
	}
}
