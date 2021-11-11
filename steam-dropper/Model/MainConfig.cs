
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace steam_dropper.Model
{
    public class MainConfig
    {
        public string DropHistoryFolder { get; set; }

        public int ParallelCount { get; set; }

        public TimeConfig TimeConfig { get; set; }

        public int StartTimeOut { get; set; }

        public DropGameList GlobalDropList { get; set; }

        public void Load(MainConfig configFile)
        {
            this.DropHistoryFolder = configFile.DropHistoryFolder;
            this.ParallelCount = configFile.ParallelCount;
            this.TimeConfig = configFile.TimeConfig;
            this.StartTimeOut = configFile.StartTimeOut;
            this.GlobalDropList = configFile.GlobalDropList;
        }
    }
}
