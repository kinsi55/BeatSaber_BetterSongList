using HarmonyLib;
using System;
using System.Reflection;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch]
	static class BlockSongDataCoreLoad {
		public static bool doBlock = false;
		static bool Prefix() => !doBlock;

		static MethodBase TargetMethod() => IPA.Loader.PluginManager.GetPluginFromId("SongDataCore")?
			.Assembly.GetType("SongDataCore.Plugin")?
			.GetMethod("LoadDatabases", BindingFlags.Instance | BindingFlags.NonPublic);
		static Exception Cleanup(Exception ex) => null;
	}
}
