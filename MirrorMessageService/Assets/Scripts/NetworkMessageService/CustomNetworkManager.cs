using Mirror;
using UnityEngine;
using Zenject;

public class CustomNetworkManager : NetworkManager
{
    private ClientMessageReceiver receiver;
    private INetworkMessageService messageService;

    [Inject]
    public void Construct(INetworkMessageService _messageService)
    {
        messageService = _messageService;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("1. Server start");
        messageService.Initialize();
    }

    public override void OnClientConnect()
    {
        Debug.Log("2. Client connect");
        receiver = new ClientMessageReceiver(messageService);
        messageService.OnClientConnected();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        messageService.Dispose();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        receiver?.Dispose();
        receiver = null;
    }
}