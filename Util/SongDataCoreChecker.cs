using System.Collections.Generic;
using System.Linq;

namespace BetterSongList.Util {
	static class SongDataCoreChecker {
		public static bool didCheck { get; private set; } = false;
		public static bool IsInstalled() {
			didCheck = true;
			return IPA.Loader.PluginManager.GetPluginFromId("SongDataCore") != null;
		}

		public static bool IsUsed() {
			foreach(var x in IPA.Loader.PluginManager.EnabledPlugins) {
				var deps = IPA.Utilities.ReflectionUtil.GetProperty<HashSet<IPA.Loader.PluginMetadata>, IPA.Loader.PluginMetadata>(x, "Dependencies");

				if(deps.Any(x => x.Id == "SongDataCore"))
					return true;
			}

			return false;
		}
	}
}
