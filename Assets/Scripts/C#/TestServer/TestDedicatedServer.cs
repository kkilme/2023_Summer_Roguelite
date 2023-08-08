using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Multiplay;
using Unity.Services.Core;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using Unity.Services.Matchmaker.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Matchmaker;
using Cysharp.Threading.Tasks.CompilerServices;

public class TestDedicatedServer : MonoBehaviour
{
    public static event System.Action ClientInstance;

    private const string InternalServerIP = "0.0.0.0";
    private string _externalServerIP = "0.0.0.0";
    private ushort _serverPort = 7777;

    private string _externalConnectionString => $"{_externalServerIP}:{_serverPort}";

    private IMultiplayService _multiplayService;
    private string _allocationId;
    private MultiplayEventCallbacks _serverCallbacks;
    private IServerEvents _serverEvents;
    private BackfillTicket _localBackfillTicket;
    private CreateBackfillTicketOptions _createBackfillTicketOptions;
    private MatchmakingResults _payload;

    private bool _backfilling = false;

    async void Awake()
    {
        bool server = false;
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-dedicatedServer")
            {
                server = true;
            }

            if (args[i] == "-port" && (i + 1 < args.Length))
            {
                _serverPort = (ushort)int.Parse(args[i + 1]);
            }

            if (args[i] == "-ip" && i + 1 < args.Length)
            {
                _externalServerIP = args[i + 1];
            }
        }

        if (server) 
        { 
            StartServer();
            await TestServerServices();
        }

        else
        {
            ClientInstance?.Invoke();
        }
    }

    private void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData
            (InternalServerIP, _serverPort);
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnect;
    }

    private async UniTask TestServerServices()
    {
        await UnityServices.InitializeAsync();

        try
        {
            _multiplayService = MultiplayService.Instance;
            await _multiplayService.StartServerQueryHandlerAsync
                (6, "TestServer", "Test", "0", "Test");
        }

        catch (Exception e)
        {
            Debug.LogError(e);
        }

        try
        {
            _payload = await GetMatchmakerPayload(600);
            if (_payload != null)
            {
                Debug.Log($"Got payload: {_payload}");
                await StartBackfill(_payload);
            }

            else
            {
                Debug.LogWarning("TimeOut Matchmaker Payload");
            }
        }

        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private async UniTask StartBackfill(MatchmakingResults payload)
    {
        var backfillProperties = new BackfillTicketProperties(payload.MatchProperties);
        _localBackfillTicket = new BackfillTicket { Id = payload.MatchProperties.BackfillTicketId, Properties = backfillProperties };

        await BeginBackfilling(payload);
    }

    private async UniTask BeginBackfilling(MatchmakingResults payload)
    {
        var matchProperties = payload.MatchProperties;

        if (string.IsNullOrEmpty(_localBackfillTicket.Id))
        {
            _createBackfillTicketOptions = new CreateBackfillTicketOptions
            {
                Connection = _externalConnectionString,
                QueueName = payload.QueueName,
                Properties = new BackfillTicketProperties(matchProperties)
            };

            _localBackfillTicket.Id
                = await MatchmakerService.Instance.CreateBackfillTicketAsync(_createBackfillTicketOptions);
        }

        _backfilling = true;
        BackfillLoop().Forget();
    }

    private async UniTaskVoid BackfillLoop()
    {
        while (_backfilling && NeedPlayers())
        {
            _localBackfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(_localBackfillTicket.Id);
            if (!NeedPlayers()) 
            {
                await MatchmakerService.Instance.DeleteBackfillTicketAsync(_localBackfillTicket.Id);
                _localBackfillTicket.Id = null;
                _backfilling = false;
                return;
            }

            await UniTask.Delay(TimeSpan.FromMilliseconds(100));
        }
        _backfilling = false;
    }

    private void ClientDisconnect(ulong clientId)
    {
        if (!_backfilling && NetworkManager.Singleton.ConnectedClients.Count > 0 && NeedPlayers())
        {
            BeginBackfilling(_payload).Forget();
        }
    }

    private bool NeedPlayers()
    {
        return NetworkManager.Singleton.ConnectedClients.Count < 10;
    }

    private async UniTask<MatchmakingResults> GetMatchmakerPayload(int timeout)
    {
        var matchmakerPayloadTask = SubscribeAndAwaitMatchmakerAllocation();

        var whatfinish = await UniTask.WhenAny(matchmakerPayloadTask, UniTask.Delay(timeout));

        if (whatfinish.hasResultLeft)
            return whatfinish.result;

        else
        {
            //cancellationtoken
            return null;
        }
    }

    private async UniTask<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
    {
        if (_multiplayService == null)
            return null;

        _allocationId = null;
        _serverCallbacks = new MultiplayEventCallbacks();
        _serverCallbacks.Allocate += OnMultiplayAllocation;

        _serverEvents = await _multiplayService.SubscribeToServerEventsAsync(_serverCallbacks);

        _allocationId = await AwaitAllocationId();
        var mmPayload = await GetMatchmakerAllocationPayloadAsync();

        return mmPayload;
    }

    private async UniTask<MatchmakingResults> GetMatchmakerAllocationPayloadAsync()
    {
        try
        {
            var payloadAllocation
                = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();
            var modelAsJson = JsonConvert.SerializeObject(payloadAllocation, Formatting.Indented);
            Debug.Log($"{nameof(GetMatchmakerAllocationPayloadAsync)}\n {modelAsJson}");

            return payloadAllocation;
        }

        catch(Exception e)
        {
            Debug.LogError($"matchmakerpayloadasync: {e}");
        }

        return null;
    }

    private async UniTask<string> AwaitAllocationId()
    {
        var config = _multiplayService.ServerConfig;
        Debug.Log("Awaiting Allocation. Server Config is:\n" +
            $"-ServerID: {config.ServerId}\n" +
            $"-AllocationID: {config.AllocationId}\n" +
            $"-Port: {config.Port}\n" +
            $"-QPort: {config.QueryPort}\n" +
            $"-logs: {config.ServerLogDirectory}");

        while (string.IsNullOrEmpty(_allocationId))
        {
            var configId = config.AllocationId;
            if (!string.IsNullOrEmpty(configId) && string.IsNullOrEmpty(_allocationId))
            {
                _allocationId = configId;
                break;
            }
        }

        await UniTask.Delay(TimeSpan.FromMilliseconds(100));

        return _allocationId;
    }

    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {
        Debug.Log($"{allocation.AllocationId} is allocation");
        if (string.IsNullOrEmpty(allocation.AllocationId)) return;

        _allocationId = allocation.AllocationId;
    }

    private void Dispose()
    {
        _serverCallbacks.Allocate -= OnMultiplayAllocation;
        _serverEvents?.UnsubscribeAsync();
    }
}