using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using steam_dropper.Steam;

namespace steam_dropper.Model
{
    public class Account
    {

        [JsonIgnore]
        public string Name { get; set; }

        public string Password { get; set; }

        public ulong? SteamId { get; set; }

        public bool IdleEnable { get; set; }

        public DateTime? LastRun { get; set; }

        public bool IdleNow { get; set; }

		[JsonIgnore]
		public MobileAuth MobileAuth { get; set; }

        private string FilePath { get; }

        public string LoginKey { get; set; }

        public byte[] SentryHash { get; set; }

        public string SharedSecret { get; set; }

        public DropGameList DropList { get; set; }

        [JsonIgnore]
        public List<uint> AppIds => DropList?.Select(t=>t.AppId).ToList();

        public TimeConfig TimeConfig { get; set; }

        public Account()
        {
        }

        public Account(string path, MainConfig mainConfig)
        {
           var obj = JsonConvert.DeserializeObject<Account>(File.ReadAllText(path));

            Password = obj.Password;
            SteamId = obj.SteamId;
            IdleEnable = obj.IdleEnable;
            SentryHash = obj.SentryHash;
            LoginKey = obj.LoginKey;
            IdleNow = obj.IdleNow;
            LastRun = obj.LastRun ?? DateTime.MinValue;
            SharedSecret = obj.SharedSecret;
            DropList = obj.DropList;
            ImportGlobalDrop(mainConfig.GlobalDropList);

            MobileAuth = SharedSecret != null ? new MobileAuth { SharedSecret = obj.SharedSecret } : MobileAuth;

            if (IdleNow)
            {
                IdleNow = false;
                if ((DateTime.UtcNow - LastRun.Value).TotalHours < 10)
                {
                    LastRun = DateTime.MinValue;
                }
            }

            Name = Path.GetFileNameWithoutExtension(path);
            TimeConfig = obj.TimeConfig ?? mainConfig.TimeConfig ?? new TimeConfig {IdleTime = 120,  PauseBetweenIdleTime = 300} ;
            FilePath = path;
        }

        private void ImportGlobalDrop(DropGameList globalGames)
        {
            if (globalGames == null)
            {
                Console.WriteLine("No GlobalDropConfig");
            }

            foreach (var dropConfig in globalGames)
            {
                if (!this.DropList.Exists(dc => dc.AppId == dropConfig.AppId))
                {
                    this.DropList.Add(dropConfig);
                }
            }
        }

        public void Save()
        {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this));
            
        }

    }
}
