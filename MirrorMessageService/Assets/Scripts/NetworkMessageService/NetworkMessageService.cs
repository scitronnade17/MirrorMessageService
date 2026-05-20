using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public interface INetworkMessageService
{
    event Action<NetworkConnectionToClient, string> OnClientSubscribed;
    void Initialize();
    void OnClientConnected();
    void Subscribe<T>(Action<T> handler) where T : struct, NetworkMessage;
    void Unsubscribe<T>(Action<T> handler) where T : struct, NetworkMessage;
    void Send<T>(T message) where T : struct, NetworkMessage;
    void Dispose();

}

public class NetworkMessageService : INetworkMessageService
{
    private readonly Dictionary<int, HashSet<string>> subscribers = new();
    private readonly Dictionary<string, List<object>> clientHandlers = new();

    public event Action<NetworkConnectionToClient, string> OnClientSubscribed;

    public void Initialize()
    {
        NetworkServer.RegisterHandler<SubscribeMessage>(OnServerSubscribe);
    }

    private void OnServerSubscribe(NetworkConnectionToClient conn, SubscribeMessage msg)
    {
        if (!subscribers.TryGetValue(conn.connectionId, out var msgs))
        {
            msgs = new HashSet<string>();
            subscribers[conn.connectionId] = msgs;
        }
        msgs.Add(msg.messageType);
        Debug.Log($"4. Server get info: client {conn.connectionId} subscribed to {msg.messageType}");

        OnClientSubscribed?.Invoke(conn, msg.messageType);
    }

    public void OnClientConnected()
    {
        foreach (var typeKey in clientHandlers.Keys)
        {
            NetworkClient.Send(new SubscribeMessage { messageType = typeKey });
        }
    }

    public void Subscribe<T>(Action<T> handler) where T : struct, NetworkMessage
    {
        string typeKey = typeof(T).FullName;

        if (!clientHandlers.ContainsKey(typeKey))
        {
            clientHandlers[typeKey] = new List<object>();

            NetworkClient.ReplaceHandler<T>(mirrorMsg =>
            {
                if (!clientHandlers.TryGetValue(typeKey, out var handlers)) return;

                foreach (var handler in new List<object>(handlers))
                    ((Action<T>)handler).Invoke(mirrorMsg);
            });
        }

        clientHandlers[typeKey].Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler) where T : struct, NetworkMessage
    {
        string typeKey = typeof(T).FullName;
        if (clientHandlers.TryGetValue(typeKey, out var handlers))
            handlers.Remove(handler);
    }

    public void Send<T>(T message) where T : struct, NetworkMessage
    {
        string typeKey = typeof(T).FullName;

        foreach (var (connId, messageTypes) in subscribers)
        {
            if (!messageTypes.Contains(typeKey)) continue;

            if (NetworkServer.connections.TryGetValue(connId, out var conn))
                conn.Send(message);
        }
    }

    public void Dispose()
    {
        NetworkServer.UnregisterHandler<SubscribeMessage>();
        subscribers.Clear();
        clientHandlers.Clear();
    }
}