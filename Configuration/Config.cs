﻿using IPA.Config.Stores;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace BetterSongList {
	internal class Config {
		public static Config Instance { get; set; }
		public string SettingsSeenInVersion { get; set; } = "";
		
		public bool SortAsc { get; set; } = false;
		public string LastSort { get; set; } = "Default";
		//public virtual bool InvertFilter { get; set; } = false;
		public string LastFilter { get; set; } = "";

		public string LastSong { get; set; } = "";
		public string LastCategory { get; set; } = "All";
		public string LastPack { get; set; } = "";

		public bool AllowWipDelete { get; set; } = false;
		public bool AutoFilterUnowned { get; set; } = true;
		public bool ClearFiltersOnPlaylistSelect { get; set; } = true;
		public bool ModBasegameSearch { get; set; } = true;
		public bool ExtendSongsScrollbar { get; set; } = true;
		public float AccuracyMultiplier { get; set; } = 1f;
		public bool EnableAlphabetScrollbar { get; set; } = true;

		public bool AllowPluginSortsAndFilters { get; set; } = true;

		public string PreferredLeaderboard { get; set; } = "ScoreSaber";

		/// <summary>
		/// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
		/// </summary>
		public virtual void OnReload() {

		}

		/// <summary>
		/// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
		/// </summary>
		public virtual void Changed() {
			// Do stuff when the config is changed.
		}

		/// <summary>
		/// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
		/// </summary>
		public virtual void CopyFrom(Config other) {
			// This instance's members populated from other
		}
	}
}
