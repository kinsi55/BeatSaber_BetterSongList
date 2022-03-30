using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterSongList.Util {
    // THANK YOU PULSELANE <3
    static class PPUtil {
        private static (float, float)[] ppCurve = new (float, float)[] {
			(0.0f, 0.0f),
			(0.6f, 0.25f),
			(0.65f, 0.29f),
			(0.7f, 0.34f),
			(0.75f, 0.40f),
			(0.8f, 0.47f),
			(0.825f, 0.51f),
			(0.85f, 0.57f),
			(0.875f, 0.655f),
			(0.9f, 0.75f),
			(0.91f, 0.79f),
			(0.92f, 0.835f),
			(0.93f, 0.885f),
			(0.94f, 0.94f),
			(0.95f, 1f),
			(0.955f, 1.045f),
			(0.96f, 1.11f),
			(0.965f, 1.20f),
			(0.97f, 1.31f),
			(0.9725f, 1.37f),
			(0.975f, 1.45f),
			(0.9775f, 1.57f),
			(0.98f, 1.71f),
			(0.9825f, 1.88f),
			(0.985f, 2.1f),
			(0.9875f, 2.38f),
			(0.99f, 2.73f),
			(0.9925f, 3.17f),
			(0.995f, 3.76f),
			(0.9975f, 4.7f),
			(0.999f, 5.8f),
			(1f, 7f)
		};

        // Pre-compute to save on division operator
        private static float[] slopes;

        public static void Init() {
            if(slopes != null)
                return;

            slopes = new float[ppCurve.Length - 1];
            for(var i = 0; i < ppCurve.Length - 1; i++) {
                var x1 = ppCurve[i].Item1;
                var y1 = ppCurve[i].Item2;
                var x2 = ppCurve[i + 1].Item1;
                var y2 = ppCurve[i + 1].Item2;

                var m = (y2 - y1) / (x2 - x1);
                slopes[i] = m;
            }
        }

        public static float PPPercentage(float accuracy) {
            if(accuracy >= 1f)
                return 1.5f;

            if(accuracy <= 0)
                return 0f;

            var i = -1;

            foreach((var score, var given) in ppCurve) {
                if(score > accuracy)
                    break;
                i++;
            }

            var lowerScore = ppCurve[i].Item1;
            var higherScore = ppCurve[i + 1].Item1;
            var lowerGiven = ppCurve[i].Item2;
            var higherGiven = ppCurve[i + 1].Item2;
            return Lerp(lowerScore, lowerGiven, higherScore, higherGiven, accuracy, i);
        }

        private static float Lerp(float x1, float y1, float x2, float y2, float x3, int i) {
            float m;
            if(slopes != null) {
                m = slopes[i];
            } else {
                m = (y2 - y1) / (x2 - x1);
            }

            return m * (x3 - x1) + y1;
        }
    }
}
