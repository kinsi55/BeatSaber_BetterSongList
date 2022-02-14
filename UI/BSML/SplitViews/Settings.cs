using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BetterSongList.HarmonyPatches.UI;
using HMUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BetterSongList.UI.SplitViews {
	class Settings {
		public static readonly Settings instance = new Settings();
		Settings() { }

		[UIParams] readonly BSMLParserParams parserParams = null;

		Config cfgi => Config.Instance;
		readonly string version = $"BetterSongList v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)} by Kinsi55";

		static void SettingsClosed() {
			SongDeleteButton.UpdateState();
			ScrollEnhancements.UpdateState();
			ExtraLevelParams.UpdateState();
			Config.Instance.Changed();
		}


		[UIComponent("sponsorsText")] CurvedTextMeshPro sponsorsText = null;
		void OpenSponsorsLink() => Process.Start("https://github.com/sponsors/kinsi55");
		void OpenSponsorsModal() {
			parserParams.EmitEvent("CloseSettings");
			sponsorsText.text = "Loading...";
			Task.Run(() => {
				string desc = "Failed to load";
				try {
					desc = (new WebClient()).DownloadString("http://kinsi.me/sponsors/bsout.php");
				} catch { }

				_ = IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => {
					sponsorsText.text = desc;
					// There is almost certainly a better way to update / correctly set the scrollbar size...
					sponsorsText.gameObject.SetActive(false);
					sponsorsText.gameObject.SetActive(true);
				});
			}).ConfigureAwait(false);
		}
	}
}
