
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace steam_dropper.Model
{
    public class MainConfig
    {
        [JsonIgnore]
        public static MainConfig Config { get; set; }

        public int ParallelCount { get; set; }

        public TimeConfig TimeConfig { get; set; }

        public int StartTimeOut { get; set; }

        public int DebugMode { get; set; }
    }
}
