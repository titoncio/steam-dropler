using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using steam_dropler.Steam;

namespace steam_dropler.Model
{
    public class AccountConfig
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

        public List<(uint, ulong)> DropConfig { get; set; }

        [JsonIgnore]
        public List<uint> AppIds => DropConfig?.Select(t=>t.Item1).ToList();

        public TimeConfig TimeConfig { get; set; }
        
        public AccountConfig()
        {
        }

        public AccountConfig(string path)
        {
           var obj = JsonConvert.DeserializeObject<AccountConfig>(File.ReadAllText(path));

            Password = obj.Password;
            SteamId = obj.SteamId;
            IdleEnable = obj.IdleEnable;
            DropConfig = obj.DropConfig ?? new List<(uint, ulong)>();
            SentryHash = obj.SentryHash;
            LoginKey = obj.LoginKey;
            IdleNow = obj.IdleNow;
            LastRun = obj.LastRun ?? DateTime.MinValue;
            SharedSecret = obj.SharedSecret;
            if (SharedSecret != null)
            {
                MobileAuth = new MobileAuth {SharedSecret = obj.SharedSecret};
            }

            if (IdleNow )
            {
                IdleNow = false;
                if ((DateTime.UtcNow - LastRun.Value).TotalHours < 10)
                {
                    LastRun = DateTime.MinValue;
                }
            }


            Name = Path.GetFileNameWithoutExtension(path);
            TimeConfig = obj.TimeConfig ?? MainConfig.Config.TimeConfig ?? new TimeConfig {IdleTime = 60,  PauseBeatwinIdleTime = 660} ;
            FilePath = path;
        }

        public void Save()
        {
            if (MainConfig.Config.DebugMode == 1)
                Console.WriteLine($"Updating file for account: {SteamId}");
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this));
        }

    }
}
