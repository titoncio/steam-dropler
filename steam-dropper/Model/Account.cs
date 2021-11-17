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

        public string Login {  get; set; }

        public string Password { get; set; }

        public ulong? SteamId { get; set; }

        public bool IdleEnable { get; set; }

        public DateTime? LastRun { get; set; }

		[JsonIgnore]
		public MobileAuth MobileAuth { get; set; }

        private string FilePath { get; }

        public string LoginKey { get; set; }

        public byte[] SentryHash { get; set; }

        public string SharedSecret { get; set; }

        public DropGameList DropList { get; set; }

        [JsonIgnore]
        public DropGameList DropGameList { get; set; }

        public TimeConfig TimeConfig { get; set; }

        public Account()
        {
        }

        public Account(string path, MainConfig mainConfig)
        {
           var obj = JsonConvert.DeserializeObject<Account>(File.ReadAllText(path));

            Name = Path.GetFileNameWithoutExtension(path);
            Login = obj.Login;
            Password = obj.Password;
            SteamId = obj.SteamId;
            IdleEnable = obj.IdleEnable;
            LastRun = obj.LastRun ?? DateTime.MinValue;
            MobileAuth = SharedSecret != null ? new MobileAuth { SharedSecret = obj.SharedSecret } : MobileAuth;
            FilePath = path;
            LoginKey = obj.LoginKey;
            SentryHash = obj.SentryHash;
            SharedSecret = obj.SharedSecret;
            DropGameList = obj.DropList;
            ImportGlobalDrop(mainConfig.GlobalDropList);
            TimeConfig = obj.TimeConfig ?? mainConfig.TimeConfig ?? new TimeConfig();
        }

        private void ImportGlobalDrop(DropGameList globalGames)
        {
            if (globalGames == null)
            {
                Console.WriteLine("No GlobalDropConfig");
                return;
            }

            foreach (var dropConfig in globalGames)
            {
                if (!this.DropGameList.Exists(dc => dc.AppId == dropConfig.AppId))
                {
                    this.DropGameList.Add(dropConfig);
                }
            }
        }

        public void Save()
        {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this));
        }

    }
}
