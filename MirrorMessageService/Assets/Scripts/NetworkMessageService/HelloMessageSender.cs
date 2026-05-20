using Mirror;
using UnityEngine;
using Zenject;

public class HelloMessageSender : MonoBehaviour
{
    private INetworkMessageService messageService;

    [Inject]
    public void Construct(INetworkMessageService _messageService)
    {
        messageService = _messageService;
    }

    private void OnEnable()
    {
        messageService.OnClientSubscribed += OnClientSubscribed;
    }

    private void OnClientSubscribed(NetworkConnectionToClient conn, string messageType)
    {
        messageService.Send(new HelloMessage { Text = "Hello Client!" });

        Debug.Log($"5. Server send to client {conn.connectionId}: {messageType}");
    }

    private void OnDisable()
    {
        messageService.OnClientSubscribed -= OnClientSubscribed;
    }
}