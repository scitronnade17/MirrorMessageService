using System;
using UnityEngine;

public sealed class ClientMessageReceiver : IDisposable
{
    private readonly INetworkMessageService messageService;

    public ClientMessageReceiver(INetworkMessageService messageService)
    {
        this.messageService = messageService;
        messageService.Subscribe<HelloMessage>(OnHelloReceived);
        Debug.Log($"3. Client subscribe to HelloMessage");
    }

    private void OnHelloReceived(HelloMessage msg)
    {
        Debug.Log($"6. Client get HelloMessage: {msg.Text}");
    }

    public void Dispose()
    {
        messageService.Unsubscribe<HelloMessage>(OnHelloReceived);
    }
}