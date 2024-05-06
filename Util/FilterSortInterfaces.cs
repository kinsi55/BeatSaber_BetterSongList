using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetterSongList.SortModels;
using static SelectLevelCategoryViewController;

#nullable enable

namespace BetterSongList.Interfaces {
	public interface IAvailabilityCheck {
		public string GetUnavailabilityReason();
	}

	public interface ITransformerPlugin : ISorter {
		public string name { get; }
		public bool visible { get; }

		/// <summary>
		/// Called whenever the selected Tab or Playlist is switched to allow the Sort / Filter
		/// to dynamically hide / show itself where applicable
		/// </summary>
		/// <param name="levelCategory"></param>
		/// <param name="playlist"></param>
		public void ContextSwitch(LevelCategory levelCategory, BeatmapLevelPack? playlist);
	}
}
