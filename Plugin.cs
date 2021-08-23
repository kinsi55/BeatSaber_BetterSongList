using BetterSongList.UI;
using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using System.Reflection;
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
			FilterUI.Init();
		}

		[OnExit]
		public void OnApplicationQuit() {
			harmony.UnpatchAll(harmony.Id);
			Config.Instance.Changed();
		}
	}
}
