using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetterSongList.Util {
	public static class PlaylistsUtil {
		public static bool hasPlaylistLib = IPA.Loader.PluginManager.GetPluginFromId("BeatSaberPlaylistsLib") != null;

		public static Dictionary<string, IBeatmapLevelPack> builtinPacks = null;

		public static IBeatmapLevelPack GetPack(string packName) {
			if(packName == null)
				return null;

			if(packName == "Custom Levels") {
				return SongCore.Loader.CustomLevelsPack;
			} else if(packName == "WIP Levels") {
				return SongCore.Loader.WIPLevelsPack;
			}

			if(builtinPacks == null) {
				var p = Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().First(x => x.ostAndExtrasPackCollection != null);

				builtinPacks =
					p.allLoadedBeatmapLevelWithoutCustomLevelPackCollection.beatmapLevelPacks
					// There shouldnt be any duplicate name basegame playlists... But better be safe
					.GroupBy(x => x.shortPackName)
					.Select(x => x.First())
					.ToDictionary(x => x.shortPackName, x => x);
			}

			if(builtinPacks.ContainsKey(packName)) {
				return builtinPacks[packName];
			} else if(hasPlaylistLib) {
				foreach(var x in BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.GetAllPlaylists()) {
					if(x.packName == packName)
						return x;
				}
			}
			return null;
		}

		public static bool IsCollection(IAnnotatedBeatmapLevelCollection levelCollection) {
			return levelCollection is BeatSaberPlaylistsLib.Legacy.LegacyPlaylist || levelCollection is BeatSaberPlaylistsLib.Blist.BlistPlaylist;
		}

		public static IPreviewBeatmapLevel[] GetLevelsForLevelCollection(IAnnotatedBeatmapLevelCollection levelCollection) {
			if(levelCollection is BeatSaberPlaylistsLib.Legacy.LegacyPlaylist legacyPlaylist)
				return legacyPlaylist.BeatmapLevels;
			if(levelCollection is BeatSaberPlaylistsLib.Blist.BlistPlaylist blistPlaylist)
				return blistPlaylist.BeatmapLevels;
			return null;
		}
	}
}
