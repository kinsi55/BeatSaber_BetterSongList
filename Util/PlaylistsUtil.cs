using System.Collections.Generic;
using System.Linq;
using BeatSaberPlaylistsLib.Types;
using UnityEngine;

namespace BetterSongList.Util {
	public static class PlaylistsUtil {
		public static bool hasPlaylistLib = false;

		public static void Init() {
			hasPlaylistLib = IPA.Loader.PluginManager.GetPluginFromId("BeatSaberPlaylistsLib") != null;
		}

		public static Dictionary<string, BeatmapLevelPack> packs = null;

		public static BeatmapLevelPack GetPack(string packName) {
			if(packName == null)
				return null;

			if(packs == null) {
				packs =
					SongCore.Loader.BeatmapLevelsModelSO?._allLoadedBeatmapLevelsRepository.beatmapLevelPacks
					// There shouldnt be any duplicate name basegame playlists... But better be safe
					.GroupBy(x => x.shortPackName)
					.Select(x => x.First())
					.ToDictionary(x => x.shortPackName, x => x);
			}

			if(packs.TryGetValue(packName, out var p)) {
				return p;
			} else if(hasPlaylistLib) {
				BeatmapLevelPack wrapper() {
					if(!SongCore.Loader.AreSongsLoaded)
						return null;
					foreach(var x in BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.GetAllPlaylists(true)) {
						var playlistLevelPack = x.PlaylistLevelPack;
						if(x.PlaylistLevelPack.packName == packName)
							return playlistLevelPack;
					}
					return null;
				}
				return wrapper();
			}
			return null;
		}

		public static BeatmapLevel[] GetLevelsForLevelCollection(BeatmapLevelPack levelCollection) {
			if(levelCollection is PlaylistLevelPack playlist)
				return playlist.playlist.BeatmapLevels;
			return null;
		}
	}
}
