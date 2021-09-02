using BetterSongList.UI;
using BetterSongList.Util;
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

			var l = IPA.Loader.PluginManager.GetPluginFromId("PlaylistManager");

			if(l != null && l.HVersion <= new Hive.Versioning.Version("1.3.0"))
				FilterUI.persistentNuts.ShowErrorASAP("Your version of PlaylistManager is outdated / incompatible with BetterSongList - It has been disabled, please update");

			l = IPA.Loader.PluginManager.GetPluginFromId("BeatSaberPlus");
			if(l.HVersion <= new Hive.Versioning.Version("3.2.9"))
				FilterUI.persistentNuts.ShowErrorASAP("Your version of BeatSaberPlus contains a bug when deleting a map that was selected from Chat requests, please update");

			PlaylistsUtil.Init();
		}

		[OnExit]
		public void OnApplicationQuit() {
			harmony.UnpatchAll(harmony.Id);
			Config.Instance.Changed();
		}
	}
}
