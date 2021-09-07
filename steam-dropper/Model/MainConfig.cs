
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace steam_dropper.Model
{
    public class MainConfig
    {
        private const string ConfigPath = "Configs\\MainConfig.json";

        [JsonIgnore]
        public static MainConfig Config { get; set; }

        public string MaFileFolder = "D:\\dropper\\steam-dropper\\Configs\\maFiles"; //Temp, just while testing

        public string DropHistoryFolder { get; set; }

        public int ParallelCount { get; set; }

        public TimeConfig TimeConfig { get; set; }

        public int StartTimeOut { get; set; }

        public int DebugMode { get; set; }

        public static void Load()
        {
            var obj = JsonConvert.DeserializeObject<MainConfig>(File.ReadAllText(ConfigPath));
            Config = obj;

        }
    }
}
