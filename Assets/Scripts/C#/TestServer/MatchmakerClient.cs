using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Multiplay;
using System.Threading;
using Unity.Services.Matchmaker;
using StatusOptions = Unity.Services.Matchmaker.Models.MultiplayAssignment.StatusOptions;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
#if UNITY_EDITOR
using ParrelSync;
#endif

public class MatchmakerClient : MonoBehaviour
{
    private string _ticketId;

    private void OnEnable()
    {
        TestDedicatedServer.ClientInstance += SignIn;
    }

    private void OnDisable()
    {
        TestDedicatedServer.ClientInstance -= SignIn;
    }

    private async void SignIn()
    {
        await ClientSignIn("TestPlayer");
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async UniTask ClientSignIn(string serviceProfileName = null)
    {
        if (serviceProfileName != null)
        {
#if UNITY_EDITOR
            serviceProfileName = $"{serviceProfileName}{GetCloneNumberSuffix()}";
#endif
            var initOptions = new InitializationOptions();
            initOptions.SetProfile(serviceProfileName);
            await UnityServices.InitializeAsync(initOptions);
        }

        else
            await UnityServices.InitializeAsync();

        Debug.Log($"Signed In Anonymously as {serviceProfileName}({PlayerID()})");
    }

    private string PlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }

#if UNITY_EDITOR
    private string GetCloneNumberSuffix()
    {

        {
            string projectPath = ClonesManager.GetCurrentProjectPath();
            int lasstidx = projectPath.LastIndexOf("_");
            string projectCloneSuffix = projectPath.Substring(lasstidx + 1);

            if (projectCloneSuffix.Length != 1)
                projectCloneSuffix = "";

            return projectCloneSuffix;
        }
    }
#endif

    public void StartClient()
    {
        CreateaTicket();
    }

    private async void CreateaTicket()
    {
        var options = new CreateTicketOptions("TestQueue");

        var players = new List<Unity.Services.Matchmaker.Models.Player>
        {
            new Unity.Services.Matchmaker.Models.Player(
                PlayerID()
            )
        };

        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
        _ticketId = ticketResponse.Id;
        Debug.Log($"Ticket Id: {_ticketId}");
        PollTicketStatus();
    }

    private async void PollTicketStatus()
    {
        MultiplayAssignment multiplayAssignment = null;
        bool gotAssignment = false;
        do
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(_ticketId);
            if (ticketStatus == null) continue;
            if (ticketStatus.Type == typeof(MultiplayAssignment))
            {
                multiplayAssignment = ticketStatus.Value as MultiplayAssignment;
            }

            switch (multiplayAssignment.Status)
            {
                case StatusOptions.Found:
                    gotAssignment = true;
                    TicketAssigned(multiplayAssignment);
                    break;

                case StatusOptions.InProgress:
                    break;

                case StatusOptions.Failed:
                    gotAssignment = true;
                    Debug.LogError($"Failed to got ticket status. {multiplayAssignment.Message}");
                    break;

                case StatusOptions.Timeout:
                    gotAssignment = true;
                    Debug.LogError($"Failed to got ticket status with time out. {multiplayAssignment.Message}");
                    break;

                default:
                    throw new InvalidOperationException();
            }

        } while (!gotAssignment);
    }

    private void TicketAssigned(MultiplayAssignment assignment)
    {
        Debug.Log($"TicketAssigned: {assignment.Ip}: {assignment.Port}");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(assignment.Ip, (ushort)assignment.Port);
        NetworkManager.Singleton.StartClient();
}
}