namespace BetterSongList.Util {
	static class XD {
		public static T FunnyNull<T>(T a) where T : UnityEngine.Object {
			if(a == null)
				return null;

			return a;
		}
	}
}
