using Newtonsoft.Json;
using steam_dropper.Model;
using steam_dropper.Steam;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace steam_dropper
{
    public static class Worker
    {
        private static readonly string CurrentFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static readonly string ConfigFolder = CurrentFolder + @"\Configs";
        private static readonly string AccountsFolder = ConfigFolder + @"\Accounts";
        private static readonly string MaFilesFolder = ConfigFolder + @"\maFiles";
        private static readonly string DropHistoryFolder = CurrentFolder + @"\DropHistory";

        private static HashSet<Account> _accounts;

        private static readonly Dictionary<string, Task> TaskDictionary = new Dictionary<string, Task>();

        private static Timer _timer;

        public static void Start()
        {
            _timer = new Timer(1000 * MainConfig.Config.StartTimeOut);
            _timer.Elapsed += CheckToAdd;
            _timer.Start();

            Console.WriteLine($"Login delay: {MainConfig.Config.StartTimeOut} seconds" +
                $"\nTotal accounts: {_accounts.Count}" +
                $"\nStarting accounts: {_accounts.Count(t => t.IdleEnable && t.MobileAuth?.SharedSecret != null)}");

        }

        public static void Run()
        {
            Console.WriteLine("Loading config");
            MainConfig.Config = JsonConvert.DeserializeObject<MainConfig>(File.ReadAllText(ConfigFolder + @"\MainConfig.json"));
            LoadAccounts();
            LoadMaFiles();
            Start();
        }

        private static void CheckToAdd(object sender, ElapsedEventArgs e)
        {
            if (TaskDictionary.Count > MainConfig.Config.ParallelCount)
            {
                return;
            }

            var steamaccount = _accounts.FirstOrDefault(t =>
                t.LastRun < DateTime.UtcNow.AddHours(-t.TimeConfig.PauseBetweenIdleTime) & t.IdleEnable && !t.IdleNow);

            if (steamaccount != null)
            {
                AddToIdlingQueue(steamaccount);
            }

        }


        private static void AddToIdlingQueue(Account account)
        {
            var id = Guid.NewGuid().ToString();

            TaskDictionary[id] = Task.Run(async () =>
            {
                try
                {
                    var machine = new SteamMachine(account);
                    var result = await machine.EasyIdling();
                    if (result != EResult.OK)
                    {
                        Console.WriteLine($"Not login {result}");
                    }
                    machine.LogOff();
                    TaskDictionary.Remove(id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            });


        }

        private static void LoadAccounts()
        {
            _accounts = new HashSet<Account>();

            if (Directory.Exists(AccountsFolder))
            {
                var jsonPaths = Directory.GetFiles(AccountsFolder).Where(t => Path.GetExtension(t) == ".json");

                foreach (var jsonPath in jsonPaths)
                {
                    if (!jsonPath.EndsWith("example.json"))
                    {
                        if (MainConfig.Config.DebugMode == 1)
                            Console.WriteLine($"Adding account from {Path.GetFileName(jsonPath)}");
                        _accounts.Add(new Account(jsonPath));
                    }
                }

            }
            else
            {
                throw new Exception("Account folder not found");
            }

        }

        private static void LoadMaFiles()
        {
            var objects = new List<MobileAuth>();
            if (!string.IsNullOrEmpty(MaFilesFolder) && Directory.Exists(MaFilesFolder))
            {
                var maFilePaths = Directory.GetFiles(MaFilesFolder).Where(t => Path.GetExtension(t) == ".maFile");
                foreach (var maFile in maFilePaths)
                {
                    if (!maFile.EndsWith("example.maFile"))
                    {
                        if (MainConfig.Config.DebugMode == 1)
                            Console.WriteLine($"Adding auth from {Path.GetFileName(maFile)}");
                        var obj = JsonConvert.DeserializeObject<MobileAuth>(File.ReadAllText(maFile));
                        _accounts.First<Account>(a => a.Name == Path.GetFileNameWithoutExtension(maFile)).MobileAuth = obj;
                    }
                }
            }
            else
            {
                Console.WriteLine("MaFile folder not found");
                throw new Exception();
            }

        }

        public static void LogDrop(string accountName, uint game, DropResult result)
        {
            if (!Directory.Exists(DropHistoryFolder))
            {
                if (MainConfig.Config.DebugMode == 1)
                    Console.WriteLine($"Creating drop history folder {DropHistoryFolder}");
                Directory.CreateDirectory(DropHistoryFolder);
            }
            if (File.Exists(Path.Combine(DropHistoryFolder, $"{accountName}.txt")))
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(DropHistoryFolder, $"{accountName}.txt")))
                {
                    sw.WriteLine($"Dropped! {DateTime.Now} - AppID: {game} - Item: {result.ItemDefId} ({result.ItemId})");
                }
            }
            else
            {
                using (StreamWriter sw = File.CreateText(Path.Combine(DropHistoryFolder, $"{accountName}.txt")))
                {
                    sw.WriteLine($"Dropped! {DateTime.Now} - AppID: {game} - Item: {result.ItemDefId} ({result.ItemId})");
                }
            }
        }
    }
}
