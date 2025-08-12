using Solution.Common.Misc;

using UnityEngine;

namespace Solution.Common.Assets {
    public class LoggingSettings : ScriptableObject {
        public ICommonLogger commonLogger = ICommonLogger.CreateDefaultLogger();
    }
}