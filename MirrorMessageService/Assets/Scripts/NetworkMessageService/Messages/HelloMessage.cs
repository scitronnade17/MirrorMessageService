using Mirror;

public struct HelloMessage : NetworkMessage
{
    public string Text;
}

public struct SubscribeMessage : NetworkMessage
{
    public string messageType;
}
