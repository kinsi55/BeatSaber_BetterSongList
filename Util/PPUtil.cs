using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterSongList.Util {
    // THANK YOU PULSELANE <3
    static class PPUtil {
        private static (float, float)[] ppCurve = new (float, float)[] {
            (0, 0),
            (0.45f, 0.015f),
            (0.5f, 0.03f),
            (0.55f, 0.06f),
            (0.6f, 0.105f),
            (0.65f, 0.15f),
            (0.7f, 0.22f),
            (0.75f, 0.3f),
            (0.8f, 0.42f),
            (0.86f, 0.6f),
            (0.9f, 0.78f),
            (0.925f, 0.905f),
            (0.945f, 1.015f),
            (0.95f, 1.046f),
            (0.96f, 1.115f),
            (0.97f, 1.2f),
            (0.98f, 1.29f),
            (0.99f, 1.39f),
            (1f, 1.5f)
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
