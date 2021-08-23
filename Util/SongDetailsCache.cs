using SongDetailsCache;
using System.Threading.Tasks;

namespace BetterSongList.Util {
	static class SongDetailsUtil {
		public static bool finishedInitAttempt { get; private set; } = false;
		public static bool attemptedToInit { get; private set; } = false;

		static bool CheckAvailable() {
			var v = IPA.Loader.PluginManager.GetPluginFromId("SongDetailsCache");

			if(v == null)
				return false;

			return v.HVersion >= new Hive.Versioning.Version("1.1.5");
		}
		public static bool isAvailable => CheckAvailable();
		public static SongDetails instance { get; private set; }

		public static string GetUnavailabilityReason() {
			if(!isAvailable)
				return "Your Version of 'SongDetailsCache' is either outdated, or you are missing it entirely";

			if(finishedInitAttempt && instance == null)
				return "SongDetailsCache failed to initialize for some reason. Try restarting your game, that might fix it";

			return null;
		}

		public static async Task<SongDetails> TryGet() {
			if(!finishedInitAttempt) {
				attemptedToInit = true;
				try {
					if(isAvailable)
						return instance = await SongDetails.Init();
				} catch { } finally {
					finishedInitAttempt = true;
				}
			}
			return instance;
		}
	}
}
