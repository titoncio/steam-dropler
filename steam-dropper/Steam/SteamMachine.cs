using Newtonsoft.Json;
using steam_dropper.Model;
using SteamKit2;
using SteamKit2.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace steam_dropper.Steam
{
    public class SteamMachine
    {

        public bool IsWork => _work;

        public SteamID SteamID => _client.SteamID;
        public uint AccountId => _client.SteamID.AccountID;

        public SteamConfiguration SteamConfiguration { get; private set; }
        public bool IsConnectedAndLoggedOn => _client?.SteamID != null;


        private readonly Account _steamAccount;
        private readonly SteamLoginHandler _loginHandler;
        private readonly SteamClient _client;
        private readonly SteamApps _steamApps;

        private readonly SteamUnifiedMessages.UnifiedService<IInventory> _inventoryService;
        //private readonly SteamUnifiedMessages.UnifiedService<IDeviceAuth> _deviceService;

        private bool _work = true;

        public SteamMachine(Account steamAccount)
        {
            _client = new SteamClient();
            SteamConfiguration = _client.Configuration;
            var manager = new CallbackManager(_client);

            var steamUnifiedMessages = _client.GetHandler<SteamUnifiedMessages>();
            _inventoryService = steamUnifiedMessages.CreateService<IInventory>();
            //_deviceService = steamUnifiedMessages.CreateService<IDeviceAuth>();
            _steamAccount = steamAccount;
            _loginHandler = new SteamLoginHandler(_steamAccount, _client, manager);
            _steamApps = _client.GetHandler<SteamApps>();

            Task.Run(() =>
            {
                while (_work)
                {
                    manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
                }

            });
        }



        public async Task<EResult> EasyIdling()
        {
            var res = await _loginHandler.Login(SteamServerList.GetServerRecord());

            if (res == EResult.OK)
            {

                _steamAccount.LastRun = DateTime.UtcNow;
                _steamAccount.IdleNow = true;
                _steamAccount.Save();

                var appId = _steamAccount.AppIds;

                if (appId.Any())
                {
                    for (int i = 0; i < _steamAccount.TimeConfig.IdleTime / 30; i++)
                    {
                        PlayGames(appId);
                        await CheckTimeItemsList(_steamAccount.DropList);
                        Thread.Sleep(1000 * 60 * 30);
                    }
                    await CheckTimeItemsList(_steamAccount.DropList);
                    StopGame();
                }

                _steamAccount.IdleNow = false;
                _steamAccount.Save();
            }

            return res;
        }

        public async Task<T> Execute<T>(Func<SteamMachine, T> func)
        {
            var res = await _loginHandler.Login(SteamServerList.GetServerRecord());

            if (res == EResult.OK)
            {
                var ret = func(this);
                LogOff();
                return ret;
            }
            LogOff();
            return default(T);
        }

        public void LogOff()
        {
            Thread.Sleep(5000);
            _work = false;
            _client.Disconnect();
            SteamServerList.ReleasServerRecord(_loginHandler.ServerRecord);

        }


        private async Task AddFreeLicense(List<uint> gamesIds)
        {
            var result = await _steamApps.RequestFreeLicense(gamesIds);
        }

        private async Task CheckTimeItemsList(DropGameList pairs)
        {
            List<uint> FarmList = new List<uint>();
            foreach (var pair in pairs)
            {
                FarmList.Add(pair.AppId);
            }
            await AddFreeLicense(FarmList);


            foreach (var pair in pairs)
            {
                Console.WriteLine($"Drop process started. Account: {_client.SteamID.ConvertToUInt64()} - App: {pair.AppId} - Time: {DateTime.Now.ToShortTimeString()}");

                CInventory_ConsumePlaytime_Request reqkf = new CInventory_ConsumePlaytime_Request
                {
                    appid = pair.AppId,
                    itemdefid = pair.DefId
                };
                var response = await _inventoryService.SendMessage(x => x.ConsumePlaytime(reqkf));
                var result = response.GetDeserializedResponse<CInventory_Response>();
                if (result.item_json != "[]")
                {
                    try
                    {
                        var items = JsonConvert.DeserializeObject<DropResult[]>(result.item_json);
                        foreach (var item in items)
                        {
                            Util.LogDrop(_steamAccount.Name, pair.AppId, item);
                            Console.WriteLine($"Dropped! {DateTime.Now} - AppID: {pair.AppId} - Item: {item.ItemDefId} ({item.ItemId})");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    
                    
                }

            }
        }

        private void StopGame()
        {
            var games = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
            games.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed()
            {
                game_id = 0
            });
            _client.Send(games);
        }

        private void PlayGames(List<uint> gamesIds)
        {
            var games = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            foreach (var gameId in gamesIds)
            {
                games.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
                {
                    game_id = new GameID(gameId),
                });
            }

            _client.Send(games);

        }

    }
}
