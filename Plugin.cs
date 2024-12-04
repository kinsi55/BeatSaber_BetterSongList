using BetterSongList.UI;
using BetterSongList.Util;
using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using System.Reflection;
using System.Linq;
using IPALogger = IPA.Logging.Logger;

namespace BetterSongList {

	[Plugin(RuntimeOptions.SingleStartInit)]
	public class Plugin {
		internal static Plugin Instance { get; private set; }
		internal static IPALogger Log { get; private set; }
		internal static Harmony harmony { get; private set; }

		[Init]
		public void Init(IPALogger logger, IPA.Config.Config conf) {
			Instance = this;
			Log = logger;

			Config.Instance = conf.Generated<Config>();
		}

		[OnStart]
		public void OnApplicationStart() {
			harmony = new Harmony("Kinsi55.BeatSaber.BetterSongList");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			PlaylistsUtil.Init();

			PPUtil.Init();
		}

		[OnExit]
		public void OnApplicationQuit() {
			harmony.UnpatchSelf();
		}
	}
}
